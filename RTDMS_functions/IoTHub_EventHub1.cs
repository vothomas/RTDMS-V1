using IotHubTrigger = Microsoft.Azure.WebJobs.EventHubTriggerAttribute;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Azure.Messaging.EventHubs;
using Microsoft.Extensions.Logging;

using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Net.Http;

namespace viceroy
{
    public class IoTHub_EventHub1
    {
        static int temperatureThreshold = 80;
        private static HttpClient client = new HttpClient();
        static int minimumEmailInterval = 3600000; // minimum milliseconds between emails, no more than 30/day for IFTTT free tier
        static DateTime nextEmailTime = new DateTime();
        public record ShortTelem(string deviceId, double temperature, double pressure); // IFTTT limits JSON payloads to 3 items
        static string iftttUrl = System.Environment.GetEnvironmentVariable("https://maker.ifttt.com/trigger/temp_threshold/json/with/key/d5l_9YkBlKTsdMIx8uthtc", EnvironmentVariableTarget.Process);

        [FunctionName("IoTHub_EventHub1")]
        public static async Task Run([IotHubTrigger("messages/events", Connection = "ConnectionString")] EventData message, ILogger log)
        {
            var jsonPayload = Encoding.UTF8.GetString(message.Body.ToArray());
            // capture the telemetry in an instance of the telemetry model variable
            // nullable to suppress warning CS8632
            #nullable enable
            Telem? telemetryDataPoint = JsonSerializer.Deserialize<Telem>(jsonPayload);
            #nullable disable

            // check whether current temp is above threshold and past minimum email time
            if (telemetryDataPoint.temperature > temperatureThreshold && DateTime.Now > nextEmailTime)
            {
                await SendEmailNotification(telemetryDataPoint, log);
                System.Diagnostics.Process.Start("https://maker.ifttt.com/trigger/temp_threshold/json/with/key/d5l_9YkBlKTsdMIx8uthtc");
            }
        }

        public static async Task SendEmailNotification(Telem abnormalDataPoint, ILogger log)
        {
            // send POST request with telemetry as payload
            // https://learn.microsoft.com/en-us/dotnet/fundamentals/networking/http/httpclient
            var response = await client.PostAsJsonAsync(
                iftttUrl,
                new ShortTelem(
                    abnormalDataPoint.deviceId,
                    abnormalDataPoint.temperature,
                    abnormalDataPoint.pressure
                )
            );

            // check response code of IFTTT call
            if (response.IsSuccessStatusCode)
            {
                // do not send anotheat least minimumEmailInterval milliseconds (3600000ms == 1hr)
                nextEmailTime = DateTime.Now.AddMilliseconds(minimumEmailInterval);
                var nextEmailString = nextEmailTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
                log.LogInformation($"Successfully sent webhook to IFTTT. Next email at {nextEmailString}.");
            }
            else
            {
                // do not attempt another IFTTT call for one minute
                nextEmailTime = DateTime.Now.AddMinutes(1);
                var nextEmailString = nextEmailTime.ToString(System.Globalization.CultureInfo.InvariantCulture);
                log.LogError($"IFTTT webhook failed. Retrying at {nextEmailString}.");
            }
        }
    }
}