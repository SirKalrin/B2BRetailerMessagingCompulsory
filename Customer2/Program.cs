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
            Console.WriteLine("Enter a country code:");
            CountryCode cc;
            Enum.TryParse(Console.ReadLine(), out cc);
            Console.WriteLine("<--Welcome dear customer!-->\nPlease enter the ID of a product you desire: (type 'Quit' to exit)");
            string input = "";
            while (true)
            {
                using (var bus = RabbitHutch.CreateBus("host=localhost"))
                {
                    input = Console.ReadLine();
                    if (input.ToLower() == "quit") {
                        break;
                    }
                    int productId;
                    try
                    {
                        int.TryParse(input, out productId);
                        CustomerRequest req = new CustomerRequest { CustomerId = id, CountryCode = CountryCode.DK, ProductId = productId };
                        bus.Send<CustomerRequest>("client.retailer", req);
                        bus.Receive<RetailerReply>($"retailer.client.{id}", response => HandleRetailerResponse(response));
                        Console.WriteLine("The query for your desired product has been sent.");
                        Console.ReadLine();
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("The query enters could not be intepreted as an integer. Insert numeric value to order...");
                    }
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
