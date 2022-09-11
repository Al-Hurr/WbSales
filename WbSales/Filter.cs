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
        public int PriceLow { get; set; }
        public int PriceHight { get; set; }

        public Filter()
        {

        }

        public Filter(List<string> brands, List<string> sizes, int priceHigth)
        {
            if(brands != null)
            {
                Brands = brands;
            }
            if(sizes != null)
            {
                Sizes = sizes;
            }
            PriceHight = priceHigth;
        }
    }
}
