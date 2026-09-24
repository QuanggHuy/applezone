using System.ComponentModel.DataAnnotations;

namespace AppleZone.Helper
{
    public class Item
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }

        [Display(Name = "Old Price")]
        [DataType(DataType.Currency)]
        public double Price { get; set; }
        [DisplayFormat(DataFormatString = "{0:P0}")]
        public double Discount { get; set; }

        [Display(Name = "New Price")]
        [DataType(DataType.Currency)]
        public double NewPrice
        {
            get
            {
                return Price * (1 - Discount);
            }
        }

        [DataType(DataType.Currency)]
        public double Amount
        {
            get
            {
                return Quantity * NewPrice;
            }
        }
    }
}
