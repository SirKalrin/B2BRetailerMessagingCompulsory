using System;
using System.Collections.Generic;

namespace Models
{
    public enum CountryCode { DK, US, EN, DE, FR };

    public class Order
    {
        public int CustomerId
        {
            get;
            set;
        }

        public List<int> ProductIds
        {
            get;
            set;
        }

        public CountryCode CountryCode
        {
            get;
            set;
        }

        public bool IsAvailable
        {
            get;
            set;
        }

        public int DeliveryTime
        {
            get;
            set;
        }

        public double ShippingCharge
        {
            get;
            set;
        }

        public Order()
        {
            ProductIds = new List<int>();
        }
    }
}
