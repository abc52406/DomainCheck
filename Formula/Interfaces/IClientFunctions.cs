using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formula.Struct;

namespace Formula.Interfaces
{
    public interface IClientFunctions
    {
#warning 由于通过接口异步调用暂时不成功，因此暂不使用HproseCallback
        byte[] ScreenShot();//屏幕截图

        string DeleteFiles(FileStruct[] files);//删除文件

        string DeleteDirectory(DirectoryStruct directory, bool recursive = true);//删除文件夹

        string RecieveDirectory(DirectoryStruct directory);//接收文件夹

        string RecieveFiles(FileStruct[] files);//接收文件

        DirectoryStruct GetDirectoryInfo(string directoryPath);//获取文件夹信息

        FileStruct GetFileInfo(string filePath);//获取文件信息

        bool KillProcessByName(string processName);//结束进程

        string StartProcess(string processPath);//启动进程

        bool ShutDown();//关机

        bool Restart();//重启电脑

        string GetLog(string logType, DateTime start, DateTime end);//获取客户端执行和异常日志

        bool UploadFiles(FileStruct[] filePaths);//上传客户端文件到服务器

        long UploadZip(string[] files, FileStruct zipFile);//上传压缩后的客户端文件到服务器
    }
}
