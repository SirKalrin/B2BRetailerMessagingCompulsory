using System;
using Models;

namespace Messages
{
    public class CustomerRequest
    {
        public CountryCode CountryCode
        {
            get;
            set;
        }

        public int CustomerId
        {
            get;
            set;
        }

        public int ProductId
        {
            get;
            set;
        }
    }
}
