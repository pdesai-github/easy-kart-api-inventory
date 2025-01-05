using Newtonsoft.Json;

namespace EasyKart.Inventory.Models
{
    public class InventoryItem
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }
        [JsonProperty("productId")]
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }

        public int Reserverd { get; set; }

        public int Available { get; set; }

        public void Reset()
        {
            Reserverd = 0;
            Available = Quantity;
        }

    }
}
