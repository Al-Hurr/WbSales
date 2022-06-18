using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WbSales
{
    // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Color
    {
        public string name { get; set; }
        public int id { get; set; }
    }

    public class Data
    {
        public List<Product> products { get; set; }
    }

    public class Product
    {
        public int __sort { get; set; }
        public int ksort { get; set; }
        public int time1 { get; set; }
        public int time2 { get; set; }
        public int id { get; set; }
        public int root { get; set; }
        public int kindId { get; set; }
        public int subjectId { get; set; }
        public int subjectParentId { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public int brandId { get; set; }
        public int siteBrandId { get; set; }
        public int sale { get; set; }
        public int priceU { get; set; }
        public int salePriceU { get; set; }
        public int pics { get; set; }
        public int rating { get; set; }
        public int feedbacks { get; set; }
        public List<Color> colors { get; set; }
        public List<Size> sizes { get; set; }
        public bool diffPrice { get; set; }
        public int? panelPromoId { get; set; }
        public string promoTextCat { get; set; }

        public int SalePrice() => this.salePriceU / 100;
    }

    public class Root
    {
        public int state { get; set; }
        public int version { get; set; }
        public Data data { get; set; }
    }

    public class Size
    {
        public string name { get; set; }
        public string origName { get; set; }
        public int rank { get; set; }
        public int optionId { get; set; }
    }
}
