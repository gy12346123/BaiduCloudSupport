using System;
using System.Collections.Generic;
using System.Text;

namespace MyDownloader.Core
{
    public enum DownloaderState: byte 
    {
        NeedToPrepare = 0,
        Preparing,
        WaitingForReconnect,
        Prepared,
        Working,
        Pausing,
        Paused,
        Ended,
        EndedWithError
    }

    public enum DownloaderStateChinese: byte
    {
        需解析 = 0,
        解析中,
        等待重连,
        解析完毕,
        下载中,
        暂停中,
        已暂停,
        已结束,
        出错
    }
}
