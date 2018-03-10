using EasyNetQ;
using System;
using System.Collections.Generic;
using System.Linq;
using Messages;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    public class Warehouse
    {
        private Models.CountryCode country;
        private int id;
        private IEnumerable<Product> products;
        private IBus bus;

        public Warehouse(int id, Models.CountryCode country, IEnumerable<Product> products)
        {
            this.country = country;
            this.id = id;
            this.products = products;
        }

        public void Start()
        {
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                this.bus = bus;
                bus.Subscribe<RetailerRequest>("retailer.warehouse." + id, HandleRetailerRequest, x => x.WithTopic("retailer.warehouses." + country.ToString()).WithTopic("retailer.warehouses"));
                Console.ReadLine();
            }
        }

        private void HandleRetailerRequest(RetailerRequest request)
        {
            WarehouseProgram.RecieveMessage(id, request.Order.ProductIds[0], country.ToString());
            WarehouseReply reply = new WarehouseReply() { Order = request.Order };

            Product p = products.FirstOrDefault(x => x.ProductId == request.Order.ProductIds[0]);
            if (p != null && p.ItemsInStock > 0)
            {
                reply.Order.IsAvailable = true;
                if (request.Order.CountryCode.Equals(country))
                {
                    reply.Order.DeliveryTime = 2;
                    reply.Order.ShippingCharge = 5;
                }
                else
                {
                    reply.Order.DeliveryTime = 4;
                    reply.Order.ShippingCharge = 10;
                }
            }
            else
                reply.Order.IsAvailable = false;
            bus.Send<WarehouseReply>("warehouse.retailer", reply);
            WarehouseProgram.SendMessage(id, reply.Order.IsAvailable);
        }
    }
}

