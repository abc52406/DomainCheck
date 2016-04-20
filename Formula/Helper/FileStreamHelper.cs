using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.IO;
using System.Linq;
using System.Text;

namespace Formula.Helper
{
    public static class FileStreamHelper
    {
        /// <summary>
        /// 保存文本文档
        /// </summary>
        /// <param name="filePath">文档路径</param>
        /// <param name="fileName">文档名称</param>
        /// <param name="fileContext">文档内容</param>
        /// <param name="encoding">编码格式</param>
        public static void SaveText(string filePath, string fileName, string fileContext, Encoding encoding)
        {
            try
            {
                if (!string.IsNullOrEmpty(filePath))
                {
                    DirectoryInfo di = new DirectoryInfo(filePath);
                    if (di.Exists)
                    {
                        FileInfo fi = new FileInfo(filePath + "\\" + fileName);
                        if (fi.Exists)
                            throw new Exception("文件已存在");
                        else
                        {
                            StreamWriter sw = new StreamWriter(filePath + "\\" + fileName, false, encoding);
                            sw.Write(fileContext);
                            sw.Close();
                        }
                    }
                    else
                        throw new Exception("文件夹不存在");
                }
                else
                {
                    FileInfo fi = new FileInfo(fileName);
                    if (fi.Exists)
                        throw new Exception("文件已存在");
                    else
                    {
                        StreamWriter sw = new StreamWriter(fileName, false, encoding);
                        sw.Write(fileContext);
                        sw.Close();
                    }
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 保存文本文档
        /// </summary>
        /// <param name="filePath">文档路径</param>
        /// <param name="fileName">文档名称</param>
        /// <param name="fileContext">文档内容</param>
        public static void SaveText(string filePath, string fileName, string fileContext)
        {
            SaveText(filePath, fileName, fileContext, Encoding.UTF8);
        }

        /// <summary>
        /// 保存文本文档
        /// </summary>
        /// <param name="filePath">文档路径</param>
        /// <param name="encoding">编码</param>
        /// <returns></returns>
        public static string ReadText(string filePath, Encoding encoding)
        {
            StreamReader sw = new StreamReader(filePath, encoding);
            try
            {
                string value = sw.ReadToEnd();
                return value;
            }
            catch
            {
                throw;
            }
            finally
            {
                sw.Close();
            }
        }

        /// <summary>
        /// 保存文本文档
        /// </summary>
        /// <param name="filePath">文档路径</param>
        /// <returns></returns>
        public static string ReadText(string filePath)
        {
            return ReadText(filePath, Encoding.UTF8);
        }
    }
}
