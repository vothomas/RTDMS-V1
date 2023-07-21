// Description: Interim (version 1) IoT bootcamp development project (Remote Data Center Monitoring) prototype
// Skill Level:  university engineering or comp sci ( sophomore and higher)

// IoTHubClient is a helper/utility class for connecting to the Azure IoT hub.

#nullable disable

using Microsoft.Azure.Devices.Client;
// using Newtonsoft.Json;
using System.Diagnostics;
using System.Text.Json;

namespace viceroy
{
    public class IotHubClient : IDisposable
    {
        private DeviceClient deviceClient;
        private readonly RDTMS_Settings settings_;
        private RTDMS_Driver _driver;

        public bool Connected {get; set;}

        public IotHubClient(RDTMS_Settings settings, TransportType tt, RTDMS_Driver driver)
        {
            settings_ = settings;
            Connected = false;
            deviceClient = DeviceClient.CreateFromConnectionString(settings_.ConnectionString, tt);
            _driver = driver;
        }

        public void Start()
        {
            // Source: https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-dev-guide-sas?tabs=node
            //         https://learn.microsoft.com/en-us/azure/iot-hub/iot-hub-mqtt-support

            if (!Connected)
            {
                // open the connection to the Azure IoT Hub
                deviceClient.OpenAsync().Wait();

                deviceClient.SetMethodHandlerAsync("ControlRelay", ControlRelay, null).Wait();

                //on and off (boolean) message type:
                //await deviceClient.SetMethodHandlerAsync("ControlLED", ControlLED, null);
                Connected = true;
            }
        }

        public void Close()
        {
            if (Connected)
            {
                deviceClient.CloseAsync().Wait();
                Connected = false;
            }
        }

        private Task<MethodResponse> ControlRelay(MethodRequest methodRequest, object userContext)
        {
            Console.WriteLine(String.Format("method ControlRelay: {0}", methodRequest.DataAsJson));
            try
            {
                // OnOffMethodData m = JsonConvert.DeserializeObject<OnOffMethodData>(methodRequest.DataAsJson);
                OnOffMethodData status = JsonSerializer.Deserialize<OnOffMethodData>(methodRequest.DataAsJson);
                _driver.External_HVAC(status.onoff, (status.onoff) ? "HVAC Remote On" : "HVAC_Remote Off");
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"{ex.Message}");
                //   this.callMeLogger(String.Format("Wrong message: {0}", methodRequest.DataAsJson));
                return Task.FromResult(new MethodResponse(400));
            }
            //this.callMeLogger(methodRequest.DataAsJson);
            return Task.FromResult(new MethodResponse(200));
        }

        public async Task SendDeviceToCloudMessagesAsync(Message message)
        {
            await deviceClient.SendEventAsync(message);
        }

        public void Dispose()
        {
            deviceClient.Dispose();
        }
    }

     class OnOffMethodData
    {
        public bool onoff { get; set; }
    }
}