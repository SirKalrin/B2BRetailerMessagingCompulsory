using System;
using System.Collections.Generic;
using System.Threading;
using EasyNetQ;
using Messages;
using Models;

namespace Retailer
{
    class MainClass
    {
        public static List<WarehouseReply> WarehouseReplies
        {
            get;
            set;
        }

        public static IBus Bus
        {
            get;
            set;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("***Welcome to B2B Retailer Service***\n");
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                WarehouseReplies = new List<WarehouseReply>();
                Bus = bus;
                Console.WriteLine("Recieving messages from queue: (client.retailer)");
                bus.Receive<CustomerRequest>("client.retailer", request => HandleCustomerRequest(request));
                Console.WriteLine("Recieving messages from queue: (warehouse.retailer)");
                bus.Receive<WarehouseReply>("warehouse.retailer", message => HandleWareHouseMessage(message));
                Console.ReadLine();
            }
        }

        private static void HandleCustomerRequest(CustomerRequest req)
        {
            Console.WriteLine($"Message recieved on (client.retailer). Client id: '{req.CustomerId}', Location: '{req.CountryCode}'");
            Order o = new Order { CustomerId = req.CustomerId, CountryCode = req.CountryCode };
            o.ProductIds.Add(req.ProductId);
            var retailerRequest = new RetailerRequest() { Order = o };
            Bus.Publish<RetailerRequest>(retailerRequest, $"retailer.warehouses.{req.CountryCode}");
        }

        private static void HandleWareHouseMessage(WarehouseReply warehouseMsg)
        {
            WarehouseReplies.Add(warehouseMsg);
            if (warehouseMsg.Order.DeliveryTime <=2) {
                if (warehouseMsg.Order.IsAvailable) {
                    RetailerReply msg = new RetailerReply() { ProductId = warehouseMsg.Order.ProductIds[0], IsAvailable = warehouseMsg.Order.IsAvailable };
                    Bus.Send<RetailerReply>($"retailer.client.{warehouseMsg.Order.CustomerId}", msg);
                    WarehouseReplies.RemoveAll(x => x.Order.CustomerId == warehouseMsg.Order.CustomerId);
                }
                else {
                    RetailerRequest req = new RetailerRequest() { Order = warehouseMsg.Order };
                    Bus.Publish<RetailerRequest>(req, "retailer.warehouses");
                }
            }
            else if (warehouseMsg.Order.DeliveryTime > 2) {
                if (warehouseMsg.Order.IsAvailable) {
                    RetailerReply reply = new RetailerReply() { IsAvailable = warehouseMsg.Order.IsAvailable };
                    Bus.Send<RetailerReply>($"retailer.client.{warehouseMsg.Order.CountryCode}", reply);
                    WarehouseReplies.RemoveAll(x => x.Order.CustomerId == warehouseMsg.Order.CustomerId);
                }
            }
        }
    }
}
