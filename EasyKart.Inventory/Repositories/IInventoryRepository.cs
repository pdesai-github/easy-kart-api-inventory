using EasyKart.Inventory.Models;

namespace EasyKart.Inventory.Repositories
{
    public interface IInventoryRepository
    {
        Task<bool> ReserveInventory(Guid productId, int quantity);
        Task<bool> ReleaseInventory(Guid productId);
    }
}
