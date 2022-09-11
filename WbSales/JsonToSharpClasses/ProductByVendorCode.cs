using System.Collections.Generic;

namespace WbSales.JsonToSharpClasses
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

    public class Extended
    {
        public int basicSale { get; set; }
        public int basicPriceU { get; set; }
        public int clientSale { get; set; }
        public int clientPriceU { get; set; }
    }

    public class Product
    {
        public int id { get; set; }
        public int root { get; set; }
        public int kindId { get; set; }
        public int subjectId { get; set; }
        public int subjectParentId { get; set; }
        public string name { get; set; }
        public string brand { get; set; }
        public int brandId { get; set; }
        public int siteBrandId { get; set; }
        public int supplierId { get; set; }
        public int priceU { get; set; }
        public int sale { get; set; }
        public int salePriceU { get; set; }
        public Extended extended { get; set; }
        public int averagePrice { get; set; }
        public int benefit { get; set; }
        public int pics { get; set; }
        public int rating { get; set; }
        public int feedbacks { get; set; }
        public List<Color> colors { get; set; }
        public List<Size> sizes { get; set; }
        public bool diffPrice { get; set; }
        public int time1 { get; set; }
        public int time2 { get; set; }
        public int wh { get; set; }
    }

    public class ProductByVendorCode
    {
        public int state { get; set; }
        public Data data { get; set; }
    }

    public class Size
    {
        public string name { get; set; }
        public string origName { get; set; }
        public int rank { get; set; }
        public int optionId { get; set; }
        public List<Stock> stocks { get; set; }
        public int? time1 { get; set; }
        public int? time2 { get; set; }
        public int? wh { get; set; }
    }

    public class Stock
    {
        public int wh { get; set; }
        public int qty { get; set; }
    }


}
