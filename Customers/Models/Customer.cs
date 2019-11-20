using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Customers.Models
{
    public class Customer
    {
        public long id { get; set; }
        public string name { get; set; }
        public int age { get; set; }
        public bool active { get; set; }
    }
}
