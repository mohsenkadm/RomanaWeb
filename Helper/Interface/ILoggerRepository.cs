using System;
using System.Threading.Tasks;

namespace RomanaWeb.Helper.Interface
{
    public interface ILoggerRepository
    {
        public void Write(Exception exception, string message);
        public Task WriteAsync(Exception exception, string message);
    }
}
