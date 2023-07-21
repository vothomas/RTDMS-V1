using IoTHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Azure.Messaging.EventHubs;
using System.Text;
using System.Net.Http;
using Microsoft.Extensions.Logging;

namespace Viceroy.Funcs
{
    public class IoTHub_EventHub1
    {
        private static HttpClient client = new HttpClient();
        
        [FunctionName("IoTHub_EventHub1")]
        public void Run([IoTHubTrigger("messages/events", Connection = "HubConnection")]EventData message, ILogger log)
        {
            log.LogInformation($"C# IoT Hub trigger function processed a message: {Encoding.UTF8.GetString(message.Body.ToArray())}");
        }
    }
}