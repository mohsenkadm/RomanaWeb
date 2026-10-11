namespace RomanaWeb.Helper.Interface
{
    public interface IOrderLogService
    {
        Task LogAsync(int orderId, string action, string title, string? details = null, int? orderNo = null);
    }
}
