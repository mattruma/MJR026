using Bogus;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace FuncApp1
{
    public static class HttpTrigger1
    {
        public const int MAXIMUM_RECORDS = 10;

        [FunctionName("HttpTrigger1")]
        [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Nothing needs to be passed")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger logger,
            [CosmosDB(
                databaseName: "%AzureCosmosDb:DatabaseName%",
                collectionName: "%AzureCosmosDb:CollectionName%",
                ConnectionStringSetting = "AzureCosmosDb:ConnectionString")]
                IAsyncCollector<Person> persons)
        {
            logger.LogInformation("HttpTrigger1 function triggered.");

            for (var i = 0; i < MAXIMUM_RECORDS; i++)
            {
                await persons.AddAsync(
                    new Faker<Person>()
                        .RuleFor(o => o.Id, f => Guid.NewGuid())
                        .RuleFor(o => o.BirthDate, f => f.Person.DateOfBirth)
                        .RuleFor(o => o.Avatar, f => f.Person.Avatar)
                        .RuleFor(o => o.FirstName, f => f.Person.FirstName)
                        .RuleFor(o => o.LastName, f => f.Person.LastName)
                        .RuleFor(o => o.Phone, f => f.Person.Phone)
                        .RuleFor(o => o.UserName, f => f.Person.UserName)
                        .RuleFor(o => o.Email, f => f.Person.Email)
                    .Generate());
            }

            logger.LogInformation($"Inserted {MAXIMUM_RECORDS} document(s) into Cosmos DB.");

            return new OkResult();
        }
    }
}
