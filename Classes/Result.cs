namespace RomanaWeb.Classes
{
    public class ResObj
    {
        public bool success { get; set; }
        public string msg { get; set; }
        public object data { get; set; }
    }
    public static class Result
    {
        public static ResObj Return(bool success)
        {
            return new() { success = success };
        }

        public static ResObj Return(bool success, object data)
        {
            return new() { success = success, data = data };
        }    
        public static ResObj Return(bool success, string msgCode, object data)
        {
            return new() { success = success, msg = msgCode, data = data };
        }
         
        public static ResObj Return(bool success, string msgCode)
        {
            return new() { success = success, msg = msgCode };
        }
         
                              
    }
}
