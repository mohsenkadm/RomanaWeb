namespace RomanaWeb.Models.Entity
{
    public class ProductIngredient
    {
        public int ProductIngredientId { get; set; }
        public int ProductsId { get; set; }
        public string IngredientName { get; set; } = "";
    }
}
