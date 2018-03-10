using System;
using EasyNetQ;
using Messages;
using Models;

namespace Customer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("***Welcome to the Customer Client for the B2BRetailer System***");
            Console.WriteLine("Enter a unique client id:");
            int id;
            int.TryParse(Console.ReadLine(), out id);
            Console.WriteLine($"Enter your country code ({CountryCode.DK}, {CountryCode.DE}, {CountryCode.EN}, {CountryCode.FR}, {CountryCode.US} :");
            CountryCode cc;
            Enum.TryParse(Console.ReadLine(), out cc);
            Console.WriteLine($"<--Welcome dear customer from {cc}!-->\nPlease enter the ID of a product you desire: (type 'Quit' to exit)");
            string input = "";
            while (true)
            {
                using (var bus = RabbitHutch.CreateBus("host=localhost"))
                {
                    input = Console.ReadLine();
                    if (input.ToLower() == "quit")
                    {
                        break;
                    }
                    int productId;
                    int.TryParse(input, out productId);
                    CustomerRequest req = new CustomerRequest { CustomerId = id, CountryCode = cc, ProductId = productId };
                    bus.Send<CustomerRequest>("client.retailer", req);
                    bus.Receive<RetailerReply>($"retailer.client.{id}", response => HandleRetailerResponse(response));
                    Console.WriteLine("The query for your desired product has been sent.");
                    Console.ReadLine();
                }
            }
        }

        private static void HandleRetailerResponse(RetailerReply response)
        {
            string status = response.IsAvailable ? "available" : "not available";
            Console.WriteLine($"Response recieved from retailer...\nThe product with product id '{response.ProductId}' is {status}");
            Console.WriteLine("Press <enter> to continue..");
        }
    }
}
