# IoT Edge sample solution

This folder contains IoT Edge deployment manifest templates and sample IoT Edge modules.

## Deployment manifest templates

### deployment.template.json

This file is a deployment manifest template that has the following modules defined in it

* rtspsim - [RTSP simulator module](https://github.com/Azure/live-video-analytics/tree/master/utilities/rtspsim-live555)
* lvaEdge - Live video analytics on IoT Edge module

### deployment.yolov3.template.json

Besides the modules already defined in deployment.template.json, this deployment manifest template references the [yolov3 module](https://github.com/Azure/live-video-analytics/tree/master/utilities/video-analysis/yolov3-onnx) that hosts the YoloV3 ONNX model with http server.

### deployment.objectCounter.template.json

Besides the modules already defined in deployment.yolov3.template.json, this deployment manifest template references the sample objectCounter module (source code for which can be found in ./modules/objectCounter). This template also has message routes defined to send messages from the lvaEdge module to the objectCounter module and vice versa, to enable the scenario of recording video clips when objects of a specified type and above a specified threshold value are found.

## Deployment manifest template variables

The deployment manifest templates contains several variables (look for '$' symbol). The values for these variables need to be specified in the .env file, which like this

```env
SUBSCRIPTION_ID=""
RESOURCE_GROUP=""
AMS_ACCOUNT=""
IOTHUB_CONNECTION_STRING=""
AAD_TENANT_ID=""
AAD_SERVICE_PRINCIPAL_ID=""
AAD_SERVICE_PRINCIPAL_SECRET=""
INPUT_VIDEO_FOLDER_ON_DEVICE=""
OUTPUT_VIDEO_FOLDER_ON_DEVICE=""
APPDATA_FOLDER_ON_DEVICE=""
CONTAINER_REGISTRY_USERNAME_myacr=""
CONTAINER_REGISTRY_PASSWORD_myacr=""
```

To generate a deployment manifest from the template, open your local clone of this git repository in Visual Studio Code, have the [Azure Iot Tools](https://marketplace.visualstudio.com/items?itemName=vsciot-vscode.azure-iot-tools) extension installed, right click on the template file and select "Generate IoT Edge deployment manifest". This will create the corresponding deployment manifest file in **./config** folder.

## Sample edge modules

### objectCounter

The folder **./modules/objectCounter** contains source code for an IoT Edge module that counts objects of a specified type and above a specified threshold value (these are specified as twin properties in deployment.objectCounter.template.json). The module expects messages emitted by yolov3 module (referenced above).

## Learn more

* [Develop IoT Edge modules](https://docs.microsoft.com/en-us/azure/iot-edge/tutorial-develop-for-linux)
