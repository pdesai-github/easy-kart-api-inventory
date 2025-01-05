
using EasyKart.Inventory.Consumers;
using EasyKart.Inventory.Repositories;
using EasyKart.Inventory.Services;
using EasyKart.Shared.Events;
using MassTransit;

namespace EasyKart.Inventory
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string conn = builder.Configuration.GetConnectionString("AzureServiceBus");

            builder.Services.AddScoped<IInventoryService, InventoryService>();
            builder.Services.AddScoped<IInventoryRepository, InventoryRepository>();

            // Add services to the container.
            builder.Services.AddMassTransit(x =>
            {
                x.AddConsumer<InventoryConsumer>();
                x.UsingAzureServiceBus((context, cfg) =>
                {
                    cfg.Host(conn);
                    cfg.Message<InventoryOutOfStockEvent>(configTopology =>
                    {
                        configTopology.SetEntityName("ordercreated");
                    });
                    cfg.Message<InventoryReservedEvent>(configTopology =>
                    {
                        configTopology.SetEntityName("ordercreated");
                    });

                    cfg.SubscriptionEndpoint("ReserveInventoryCommandSubscription", "ordercreated", e =>
                    {
                        e.ConfigureConsumer<InventoryConsumer>(context);
                    });
                   
                });
            });
            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
             
            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
