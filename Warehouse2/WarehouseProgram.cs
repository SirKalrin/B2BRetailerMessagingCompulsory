using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Warehouse
{
    class WarehouseProgram
    {
        static void Main(string[] args)
        {
            List<Product> dkProducts = new List<Product>{
                new Product { ProductId = 1, ItemsInStock = 10 }
            };

            List<Product> frProducts = new List<Product>{
                new Product { ProductId = 1, ItemsInStock = 10 },
                new Product { ProductId = 2, ItemsInStock = 2 }
            };
            List<Product> usProducts = new List<Product>{
                new Product { ProductId = 1, ItemsInStock = 10 },
                new Product { ProductId = 2, ItemsInStock = 2 },
                new Product { ProductId = 3, ItemsInStock = 5 }
            };

            Task.Factory.StartNew(() => new Warehouse(1, Models.CountryCode.DK, dkProducts).Start());
            Task.Factory.StartNew(() => new Warehouse(2, Models.CountryCode.FR, frProducts).Start());
            Task.Factory.StartNew(() => new Warehouse(3, Models.CountryCode.US, usProducts).Start());

            Console.ReadLine();
        }

        public static void RecieveMessage(int warehouseId, int productId, String countryCode)
        {
            Console.WriteLine("Warehouse " + warehouseId + ", " + countryCode + ", recieved an order on product " + productId);
        }
        public static void SendMessage(int warehouseId, bool isAvailable)
        {
            Console.WriteLine("Warehouse " + warehouseId + " sent a " + isAvailable + " reply.");
        }

    }
}
