
:bangbang: | No new features are being added here. For the latest verison of the service, see [Azure Video Analyzer](https://github.com/Azure/video-analyzer) and the [Csharp sample](https://github.com/Azure-Samples/video-analyzer-iot-edge-csharp)
:---: | :---

---
page_type: sample
languages:
  - csharp
products:
  - azure
  - azure-media-services
  - azure-live-video-analytics
description: "The samples in this repo show how to use the Live Video Analytics on IoT Edge module to analyze live video using AI modules of your choice and optionally record video on the edge device or in the cloud."  
---

# Live Video Analytics on IoT Edge samples

This repository contains C# samples for Live Video Analytics on IoT Edge

## Contents

| File/folder       | Description                                |
|----------------------|--------------------------------------------|
| `src`                | Sample source code.                        |
| `.gitignore`         | Defines what to ignore at commit time.     |
| `README.md`          | This README file.                          |
| `LICENSE`            | The license for the sample.                |
| `SECURITY`           | Guidelines for reporting security issues   |
| `CODE_OF_CONDUCT.md` | Open source code of conduct                |

The 'src' folder contains three sub-folders

* **cloud-to-device-console-app** - This folder contains a dotnet core console app that enables you to invoke direct methods of Live Video Analytics on IoT Edge module, with parameters defined by you in a JSON file (operations.json).
* **edge** - This folder has a few IoT Edge deployment manifest templates, along with sample code for an IoT Edge module (under 'modules' folder) that can be used in conjunction with the Live Video Analytics on IoT Edge module.
* **ams-asset-player** - This folder contains a ASP dotnet core app that showcases how you can playback assets recorded by Live Video Analytics on IoT Edge (in the Azure Media Services account referenced in the module twin properties).

## Prerequisites

1. An active Azure subscription
2. Azure resources deployed in the Azure subscription

    a. IoT Hub

    b. Storage Account

    c. Media Services

    d. Azure container registry

3. A Linux edge device with [IoT Edge runtime](https://docs.microsoft.com/en-us/azure/iot-edge/how-to-install-iot-edge-linux)

4. [Visual Studio Code](https://code.visualstudio.com/) on your development machine with following extensions

    a. [Azure IoT Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools)

    b. [C#](https://marketplace.visualstudio.com/items?itemName=ms-dotnettools.csharp)

5. [.NET Core 3.1 SDK](https://dotnet.microsoft.com/download/dotnet-core/3.1) on your development machine

6. [Docker](https://docs.docker.com/engine/install/) on your development machine

You can use the [LVA resources setup script](https://github.com/Azure/live-video-analytics/tree/master/edge/setup) to deploy the Azure resources mentioned above, along with an Azure Linux VM to serve as your IoT Edge device.

## Setup

After cloning the repository, follow instructions outlined in **src/cloud-to-device-console-app/readme.md** to setup the console app and instructions in **src/ams-asset-player/readme.md** to setup the ASP dotnet core app.

## Running the sample

Follow instructions outlined in **src/cloud-to-device-console-app/readme.md** to run the console app and instructions in **src/ams-asset-player/readme.md** to run the ASP dotnet core app.

## Key concepts

Read [Live Video Analytics on IoT Edge concepts](https://docs.microsoft.com/en-us/azure/media-services/live-video-analytics-edge/overview)

## Code of conduct

This project has adopted the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/). For more information see the [Code of Conduct FAQ](https://opensource.microsoft.com/codeofconduct/faq/) or
contact [opencode@microsoft.com](mailto:opencode@microsoft.com) with any additional questions or comments.
