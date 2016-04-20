using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Diagnostics;
using System.Configuration;
using System.Data.OleDb;
using System.Data;

namespace Formula.Helper
{
    /// <summary>
    /// 功能说明：套用模板输出Excel，并对数据进行分页
    /// 作    者：Lingyun_k
    /// 创建日期：2005-7-12
    /// </summary>
    public class ExcelHelper
    {
        private string _FilePath = string.Empty;
        private OleDbConnection _OleQueryConn;
        private OleDbConnection _OleAlterConn;

        public ExcelHelper(string filePath)
        {
            this._FilePath = filePath;
            string strQueryConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + _FilePath + "';User ID=Admin;Mode=Share Deny None;Extended Properties='Excel 12.0 xml;HDR=No;IMEX=1'";
            string strAlterConn = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source='" + _FilePath + "';User ID=Admin;Mode=Share Deny None;Extended Properties='Excel 12.0 xml;HDR=No;IMEX=0'";
            //修改连接Excel的字符串，根据判断不同后缀名来使用不同的连接字符串
            if (_FilePath.ToLower().EndsWith("xls"))
            {
                strQueryConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + _FilePath + "';Extended Properties='Excel 8.0; HDR=yes; IMEX=1;'";
                strAlterConn = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source='" + _FilePath + "';Extended Properties='Excel 8.0; HDR=yes; IMEX=0;'";
            }
            _OleQueryConn = new OleDbConnection(strQueryConn);
            _OleAlterConn = new OleDbConnection(strAlterConn);
        }

        public DataTable GetSheet(string sheetName)
        {
            DataTable dt = null;
            try
            {
                _OleQueryConn.Open();
                DataTable table = _OleQueryConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                #region 判断Sheet页是否存在
                bool existSheet = false;
                foreach (DataRow row in table.Rows)
                {
                    string tableName = row["Table_Name"].ToString().Trim('\'').TrimEnd('$');
                    if (tableName == sheetName)
                    {
                        existSheet = true;
                        break;
                    }
                }
                if (existSheet == false)
                {
                    _OleQueryConn.Close();
                    dt = new DataTable();
                    return dt;
                }
                #endregion



                String sql = string.Format("SELECT * FROM  [{0}$]", sheetName);
                //可是更改Sheet名称，比如sheet2，等等    
                OleDbDataAdapter OleDaExcel = new OleDbDataAdapter(sql, _OleQueryConn);
                DataSet OleDsExcle = new DataSet();
                OleDaExcel.Fill(OleDsExcle, "aa");
                _OleQueryConn.Close();
                dt = OleDsExcle.Tables[0];
            }
            catch
            {
                throw;
            }
            return dt;
        }

        public void InsertData(string sheetName, DataTable dt)
        {
            try
            {
                _OleAlterConn.Open();
                DataTable table = _OleAlterConn.GetOleDbSchemaTable(OleDbSchemaGuid.Tables, null);

                #region 判断Sheet页是否存在
                bool existSheet = false;
                foreach (DataRow row in table.Rows)
                {
                    string tableName = row["Table_Name"].ToString().Trim('$');
                    if (tableName == sheetName)
                    {
                        existSheet = true;
                        break;
                    }
                }
                if (existSheet == false)
                {
                    return;
                }
                #endregion

                OleDbCommand cmd =null;
                List<string> cols = new List<string>();
                foreach (DataColumn col in dt.Columns)
                    cols.Add(col.ColumnName);

                string strSQL = string.Format("INSERT INTO [{0}$] ({1})   VALUES ({2})",sheetName, string.Join(",", cols.ToArray()), string.Join(",", cols.Select(c => "?").ToArray()));

                foreach (DataRow dr in dt.Rows)
                {
                    cmd = new OleDbCommand(strSQL, _OleAlterConn);
                    cmd.CommandText = strSQL;
                    foreach (var item in cols)
                    {
                        cmd.Parameters.Add(new OleDbParameter(item, dr[item]));
                    }
                    cmd.ExecuteNonQuery();
                    cmd.Parameters.Clear();
                }
                _OleAlterConn.Close();
            }
            catch
            {
                throw;
            }
        }

        public static void SaveDataTableToExcel(DataTable excelTable, string filePath)
        {
            Microsoft.Office.Interop.Excel.Application app = new Microsoft.Office.Interop.Excel.Application();
            try
            {
                app.Visible = false;
                Microsoft.Office.Interop.Excel.Workbook wBook = app.Workbooks.Add(true);
                Microsoft.Office.Interop.Excel.Worksheet wSheet = wBook.Worksheets[1] as Microsoft.Office.Interop.Excel.Worksheet;
                if (excelTable.Rows.Count > 0)
                {
                    int row = 0;
                    row = excelTable.Rows.Count;
                    int col = excelTable.Columns.Count;
                    for (int i = 0; i < row; i++)
                    {
                        for (int j = 0; j < col; j++)
                        {
                            string str = excelTable.Rows[i][j].ToString();
                            wSheet.Cells[i + 2, j + 1] = str;
                        }
                    }
                }
                int size = excelTable.Columns.Count;
                for (int i = 0; i < size; i++)
                {
                    wSheet.Cells[1, 1 + i] = excelTable.Columns[i].ColumnName;
                }
                //设置禁止弹出保存和覆盖的询问提示框  
                app.DisplayAlerts = false;
                app.AlertBeforeOverwriting = false;
                //保存工作簿   
                wBook.Save();
                //保存excel文件   
                app.Save(filePath);
                app.SaveWorkspace(filePath);
                app.Quit();
                app = null;
            }
            catch
            {
                throw;
            }
        }
    }
}
