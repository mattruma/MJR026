// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace FuncApp1
{
    public static class EventGridTrigger1
    {
        [FunctionName("EventGridTrigger1")]
        public static void Run(
            [EventGridTrigger]EventGridEvent eventGridEvent,
            ILogger logger)
        {
            logger.LogInformation("EventGridTrigger1 function triggered.");

            logger.LogInformation(eventGridEvent.Data.ToString());
        }
    }
}
