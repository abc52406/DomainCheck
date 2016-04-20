using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Checksums;

namespace Formula.Helper
{
    public class ZipOutputHelper : IDisposable
    {
        #region 私有变量
        ZipOutputStream zipStream;
        Crc32 crc;
        ZipEntry entry;
        #endregion

        #region 构造函数
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="filePath"></param>
        public ZipOutputHelper(string filePath)
        {
            zipStream = new ZipOutputStream(File.Create(filePath));
        }
        #endregion

        #region 在压缩文件中追加多个文件
        /// <summary>
        /// 在压缩文件中追加多个文件
        /// </summary>
        /// <param name="fileNames"></param>
        public void AddFile(string[] fileNames)
        {
            foreach (var filename in fileNames)
            {
                FileInfo fi = new FileInfo(filename);
                if (fi.Exists)
                {
                    AddFile(fi.Name, fi.Length);

                    int pos = 0;
                    int length = 1024 * 1024;
                    var st = fi.OpenRead();
                    byte[] bytes = GetFileBytes(fi.FullName, pos, length);
                    while (bytes != null && bytes.Length > 0)
                    {
                        AppendFileBytes(bytes, bytes.Length);
                        pos += length;
                        bytes = GetFileBytes(fi.FullName, pos, length); //fsFO.GetFileBytes(id, pos, length);
                    }
                }
            }
        }
        #endregion

        #region 在压缩文件中创建一个新的空文件
        /// <summary>
        /// 在压缩文件中创建一个新的空文件
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="fileSize"></param>
        public void AddFile(string fileName, long fileSize)
        {
            crc = new Crc32();
            crc.Reset();
            entry = new ZipEntry(fileName);
            entry.DateTime = DateTime.Now;
            entry.Size = fileSize;
            zipStream.PutNextEntry(entry);
        }
        #endregion

        #region 为当前文件输入流
        /// <summary>
        /// 为当前文件输入流
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="length"></param>
        public void AppendFileBytes(byte[] buffer, int length)
        {

            crc.Update(buffer, 0, length);
            entry.Crc = crc.Value;

            zipStream.Write(buffer, 0, length);
        }
        #endregion

        private byte[] GetFileBytes(string fileFullName, int pos, int length)
        {
            FileStream fs = File.OpenRead(fileFullName);

            long len = fs.Length - pos;
            if (len <= 0)
            {
                fs.Close();
                return new byte[0];
            }
            if (len > length)
                len = length;

            byte[] bytes = new byte[len];

            fs.Position = pos;
            fs.Read(bytes, 0, (int)len);


            fs.Close();
            return bytes;
        }

        public void Complete()
        {
            zipStream.Finish();
            zipStream.Close();
        }

        public void Dispose()
        {
            zipStream.Close();
        }


    }
}
