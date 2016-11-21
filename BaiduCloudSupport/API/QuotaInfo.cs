using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    class QuotaInfo
    {
        public ulong quota { get; set; }
        public ulong used { get; set; }
        public ulong request_id { get; set; }
    }
}
