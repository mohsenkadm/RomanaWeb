namespace RomanaWeb.Classes
{
    public static class Validation
    {
        public static bool IsEmpty(this string str)
        {
            return string.IsNullOrWhiteSpace(str);
        }
    }
}
