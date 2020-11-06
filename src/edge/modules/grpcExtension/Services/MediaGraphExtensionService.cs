// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

using Grpc.Core;
using GrpcExtension.Core;
using GrpcExtension.Processors;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace GrpcExtension
{
    public class MediaGraphExtensionService : MediaGraphExtension.MediaGraphExtensionBase, IDisposable
    {
        private readonly ILogger _logger;
        private readonly int _batchSize;
        private readonly BatchImageProcessor _imageProcessor;
        private MemoryMappedFileMemoryManager<byte> _memoryManager;
        private MediaDescriptor _clientMediaDescriptor;
        private Memory<byte> _memory;

        public MediaGraphExtensionService(ILogger logger, int batchSize)
        {
            _logger = logger;
            _batchSize = batchSize;
            _imageProcessor = new BatchImageProcessor(_logger);
        }

        public async override Task ProcessMediaStream(IAsyncStreamReader<MediaStreamMessage> requestStream, IServerStreamWriter<MediaStreamMessage> responseStream, ServerCallContext context)
        {
            //First message from the client is (must be) MediaStreamDescriptor
            _ = await requestStream.MoveNext();
            var requestMessage = requestStream.Current;
            _logger.LogInformation($"[Received MediaStreamDescriptor] SequenceNum: {requestMessage.SequenceNumber}");
            var response = ProcessMediaStreamDescriptor(requestMessage.MediaStreamDescriptor);
            
            var responseMessage = new MediaStreamMessage()
            {
                MediaStreamDescriptor = response
            };
            
            await responseStream.WriteAsync(responseMessage);

            // Process rest of the MediaStream message sequence
            var height = (int)requestMessage.MediaStreamDescriptor.MediaDescriptor.VideoFrameSampleFormat.Dimensions.Height;
            var width = (int)requestMessage.MediaStreamDescriptor.MediaDescriptor.VideoFrameSampleFormat.Dimensions.Width;
            ulong responseSeqNum = 0;
            int messageCount = 1;
            List<Image> imageBatch = new List<Image>();
            while (await requestStream.MoveNext())
            {
                // Extract message IDs
                requestMessage = requestStream.Current;
                var requestSeqNum = requestMessage.SequenceNumber;
                _logger.LogInformation($"[Received MediaSample] SequenceNum: {requestSeqNum}");

                // Retrieve the sample content
                ReadOnlyMemory<byte> content = null;
                var inputSample = requestMessage.MediaSample;

                switch (inputSample.ContentCase)
                {
                    case MediaSample.ContentOneofCase.ContentReference:

                        content = _memory.Slice(
                            (int)inputSample.ContentReference.AddressOffset,
                            (int)inputSample.ContentReference.LengthBytes);

                        break;

                    case MediaSample.ContentOneofCase.ContentBytes:
                        content = inputSample.ContentBytes.Bytes.ToByteArray();
                        break;
                }

                var mediaStreamMessageResponse = new MediaStreamMessage()
                {
                    SequenceNumber = ++responseSeqNum,
                    AckSequenceNumber = requestSeqNum
                };

                imageBatch.Add(GetImageFromContent(content, width, height));

                // If batch size hasn't been reached
                if (messageCount < _batchSize)
                {
                    // Return acknowledge message
                    mediaStreamMessageResponse.MediaSample = new MediaSample();
                    await responseStream.WriteAsync(mediaStreamMessageResponse);
                    messageCount++;
                    continue;
                }

                foreach (var inference in inputSample.Inferences)
                {
                    NormalizeInference(inference);
                }

                // Process images
                var inferencesResponse = _imageProcessor.ProcessImages(imageBatch);
                var mediaSampleResponse = new MediaSample()
                {
                    Inferences = { inferencesResponse }
                };

                mediaStreamMessageResponse.MediaSample = mediaSampleResponse;

                await responseStream.WriteAsync(mediaStreamMessageResponse);
                imageBatch.Clear();
                messageCount = 1;
            }
        }

        private Image GetImageFromContent(ReadOnlyMemory<byte> content, int width, int height)
        {
            var imageBytes = content.ToArray();
            var region = new System.Drawing.Rectangle(0, 0, width, height);
            var bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
            BitmapData bitmapData = bitmap.LockBits(region, ImageLockMode.WriteOnly, PixelFormat.Format24bppRgb);

            var length = Math.Abs(bitmapData.Stride) * height;

            Marshal.Copy(imageBytes, 0, bitmapData.Scan0, length);

            bitmap.UnlockBits(bitmapData);

            return bitmap;
        }

        private static void NormalizeInference(Inference inference)
        {
            if (inference.ValueCase == Inference.ValueOneofCase.None)
            {
                // Note: This will terminate the RPC call. This is okay because the inferencing engine has broken the
                // RPC contract.
                throw new RpcException(
                    new Status(
                        StatusCode.InvalidArgument,
                        $"Inference has no value case set. Inference type: {inference.Type} Inference subtype: {inference.Subtype}"));
            }

            // If the type is auto, this will overwrite it to the correct type.
            // If the type is correctly set, this will be a no-op.
            var inferenceType = inference.ValueCase switch
            {
                Inference.ValueOneofCase.Classification => Inference.Types.InferenceType.Classification,
                Inference.ValueOneofCase.Motion => Inference.Types.InferenceType.Motion,
                Inference.ValueOneofCase.Entity => Inference.Types.InferenceType.Entity,
                Inference.ValueOneofCase.Text => Inference.Types.InferenceType.Text,
                Inference.ValueOneofCase.Event => Inference.Types.InferenceType.Event,
                Inference.ValueOneofCase.Other => Inference.Types.InferenceType.Other,

                _ => throw new ArgumentException($"Inference has unrecognized value case {inference.ValueCase}"),
            };

            inference.Type = inferenceType;
        }


        /// Process the Media Stream session preamble.
        /// </summary>
        /// <param name="mediaStreamDescriptor">Media session preamble.</param>
        /// <returns>Preamble response.</returns>
        public MediaStreamDescriptor ProcessMediaStreamDescriptor(MediaStreamDescriptor mediaStreamDescriptor)
        {
            // Setup data transfer
            switch (mediaStreamDescriptor.DataTransferPropertiesCase)
            {
                case MediaStreamDescriptor.DataTransferPropertiesOneofCase.SharedMemoryBufferTransferProperties:

                    var memoryMappedFileProperties = mediaStreamDescriptor.SharedMemoryBufferTransferProperties;

                    // Create a view on the memory mapped file.
                    _logger.LogInformation($"Using shared memory transfer. Handle: {memoryMappedFileProperties.HandleName}, Size:{memoryMappedFileProperties.LengthBytes}");

                    try
                    {
                        _memoryManager = new MemoryMappedFileMemoryManager<byte>(
                        memoryMappedFileProperties.HandleName,
                        (int)memoryMappedFileProperties.LengthBytes,
                        desiredAccess: MemoryMappedFileAccess.Read);

                        _memory = _memoryManager.Memory;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error creating memory manager: {ex.Message}");

                        throw new RpcException(
                            new Status(
                                StatusCode.InvalidArgument,
                                $"Error creating memory manager: {ex.Message}"));
                    }

                    break;

                case MediaStreamDescriptor.DataTransferPropertiesOneofCase.None:
                    // Nothing to be done.
                    _logger.LogInformation("Using embedded frame transfer.");
                    break;

                default:
                    _logger.LogInformation($"Unsupported data transfer method: {mediaStreamDescriptor.DataTransferPropertiesCase}");
                    throw new RpcException(new Status(StatusCode.OutOfRange, $"Unsupported data transfer method: {mediaStreamDescriptor.DataTransferPropertiesCase}"));
            }

            // Validate encoding
            if (!_imageProcessor.IsMediaFormatSupported(mediaStreamDescriptor.MediaDescriptor, out var errorMessage))
            {
                _logger.LogInformation($"validate enconding: {errorMessage}");
                throw new RpcException(new Status(StatusCode.OutOfRange, errorMessage));
            }

            // Cache the client media descriptor for this stream
            _clientMediaDescriptor = mediaStreamDescriptor.MediaDescriptor;

            // Return a empty server stream descriptor as no samples are returned (only inferences)
            return new MediaStreamDescriptor { MediaDescriptor = new MediaDescriptor { Timescale = _clientMediaDescriptor.Timescale } };
        }

        public void Dispose()
        {
            ((IDisposable)_memoryManager)?.Dispose();
        }
    }
}