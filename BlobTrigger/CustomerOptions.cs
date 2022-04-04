using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlobTrigger
{
    public class CustomerOptions
    {
        public string Name { get; set; } = string.Empty;
        public int CustomerNumber { get; set; } = 0;
        public AddressOptions Address { get; set; } = new AddressOptions();
    }
}
