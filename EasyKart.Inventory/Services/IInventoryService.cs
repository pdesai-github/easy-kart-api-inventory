using EasyKart.Inventory.Models;
using EasyKart.Shared.Models;

namespace EasyKart.Inventory.Services
{
    public interface IInventoryService
    {
        Task<bool> ReserveInventory(Order order);
    }
}
