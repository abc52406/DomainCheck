using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Formula
{
    public class FormulaHelper
    {
        #region 生成ID

        /// <summary>
        /// 创建一个按时间排序的Guid
        /// </summary>
        /// <returns></returns>
        public static string CreateGuid()
        {
            //CAST(CAST(NEWID() AS BINARY(10)) + CAST(GETDATE() AS BINARY(6)) AS UNIQUEIDENTIFIER)
            byte[] guidArray = Guid.NewGuid().ToByteArray();
            DateTime now = DateTime.Now;

            DateTime baseDate = new DateTime(1900, 1, 1);

            TimeSpan days = new TimeSpan(now.Ticks - baseDate.Ticks);

            TimeSpan msecs = new TimeSpan(now.Ticks - (new DateTime(now.Year, now.Month, now.Day).Ticks));
            byte[] daysArray = BitConverter.GetBytes(days.Days);
            byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

            Array.Copy(daysArray, 0, guidArray, 2, 2);
            //毫秒高位
            Array.Copy(msecsArray, 2, guidArray, 0, 2);
            //毫秒低位
            Array.Copy(msecsArray, 0, guidArray, 4, 2);
            return new System.Guid(guidArray).ToString();
        }

        public static DateTime GetDateTimeFromGuid(string strGuid)
        {
            Guid guid = Guid.Parse(strGuid);

            DateTime baseDate = new DateTime(1900, 1, 1);
            byte[] daysArray = new byte[4];
            byte[] msecsArray = new byte[4];
            byte[] guidArray = guid.ToByteArray();

            // Copy the date parts of the guid to the respective byte arrays. 
            Array.Copy(guidArray, guidArray.Length - 6, daysArray, 2, 2);
            Array.Copy(guidArray, guidArray.Length - 4, msecsArray, 0, 4);

            // Reverse the arrays to put them into the appropriate order 
            Array.Reverse(daysArray);
            Array.Reverse(msecsArray);

            // Convert the bytes to ints 
            int days = BitConverter.ToInt32(daysArray, 0);
            int msecs = BitConverter.ToInt32(msecsArray, 0);

            DateTime date = baseDate.AddDays(days);
            date = date.AddMilliseconds(msecs * 3.333333);

            return date;
        }

        #endregion

        #region 实体库
        private static Dictionary<string, object> _entitiesContext = new Dictionary<string, object>();
        public static void DisposeEntities()
        {
            foreach (var item in _entitiesContext)
            {
                if (item.Value.GetType().BaseType == typeof(DbContext))
                    ((DbContext)item.Value).Dispose();
            }
        }

        private static Dictionary<string, string> _entitiesDic = new Dictionary<string, string>();

        public static void RegistEntities<T>(string conn)
        {
            _entitiesDic[typeof(T).FullName] = conn;
        }


        public static T GetEntities<T>() where T : DbContext, new()
        {
            string key = typeof(T).FullName;

            if (_entitiesContext.ContainsKey(key))
                return (T)_entitiesContext[key];
            else
            {
                string connName = "";// _entitiesDic[key];
                if (_entitiesDic.ContainsKey(key))
                    connName = _entitiesDic[key];
                else
                {
                    string entitiesName = key.Split('.').Last();
                    foreach (ConnectionStringSettings item in System.Configuration.ConfigurationManager.ConnectionStrings)
                    {
                        if (entitiesName.StartsWith(item.Name))
                        {
                            connName = item.Name;
                            break;
                        }
                    }
                }

                if (string.IsNullOrEmpty(connName))
                    throw new Exception(string.Format("配置文件中不包含{0}的链接字符串", key));

                ConstructorInfo constructor = typeof(T).GetConstructor(new Type[] { typeof(string) });
                T entities = (T)constructor.Invoke(new object[] { connName });

                _entitiesContext[key] = entities;
                return (T)_entitiesContext[key];
            }
        }


        #endregion

        #region 实体转化

        /// <summary>
        /// 对象列表转化为字典列表
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static List<Dictionary<string, object>> CollectionToListDic<T>(ICollection<T> list)
        {
            List<Dictionary<string, object>> resultList = new List<Dictionary<string, object>>();

            foreach (var item in list)
            {
                resultList.Add(ModelToDic<T>(item));
            }

            return resultList;
        }

        /// <summary>
        /// 对象转换为字典
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static Dictionary<string, object> ModelToDic<T>(T obj)
        {
            Dictionary<string, object> dic = new Dictionary<string, object>();

            PropertyInfo[] arrPtys = typeof(T).GetProperties();
            foreach (PropertyInfo destPty in arrPtys)
            {
                if (destPty.CanRead == false)
                    continue;
                if (destPty.PropertyType.Name == "ICollection`1")
                    continue;
                if ((destPty.PropertyType.IsClass && destPty.PropertyType.Name != "String") || destPty.PropertyType.IsArray || destPty.PropertyType.IsInterface)
                    continue;
                object value = destPty.GetValue(obj, null);
                dic.Add(destPty.Name, value);
            }
            return dic;
        }

        public static void UpdateModel(object dest, object src)
        {
            PropertyInfo[] destPtys = dest.GetType().GetProperties();
            PropertyInfo[] srcPtys = src.GetType().GetProperties();

            foreach (PropertyInfo destPty in destPtys)
            {
                if (destPty.CanRead == false)
                    continue;
                if (destPty.PropertyType.Name == "ICollection`1")
                    continue;
                if ((destPty.PropertyType.IsClass && destPty.PropertyType.Name != "String") || destPty.PropertyType.IsArray || destPty.PropertyType.IsInterface)
                    continue;

                PropertyInfo srcPty = srcPtys.Where(c => c.Name == destPty.Name).SingleOrDefault();
                if (srcPty == null)
                    continue;
                if (srcPty.CanWrite == false)
                    continue;

                object value = srcPty.GetValue(src, null);

                destPty.SetValue(dest, value, null);
            }
        }

        #endregion

        #region 获取单例化服务
        /// <summary>
        /// 获取服务（服务是单例的）
        /// </summary>
        /// <typeparam name="T">服务接口</typeparam>
        /// <returns></returns>
        public static T GetService<T>()
        {
            if (_DicSingletonSerivces.ContainsKey(typeof(T)))
            {
                return (T)Activator.CreateInstance(_DicSingletonSerivces[typeof(T)]);
            }
            else
            {
                return default(T);
            }
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <typeparam name="T1">接口</typeparam>
        /// <typeparam name="T2">实现</typeparam>
        public static void RegisterService<T1, T2>()
            where T2 : T1
        //where T1 : ISingleton
        {
            _DicSingletonSerivces[typeof(T1)] = typeof(T2);
        }
        private static Dictionary<Type, Type> _DicSingletonSerivces = new Dictionary<Type, Type>();
        #endregion
    }
}
