using EasyKart.Inventory.Models;
using Microsoft.Azure.Cosmos;
using System.ComponentModel;

namespace EasyKart.Inventory.Repositories
{
    public class InventoryRepository : IInventoryRepository
    {
        IConfiguration _configuration;
        private readonly string _cosmosEndpoint;
        private readonly string _cosmosKey;
        private readonly string _databaseId;
        private readonly string _containerId;
        private readonly string _partitionKey;

        private readonly CosmosClient _cosmosClient;
        private readonly Microsoft.Azure.Cosmos.Container _container;

        public InventoryRepository(IConfiguration configuration)
        {
            _configuration = configuration;
            _cosmosEndpoint = _configuration["CosmosDB:endpoint"];
            _cosmosKey = _configuration["CosmosDB:authKey"];
            _databaseId = _configuration["CosmosDB:databaseId"];
            _containerId = _configuration["CosmosDB:containerId"];
            _partitionKey = _configuration["CosmosDB:partitionKey"];

            _cosmosClient = new CosmosClient(_cosmosEndpoint, _cosmosKey, new CosmosClientOptions() 
            {
                SerializerOptions = new CosmosSerializationOptions()
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            });
            _container = _cosmosClient.GetContainer(_databaseId, _containerId);
        }

        public async Task<bool> ReserveInventory(Guid productId, int quantity)
        {
            try
            {
                var inventoryItem = await GetInventoryItem(productId);
                if (inventoryItem == null)
                {
                    return false;
                }
                if (inventoryItem.Available >= quantity)
                {
                    inventoryItem.Available -= quantity;
                    inventoryItem.Reserverd += quantity;
                    await UpdateInventory(inventoryItem);
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        
            return false;

        }

        public async Task<bool> ReleaseInventory(Guid productId)
        {
            var inventoryItem = await GetInventoryItem(productId);
            if (inventoryItem == null)
            {
                return false;
            }
            inventoryItem.Available = inventoryItem.Quantity;
            inventoryItem.Reserverd = 0;
            await UpdateInventory(inventoryItem);
            return true;
        }

        public async Task<InventoryItem> GetInventoryItem(Guid productId)
        {
            var sqlQueryText = $"SELECT * FROM c WHERE c.productId = '{productId.ToString()}'";
            QueryDefinition queryDefinition = new QueryDefinition(sqlQueryText);
            FeedIterator<InventoryItem> queryResultSetIterator = _container.GetItemQueryIterator<InventoryItem>(queryDefinition);
            while (queryResultSetIterator.HasMoreResults)
            {
                FeedResponse<InventoryItem> currentResultSet = await queryResultSetIterator.ReadNextAsync();
                foreach (InventoryItem inventoryItem in currentResultSet)
                {
                    return inventoryItem;
                }
            }
            return null;
        }

        public async Task UpdateInventory(InventoryItem inventoryItem)
        {
            await _container.UpsertItemAsync<InventoryItem>(inventoryItem, new PartitionKey(inventoryItem.ProductId.ToString()));
        }
    }
}
