namespace RomanaWeb.Models.Entity
{
    public class OrderLog
    {
        public int OrderLogId { get; set; }
        public int OrderId { get; set; }
        public int? OrderNo { get; set; }
        public string Action { get; set; } = "";
        public string Title { get; set; } = "";
        public string? Details { get; set; }
        public string ActorType { get; set; } = "System";
        public int? ActorId { get; set; }
        public string? ActorName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
