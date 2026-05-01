namespace RomanaWeb.Models.Entity
{
    public class ProductSize
    {
        public int ProductSizeId { get; set; }
        public int ProductsId { get; set; }
        public string SizeName { get; set; } = "";
        public double SizePrice { get; set; }
    }
}
