using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    class PCSException
    {
    }

    public class ErrorCodeException : ApplicationException
    {
        public ErrorCodeException()
        {

        }
        public ErrorCodeException(string message) : base(message)
        {

        }
        public ErrorCodeException(string message, Exception inner) : base(message, inner)
        {

        }
    }
}
