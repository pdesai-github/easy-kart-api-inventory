
using EasyKart.Inventory.Repositories;
using EasyKart.Shared.Models;

namespace EasyKart.Inventory.Services
{
   
    public class InventoryService : IInventoryService
    {
        private IInventoryRepository _inventoryRepository;
        public InventoryService(IInventoryRepository inventoryRepository)
        {
            _inventoryRepository = inventoryRepository;
        }
        public async Task<bool> ReserveInventory(Order order)
        {
            bool result = true;
            foreach (var item in order.Items)
            {
               bool res = await  _inventoryRepository.ReserveInventory(item.ProductId, item.Quantity);
                if (!res)
                {
                    result = false;
                    break;
                }
            }

            if (!result)
            {
                foreach (var item in order.Items)
                {
                    await _inventoryRepository.ReleaseInventory(item.ProductId);
                }
            }
            return result;
        }
    }
}
