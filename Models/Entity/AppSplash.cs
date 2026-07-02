namespace RomanaWeb.Models.Entity
{
    /// <summary>Single splash row shown when the customer app opens (admin-managed).</summary>
    public class AppSplash
    {
        public int AppSplashId { get; set; }
        public string ImageUrl { get; set; } = "";
        public string? Details { get; set; }
        public bool IsVisible { get; set; }
        public DateTime UpdatedAt { get; set; }
    }
}
