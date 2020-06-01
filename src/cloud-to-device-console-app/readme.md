# LVA cloud to device sample console app

This directory contains a dotnet core sample app that would enable you to invoke LVA on IoT Edge Direct Methods in a sequence and with parameters, defined by you in a JSON file (operations.json)

## Contents

| File/folder             | Description                                                   |
|-------------------------|---------------------------------------------------------------|
| `c2d-console-app.csproj`| Project file.                                                 |
| `.gitignore`            | Define what to ignore at commit time.                         |
| `README.md`             | This README file.                                             |
| `operations.json`       | JSON file defining the sequence of operations to execute upon.|
| `Program.cs`            | The main program file                                         |

## Setup

Create a file named appsettings.json in this folder. Add the following text and provide values for all parameters.

```JSON
{
    "IoThubConnectionString" : "",
    "deviceId" : "",
    "moduleId" : ""
}
```

* **IoThubConnectionString** - Refers to the connection string of your IoT hub
* **deviceId** - Refers to your IoT Edge device id (registered with your IoT hub)
* **moduleId** - Refers to the module id of LVA on IoT Edge module (when deployed to the IoT Edge device)

Running [LVA resources setup script](https://github.com/Azure/live-video-analytics/tree/master/edge/setup), generates **appsettings.json** file with values pre-filled by the script.

Create a file named .env in src/edge folder and add the following text to it. Provide values for all variables.

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

Running [LVA resources setup script](https://github.com/Azure/live-video-analytics/tree/master/edge/setup) generates the **.env** file with values pre-filled by the script.

## Running the sample

Detailed instructions for running the sample can be found in the tutorials for LVA on IoT Edge. Below is a summary of key steps.

* Right click on src/edge/deployment.template.json and select **“Generate Iot Edge deployment manifest”**. This will create an IoT Edge deployment manifest file in src/edge/config folder named deployment.amd64.json.
* Right click on src/edge/config /deployment.amd64.json and select **"Create Deployment for single device"** and select the name of your edge device. This will trigger the deployment of the IoT Edge modules to your Edge device. You can view the status of the deployment in the Azure IoT Hub extension (expand 'Devices' and then 'Modules' under your IoT Edge device).
* Right click on your edge device in Azure IoT Hub extension and select **"Start Monitoring Built-in Event Endpoint"**.
* Start a debugging session (hit F5). You will start seeing some messages printed in the TERMINAL window. In the OUTPUT window, you will see messages that are being sent to the IoT Hub, by the lvaEdge module.

## Next steps

Experiment with different sequence of Direct Method calls by modifying operations.json.
