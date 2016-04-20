using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SQLDMO;

namespace Formula.Helper
{
    public static class BakHelper
    {
        #region 备份数据库
        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="connectionstring">数据库连接字符串</param>
        /// <param name="path">备份路径</param>
        public static void CompressDatabase(string connectionstring, string path)
        {
            var items = connectionstring.Trim(';').Split(';').Select(c => c.Split('='));
            var servername = items.Where(c => c[0] == "Data Source");
            if (servername.Count() != 1 || servername.Single().Count() != 2)
                throw new Exception("数据库服务器字符串错误");
            var username = items.Where(c => c[0] == "User ID");
            if (username.Count() != 1 || username.Single().Count() != 2)
                throw new Exception("用户名字符串错误");
            var password = items.Where(c => c[0] == "Password");
            if (password.Count() != 1 || password.Single().Count() != 2)
                throw new Exception("密码字符串错误");
            var database = items.Where(c => c[0] == "Initial Catalog");
            if (database.Count() != 1 || database.Single().Count() != 2)
                throw new Exception("数据库名字符串错误");
            CompressDatabase(servername.Single()[1], username.Single()[1], password.Single()[1], database.Single()[1], path);
        }

        /// <summary>
        /// 备份数据库
        /// </summary>
        /// <param name="serverName">数据实例名</param>
        /// <param name="userName">用户</param>
        /// <param name="password">密码</param>
        /// <param name="databaseName">库名</param>
        /// <param name="path">备份路径</param>
        public static void CompressDatabase(string serverName,string userName,string password,string databaseName,string path)
        {
            Backup oBackup = new Backup();
            SQLServer oSQLServer = new SQLServer();
            try
            {
                oSQLServer.LoginSecure = false;
                oSQLServer.Connect(serverName, userName, password);
                oBackup.Action = SQLDMO_BACKUP_TYPE.SQLDMOBackup_Database;
                oBackup.Database = databaseName;
                oBackup.Files = path;
                oBackup.BackupSetName = databaseName;
                oBackup.BackupSetDescription = string.Format("{0} {1}", databaseName, DateTime.Now);
                oBackup.Initialize = true;
                oBackup.SQLBackup(oSQLServer);
            }
            catch
            {
                throw;
            }
            finally
            {
                oSQLServer.DisConnect();
            }
        }
        #endregion

        #region 还原数据库
        /// <summary>
        /// 还原数据库
        /// </summary>
        /// <param name="connectionstring">数据库连接字符串</param>
        /// <param name="path">文件路径</param>
        public static void RestoreDatabase(string connectionstring, string path)
        {
            var items = connectionstring.Trim(';').Split(';').Select(c => c.Split('='));
            var servername = items.Where(c => c[0] == "Data Source");
            if (servername.Count() != 1 || servername.Single().Count() != 2)
                throw new Exception("数据库服务器字符串错误");
            var username = items.Where(c => c[0] == "User ID");
            if (username.Count() != 1 || username.Single().Count() != 2)
                throw new Exception("用户名字符串错误");
            var password = items.Where(c => c[0] == "Password");
            if (password.Count() != 1 || password.Single().Count() != 2)
                throw new Exception("密码字符串错误");
            var database = items.Where(c => c[0] == "Initial Catalog");
            if (database.Count() != 1 || database.Single().Count() != 2)
                throw new Exception("数据库名字符串错误");
            RestoreDatabase(servername.Single()[1], username.Single()[1], password.Single()[1], database.Single()[1], path);
        }

        /// <summary>
        /// 还原数据库
        /// </summary>
        /// <param name="serverName">数据实例名</param>
        /// <param name="userName">用户</param>
        /// <param name="password">密码</param>
        /// <param name="databaseName">库名</param>
        /// <param name="path">文件路径</param>
        public static void RestoreDatabase(string serverName, string userName, string password, string databaseName, string path)
        {
            SQLDMO.Restore oRestore = new SQLDMO.Restore();
            SQLServer oSQLServer = new SQLServer();
            try
            {
                oSQLServer.LoginSecure = false;
                oSQLServer.Connect(serverName, userName, password);
                oRestore.Action = SQLDMO_RESTORE_TYPE.SQLDMORestore_Database;
                oRestore.Database = databaseName;
                oRestore.Files = path;
                oRestore.FileNumber = path.Split(',').Count();
                oRestore.BackupSetName = databaseName;
                oRestore.ReplaceDatabase = true;
                oRestore.SQLRestore(oSQLServer);
            }
            catch
            {
                throw;
            }
            finally
            {
                oSQLServer.DisConnect();
            }
        }
        #endregion
    }
}
