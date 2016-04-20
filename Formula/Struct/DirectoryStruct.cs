using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Formula.Struct
{
    /// <summary>
    /// 文件夹信息
    /// </summary>
    [Serializable]
    public class DirectoryStruct
    {
        /// <summary>
        /// 文件夹名称
        /// </summary>
        public string DirectoryName
        {
            get;
            set;
        }

        /// <summary>
        /// 文件夹全路径
        /// </summary>
        public string FullName
        {
            get;
            set;
        }

        /// <summary>
        /// 子文件夹信息
        /// </summary>
        public List<DirectoryStruct> ChildDirectoryDatas
        {
            get;
            set;
        }

        /// <summary>
        /// 文件信息
        /// </summary>
        public List<FileStruct> FileDatas
        {
            get;
            set;
        }
    }
}
