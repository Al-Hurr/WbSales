using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WbSales
{
    public class Filter
    {
        public List<string> Brands { get; set; }
        public List<string> Sizes { get; set; }
        public int Price { get; set; }
        public Filter()
        {

        }

        public Filter(List<string> brands, List<string> sizes, int price)
        {
            if(brands != null)
            {
                Brands = brands;
            }
            if(sizes != null)
            {
                Sizes = sizes;
            }
            Price = price;
        }
    }
}
