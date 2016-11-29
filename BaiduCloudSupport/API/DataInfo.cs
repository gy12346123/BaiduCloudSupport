using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace BaiduCloudSupport.API
{
    public class SimpleUserInfo
    {
        public ulong uid { get; set; }
        public string uname { get; set; }
        public string portrait { get; set; }
    }


    public class QuotaInfo
    {
        public ulong quota { get; set; }
        public ulong used { get; set; }
        public ulong request_id { get; set; }
    }

    public class FileMeta
    {
        public ulong fs_id { get; set; }
        public string path { get; set; }
        public UInt32 ctime { get; set; }
        public UInt32 mtime { get; set; }
        public string block_list { get; set; }
        public ulong size { get; set; }
        public UInt32 isdir { get; set; }
        public UInt32 ifhassubdir { get; set; }
        public UInt32 filenum { get; set; }
    }

    public class FileList
    {
        public ulong fs_id { get; set; }
        public string path { get; set; }
        public UInt32 ctime { get; set; }
        public UInt32 mtime { get; set; }
        public string md5 { get; set; }
        public ulong size { get; set; }
        public UInt32 isdir { get; set; }
    }

    public class EachFilePath
    {
        public string path;
    }

    public class FilePath
    {
        public List<EachFilePath> list;
    }

    public class FileListDataItem
    {
        public ulong fs_id { get; set; }
        public string path { get; set; }
        public string file { get; set; }
        public DateTime mtime { get; set; }
        public string md5 { get; set; }
        public string size { get; set; }
        public UInt32 isdir { get; set; }
        public bool isSelected { get; set; }
        public BitmapImage Icon { get; set; }
    }

    public class DownloadListDataItem
    {
        public ulong fs_id { get; set; }
        public string file { get; set; }
        public long size { get; set; }
        public long received { get; set; }
        public double percentage { get; set; }
        public double rate { get; set; }
        public DateTime startTime { get; set; }
        public bool isSelected { get; set; }
    }
}
