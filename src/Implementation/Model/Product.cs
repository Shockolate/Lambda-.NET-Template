using System.ComponentModel.DataAnnotations;

namespace Implementation.Model
{
    public class Product
    {
        [Key]
        public string ProductCrn { get; set; }
        public string Name { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
    }
}
