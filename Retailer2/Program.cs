using System;
using System.Collections.Generic;
using EasyNetQ;
using Messages;
using Models;

namespace Retailer
{
    class MainClass
    {
        public static List<CustomerRequest> CustomerRequests
        {
            get;
            set;
        }

        public static Dictionary<int, int> WarehouseReplyLimiter
        {
            get;
            set;
        }

        public static IBus Bus
        {
            get;
            set;
        }

        public static int MAX_WAREHOUSES
        {
            get;
            set;
        } = 3;

        public static Dictionary<int, bool> HasPublished
        {
            get;
            set;
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("***Welcome to B2B Retailer Service***\n");
            using (var bus = RabbitHutch.CreateBus("host=localhost"))
            {
                CustomerRequests = new List<CustomerRequest>();
                WarehouseReplyLimiter = new Dictionary<int, int>();
                HasPublished = new Dictionary<int, bool>();
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
            CustomerRequests.Add(req);
            HasPublished.Add(req.CustomerId, false);
            WarehouseReplyLimiter.Add(req.CustomerId, 0);
            Console.WriteLine($"Message recieved on (client.retailer). Client id: '{req.CustomerId}', Location: '{req.CountryCode}'");
            Order o = new Order { CustomerId = req.CustomerId, CountryCode = req.CountryCode };
            o.ProductIds.Add(req.ProductId);
            var retailerRequest = new RetailerRequest() { Order = o };
            Bus.Publish<RetailerRequest>(retailerRequest, $"retailer.warehouses.{req.CountryCode}");
        }

        private static void HandleWareHouseMessage(WarehouseReply warehouseMsg)
        {
            if (CustomerRequests.Find(x => x.CustomerId == warehouseMsg.Order.CustomerId) != null)
            {
                if (!HasPublished[warehouseMsg.Order.CustomerId])
                {
                    Console.WriteLine("Recieved message from a local warehouse.");
                    if (warehouseMsg.Order.IsAvailable)
                    {
                        Console.WriteLine("Product is available..\nSending to client..");
                        RetailerReply msg = new RetailerReply() { ProductId = warehouseMsg.Order.ProductIds[0], IsAvailable = warehouseMsg.Order.IsAvailable };
                        Bus.Send<RetailerReply>($"retailer.client.{warehouseMsg.Order.CustomerId}", msg);
                        CustomerRequests.RemoveAll(x => x.CustomerId == warehouseMsg.Order.CustomerId);
                        WarehouseReplyLimiter.Remove(warehouseMsg.Order.CustomerId);
                        HasPublished.Remove(warehouseMsg.Order.CustomerId);
                        Console.WriteLine($"Response sent to customer with client id: {warehouseMsg.Order.CustomerId}");
                    }
                    else
                    {
                        Console.WriteLine("Product not available.. Publishing to all warehouses..");
                        RetailerRequest req = new RetailerRequest() { Order = warehouseMsg.Order };
                        Bus.Publish<RetailerRequest>(req, "retailer.warehouses");
                        HasPublished[warehouseMsg.Order.CustomerId] = true;
                    }
                }
                else
                {
                    Console.WriteLine("Recieved message from one of all warehouses..");
                    if (warehouseMsg.Order.IsAvailable || ++WarehouseReplyLimiter[warehouseMsg.Order.CustomerId] >= MAX_WAREHOUSES)
                    {
                        RetailerReply reply = new RetailerReply() { ProductId = warehouseMsg.Order.ProductIds[0], IsAvailable = warehouseMsg.Order.IsAvailable };
                        Bus.Send<RetailerReply>($"retailer.client.{warehouseMsg.Order.CustomerId}", reply);
                        CustomerRequests.RemoveAll(x => x.CustomerId == warehouseMsg.Order.CustomerId);
                        WarehouseReplyLimiter.Remove(warehouseMsg.Order.CustomerId);
                        HasPublished.Remove(warehouseMsg.Order.CustomerId);
                        Console.WriteLine($"Response sent to customer with client id: {warehouseMsg.Order.CustomerId}");
                    }
                }
            }
        }
    }
}