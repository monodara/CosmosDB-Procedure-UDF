using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Scripts;
using Microsoft.Extensions.Configuration;

internal class Program
{
    private static string URI = "https://cosmosdb-for-peacoplaza.documents.azure.com:443/";

    private static string ProductContainer = "ProductCatalog";
    private static string DbName = "PeacoPlaza-Store";
    private static Container container;
    private static async Task Main(string[] args)
    {
        // 
        // Build configuration
        IConfiguration config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();
        var COSMOS_KEY = config["CosmosKey"];


        CosmosClient client = new CosmosClient(URI, COSMOS_KEY);//create a client
        // Create a database
        // An object containing relevant information about the response
        Database database = await client.CreateDatabaseIfNotExistsAsync(DbName);
        // Create a container
        // Set throughput to the minimum value of 400 RU/s
        container = await database.CreateContainerIfNotExistsAsync(
            ProductContainer,
            "/categoryId" //logical partition
            );

        var product = new
        {
            id = Guid.NewGuid(),
            name = "IPhone 14 while",
            categoryId = "23",
            price = 1099.99,
        };


        // await CreateProductAsync(product);
        // await ReadProductAsync();

        // Save procedure into database
        var createProcedure = new StoredProcedureProperties{
            Id = "createProduct",
            Body = File.ReadAllText("productCreation.js"),
        };
        await container.Scripts.CreateStoredProcedureAsync(createProcedure);
        // Execute the procedure
        var response = await container.Scripts.ExecuteStoredProcedureAsync<dynamic>("createProduct", new PartitionKey(product.categoryId), new[] { product });
        Console.WriteLine(response.Resource);
    }

    private static async Task CreateProductAsync(Object product)
    {
        if (product.GetType().GetProperty("categoryId") is not null)
        {
            var categoryId = product.GetType().GetProperty("categoryId")!.GetValue(product);
            await container.CreateItemAsync(product, new PartitionKey(categoryId.ToString()));
            Console.WriteLine("Product is created successfully");
        }
        else
        {
            throw new ArgumentException("Product must have a categoryId.");
        }
    }

    private static async Task ReadProductAsync()
    {
        using (FeedIterator<dynamic> feedIterator = container.GetItemQueryIterator<dynamic>(
        $"select * from {ProductContainer} "))
        {
            while (feedIterator.HasMoreResults)
            {
                FeedResponse<dynamic> response = await feedIterator.ReadNextAsync();
                foreach (var item in response)
                {
                    Console.WriteLine(item);
                }
            }
        }
    }
}