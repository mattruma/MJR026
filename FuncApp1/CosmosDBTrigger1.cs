using FuncApp1.Helpers;
using Microsoft.Azure.Documents;
using Microsoft.Azure.EventGrid;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace FuncApp1
{
    public static class CosmosDBTrigger1
    {
        private static HttpClient _httpClient;

        [FunctionName("CosmosDBTrigger1")]
        public static async Task Run([CosmosDBTrigger(
            databaseName: "%AzureCosmosDb:DatabaseName%",
            collectionName: "%AzureCosmosDb:CollectionName%",
            MaxItemsPerInvocation = 10,
            ConnectionStringSetting = "AzureCosmosDb:ConnectionString",
            CreateLeaseCollectionIfNotExists = true,
            LeaseCollectionName = "%AzureCosmosDb:LeaseCollectionName%")]IReadOnlyList<Document> documents,
            ILogger logger,
            [Blob("cosmosdbtrigger1-errors", FileAccess.Write, Connection = "AzureStorage:ConnectionString")] CloudBlobContainer blobContainer)
        {
            logger.LogInformation("CosmosDBTrigger1 function triggered.");

            if (_httpClient == null)
            {
                _httpClient =
                    new HttpClient(new HttpRetryMessageHandler(logger, new HttpClientHandler()));
            }

            if (documents != null)
            {
                logger.LogInformation($"Received {documents.Count} document(s) from Cosmos DB.");

                var eventGridEventList =
                    new List<EventGridEvent>();

                var topicCredentials =
                    new TopicCredentials(
                        Environment.GetEnvironmentVariable("AzureEventGrid:TopicKey"));

                var eventGridClient =
                    new EventGridClient(
                        topicCredentials, _httpClient, false);

                foreach (var document in documents)
                {
                    var eventGridEvent =
                        new EventGridEvent()
                        {
                            Id = Guid.NewGuid().ToString(),
                            Subject = $"/persons/{document.GetPropertyValue<Guid>("person_id")}",
                            EventType = "Person.Added",
                            Data = document,
                            EventTime = DateTime.Now,
                            DataVersion = "1.0"
                        };

                    eventGridEventList.Add(
                        eventGridEvent);
                }

                try
                {
                    logger.LogInformation(
                        $"Sending a batch of {documents.Count} event(s) to Azure Event Grid for processing.");

                    await eventGridClient.PublishEventsAsync(
                       new Uri(Environment.GetEnvironmentVariable("AzureEventGrid:TopicEndpoint")).Host,
                       eventGridEventList);
                }
                catch
                {
                    logger.LogInformation(
                        $"Logging a batch of {documents.Count} event(s) to Azure Blob Storage that were not able to be sent to Azure Event Grid.");

                    var cloudBlockBlob =
                        blobContainer.GetBlockBlobReference($"{Guid.NewGuid()}.json");

                    cloudBlockBlob.Properties.ContentType =
                        "application/json";

                    await cloudBlockBlob.UploadTextAsync(
                        JsonConvert.SerializeObject(eventGridEventList, Formatting.Indented));

                    throw;
                }
            }
        }
    }
}
