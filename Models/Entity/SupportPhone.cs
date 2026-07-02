namespace RomanaWeb.Models.Entity
{
    public class SupportPhone
    {
        public int SupportPhoneId { get; set; }

        /// <summary>1 = زبون، 2 = متجر، 3 = سائق</summary>
        public int AppType { get; set; }

        public string Phone { get; set; } = "";
        public string? Label { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
