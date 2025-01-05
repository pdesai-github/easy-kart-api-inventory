using EasyKart.Inventory.Services;
using EasyKart.Shared.Commands;
using EasyKart.Shared.Events;
using EasyKart.Shared.Models;
using MassTransit;

namespace EasyKart.Inventory.Consumers
{
    public class InventoryConsumer : IConsumer<ReserveInventoryCommand>
    {
        IPublishEndpoint _publishEndpoint;
        IInventoryService _inventoryService;
        public InventoryConsumer(IPublishEndpoint publishEndpoint, IInventoryService inventoryService)
        {
            _publishEndpoint = publishEndpoint;
            _inventoryService = inventoryService;
        }
        public async Task Consume(ConsumeContext<ReserveInventoryCommand> context)
        {
            Order order = context.Message.Order;
            Console.WriteLine($"Inventory reserved for order {order.Id}");

            bool res = await _inventoryService.ReserveInventory(order);

            if (!res)
            {
                await _publishEndpoint.Publish(new InventoryOutOfStockEvent()
                {
                    Order = order,
                    CreatedAt = DateTime.Now
                });
            }
            else
            {
                await _publishEndpoint.Publish(new InventoryReservedEvent()
                {
                    Order = order,
                    CreatedAt = DateTime.Now
                });
            }
        }
    }
}
