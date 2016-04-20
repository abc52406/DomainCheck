using System;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.ServiceProcess;

namespace Formula.Helper
{
    /// <summary>
    /// windows系统方法帮助类
    /// </summary>
    public static class WindowsHelper
    {
        #region 屏幕截图
        /// <summary>
        /// 屏幕截图
        /// </summary>
        /// <returns></returns>
        public static Bitmap ScreenShot()
        {
            int w = Screen.PrimaryScreen.Bounds.Width;
            int h = Screen.PrimaryScreen.Bounds.Height;
            Bitmap myImage = new Bitmap(w, h);
            Graphics g = Graphics.FromImage(myImage);
            g.CopyFromScreen(new System.Drawing.Point(0, 0), new System.Drawing.Point(0, 0), new System.Drawing.Size(w, h));
            IntPtr dc1 = g.GetHdc();
            g.ReleaseHdc(dc1);
            return myImage;
        }
        #endregion

        #region 关机
        /// <summary>
        /// 关机
        /// </summary>
        public static void ShutDown()
        {
            //启动本地程序并执行命令
            Process.Start("Shutdown.exe", " -s -t 0");
        }
        #endregion

        #region 重启
        /// <summary>
        /// 重启
        /// </summary>
        public static void Restart()
        {
            Process.Start("Shutdown.exe", " -r -t 0");
        }
        #endregion

        #region 启动进程
        /// <summary>
        /// 启动进程
        /// </summary>
        /// <param name="AppPath">文件全路径</param>
        public static void StartProcess(string AppPath)
        {
            Process.Start(AppPath);
        }
        #endregion

        #region 结束进程
        /// <summary>
        /// 结束进程
        /// </summary>
        /// <param name="Name">进程名称</param>
        public static bool KillProcessByName(string Name)
        {
            Process[] ps = Process.GetProcessesByName(Name);
            if (ps != null)
            {
                foreach (Process p in ps)
                {
                    p.Kill();
                }
                return true;
            }
            else
                return false;
        }

        /// <summary>
        /// 结束进程
        /// </summary>
        /// <param name="Name">进程Id</param>
        public static void KillProcessById(int Id)
        {
            try
            {
                Process ps = Process.GetProcessById(Id);
                if (ps != null)
                {
                    ps.Kill();
                }
            }
            catch (Exception ex)
            {
                LogWriter.Error(ex, string.Format("结束进程{0}失败", Id));
                throw ex;
            }
        }
        #endregion

        #region 启动服务
        /// <summary>
        /// 启动服务
        /// </summary>
        /// <param name="Name"></param>
        public static bool StartService(string Name)
        {
            var services = ServiceController.GetServices();
            foreach (var s in services)
            {
                if (s.ServiceName == Name)
                {
                    if (s.Status == ServiceControllerStatus.Stopped)
                    {
                        s.Start();
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion

        #region 关闭服务
        /// <summary>
        /// 关闭服务
        /// </summary>
        /// <param name="Name"></param>
        public static bool StopService(string Name)
        {
            var services = ServiceController.GetServices();
            foreach (var s in services)
            {
                if (s.ServiceName == Name)
                {
                    if (s.Status == ServiceControllerStatus.Running)
                    {
                        s.Stop();
                        return true;
                    }
                }
            }
            return false;
        }
        #endregion
    }
}
