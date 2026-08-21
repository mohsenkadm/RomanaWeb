using System.ComponentModel.DataAnnotations.Schema;

namespace RomanaWeb.Models.Entity
{
    /// <summary>
    /// Driver-submitted problem report linked to an order.
    /// Status: 0=Pending, 1=InProgress, 2=Resolved, 3=Unresolved.
    /// </summary>
    public class ProblemReport
    {
        public int ProblemReportId { get; set; }
        public int OrderId { get; set; }
        public int SaleManId { get; set; }
        public string Message { get; set; } = "";
        public int Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? AdminNote { get; set; }

        [NotMapped] public int? OrderNo { get; set; }
        [NotMapped] public string? SaleManName { get; set; }
        [NotMapped] public string? SaleManPhone { get; set; }
        [NotMapped] public string? RestaurantName { get; set; }
        [NotMapped] public DateTime? OrderDate { get; set; }
        [NotMapped] public string? StatusLabel { get; set; }
    }

    public static class ProblemReportStatus
    {
        public const int Pending = 0;
        public const int InProgress = 1;
        public const int Resolved = 2;
        public const int Unresolved = 3;

        public static string ToLabel(int status) => status switch
        {
            Pending => "قيد الانتظار",
            InProgress => "قيد المعالجة",
            Resolved => "حُلت",
            Unresolved => "لم يتم حلها",
            _ => "غير معروف"
        };

        public static bool IsValid(int status) =>
            status is Pending or InProgress or Resolved or Unresolved;
    }
}
