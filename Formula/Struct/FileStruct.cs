using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formula.Struct
{
    /// <summary>
    /// 文件信息
    /// </summary>
    [Serializable]
    public class FileStruct
    {
        /// <summary>
        /// 标识符
        /// </summary>
        public string FileID
        {
            get;
            set;
        }

        /// <summary>
        /// 文件名称
        /// </summary>
        public string FileName
        {
            get;
            set;
        }

        /// <summary>
        /// 文件全路径
        /// </summary>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime LastWriteTime
        {
            get;
            set;
        }

        /// <summary>
        /// 文件大小
        /// </summary>
        public long Length
        {
            get;
            set;
        }

        /// <summary>
        /// 所属文件夹
        /// </summary>
        public DirectoryStruct Directory
        {
            get;
            set;
        }

        /// <summary>
        /// 所属文件夹的全路径
        /// </summary>
        public string DirectoryName
        {
            get;
            set;
        }

        /// <summary>
        /// 文件在Ftp上所属的文件夹
        /// </summary>
        public string FtpFolder
        {
            get;
            set;
        }

        /// <summary>
        /// 文件在Ftp上面的真实文件名
        /// </summary>
        public string FtpFileName
        {
            get;
            set;
        }
    }
}
