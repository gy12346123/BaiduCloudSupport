using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BaiduCloudSupport.API
{
    public struct SimpleUserInfoStruct
    {
        public ulong uid;
        public string uname;
        public string portrait;
    }

    public struct PCSFileMetaStruct
    {
        public ulong fs_id;
        public string path;
        public UInt32 ctime;
        public UInt32 mtime;
        public string block_list;
        public ulong size;
        public UInt32 isdir;
        public UInt32 ifhassubdir;
        public UInt32 filenum;
    }

    public struct FileListStruct
    {
        public ulong fs_id;
        public string path;
        public UInt32 ctime;
        public UInt32 mtime;
        public string md5;
        public ulong size;
        public UInt32 isdir;
    }

    public struct DBCFileMetaStruct
    {
        public UInt32 category;
        public UInt32 dir_empty;
        public UInt32 empty;
        public ulong fs_id;
        public UInt32 isdir;
        public UInt32 local_ctime;
        public UInt32 local_mtime;
        public string md5;
        public UInt32 oper_id;
        public string path;
        public UInt32 server_ctime;
        public string server_filename;
        public UInt32 server_mtime;
        public ulong size;
        public string[] thumbs;
        public UInt32 unlist;
    }

    public struct DBCCopyStruct
    {
        public string path;
        public string dest;
        public string newname;
    }

    public struct DBCFolderListStruct
    {
        public int dir_empty;
        public string path;
    }

    public struct DBCFileShareStruct
    {
        public UInt32 ctime;
        public ulong shareid;
        public string link;
        public string shorturl;
        public string password;
    }
}
