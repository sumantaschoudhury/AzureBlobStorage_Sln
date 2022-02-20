using System;
using System.IO;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace AzureBlobTriggerFunction
{

    public static class BlobTriggerFunction
    {
        [FunctionName("BlobTriggerFunction")]
        public static async Task Run([BlobTrigger("sounakblobctr/{name}", Connection = "AzureWebJobsStorage")]Stream myBlob, string name, ILogger log)
        {
            log.LogInformation($"C# Blob trigger function Processed blob\n Name:{name} \n Size: {myBlob.Length} Bytes");

            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();

            /// The Azure Cosmos DB endpoint for running this GetStarted sample.
            string EndpointUrl = config.GetSection("EndpointUrl").Value;

            /// The primary key for the Azure DocumentDB account.
            string PrimaryKey = config.GetSection("PrimaryKey").Value;

            // The Cosmos client instance
            CosmosClient cosmosClient = new CosmosClient(EndpointUrl, PrimaryKey);

            // The database we will create
            Database database;

            // The container we will create.
            Container container;

            // The name of the database and container we will create
            string databaseId = "sounakdb";
            string containerId = "sounakcntr";

            var blobFile = new BlobFile { FileName = name, ImageId = "One", id = "one" };
            ////var order = new Order { OrderCode = "O5", OrderId = "five", id = "five" };

            //if (message == null)
            //    throw new ArgumentNullException(nameof(message));

            //var body = Encoding.UTF8.GetString(message.Body);
            //var order = Newtonsoft.Json.JsonConvert.DeserializeObject<Order>(body);
            //order.id = order.OrderId;

            //Console.WriteLine($"Create Order Details are: {body}");


            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseId);
            container = await database.CreateContainerIfNotExistsAsync(containerId, "/ImageId");
            ItemResponse<BlobFile> orderResponse = await container.CreateItemAsync<BlobFile>(blobFile, new PartitionKey(blobFile.ImageId));
            // Note that after creating the item, we can access the body of the item with the Resource property of the ItemResponse. We can also access the RequestCharge property to see the amount of RUs consumed on this request.
            Console.WriteLine("Created item in database with id: {0} Operation consumed {1} RUs.\n", orderResponse.Resource.ImageId, orderResponse.RequestCharge);

        }
    }

    internal class BlobFile
    {
        public string FileName { get; set; }
        public string ImageId { get; set; }
        public string id { get; set; }
    }
}
