using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ICSharpCode.SharpZipLib.Zip;
using System.IO;

namespace Formula.Helper
{
    public class ZipInputHelper : IDisposable
    {
        #region 私有变量
        ZipInputStream zipStream;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        public ZipInputHelper(string filePath)
        {
            zipStream = new ZipInputStream(File.OpenRead(filePath));
        }
        #endregion

        #region 将压缩文件中的文件解压到指定的文件夹
        /// <summary>
        /// 将压缩文件中的文件解压到指定的文件夹
        /// </summary>
        /// <param name="DirectoryPath"></param>
        public void DecompressFiles(string DirectoryPath)
        {
            if (Directory.Exists(DirectoryPath))
            {
                var entry = zipStream.GetNextEntry();
                while (entry != null)
                {
                    if (entry.CompressedSize != 0)
                        using (var fs = File.Create(string.Format("{0}\\{1}", DirectoryPath.TrimEnd('\\'), entry.Name)))
                        {
                            int pos = 0;
                            int length = 1024 * 1024;
                            byte[] bytes = GetFileBytes(zipStream, pos, length);
                            while (bytes != null && bytes.Length > 0)
                            {
                                fs.Write(bytes, pos, bytes.Length);
                                pos += length;
                                bytes = GetFileBytes(zipStream, pos, length); //fsFO.GetFileBytes(id, pos, length);
                            }
                            fs.Close();
                        }
                    entry = zipStream.GetNextEntry();
                }
            }
        }
        #endregion

        private byte[] GetFileBytes(ZipInputStream stream, int pos, int length)
        {
            long len = stream.Length - pos;
            if (len <= 0)
            {
                return new byte[0];
            }
            if (len > length)
                len = length;

            byte[] bytes = new byte[len];

            stream.Read(bytes, pos, (int)len);

            return bytes;
        }

        public void Dispose()
        {
            zipStream.Close();
        }
    }
}
