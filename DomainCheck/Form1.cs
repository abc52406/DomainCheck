using Formula;
using Formula.Helper;
using Formula.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DomainCheck
{
    public partial class Form1 : Form
    {
        static string currentpath = Environment.CurrentDirectory;
        static int domainlength = 0, searchindex = 0;
        static List<string> result = new List<string>();
        public delegate void CallBackDelegate(string message);
        ThreadingCheckDomain tcd = null;
        string[] domainext = new string[] { "com", "cn", "com.cn" };
        public Form1()
        {
            InitializeComponent();
            foreach (var item in domainext)
                this.listBoxDomain.Items.Add(item);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Log4NetConfig.Configure();
            var numberlength = this.textBox1.Text;
            var letterlength = this.textBox2.Text;
            var pinyinlength = this.textBox3.Text;
            var sleeptime = this.textBox4.Text;
            int nl = 0, ll = 0, pl = 0, st = 0;
            if (!int.TryParse(numberlength, out nl))
            {
                MessageBox.Show("数字域名长度有误");
                return;
            }
            if (nl < 0 || nl > 6)
            {
                MessageBox.Show("数字域名长度必须是0到6");
                return;
            }
            if (!int.TryParse(letterlength, out ll))
            {
                MessageBox.Show("字母域名长度有误");
                return;
            }
            if (ll < 0 || ll > 6)
            {
                MessageBox.Show("字母域名长度必须是0到6");
                return;
            }
            if (!int.TryParse(pinyinlength, out pl))
            {
                MessageBox.Show("拼音域名长度有误");
                return;
            }
            if (pl < 0 || pl > 2)
            {
                MessageBox.Show("拼音域名长度必须是0到2");
                return;
            }
            if (!int.TryParse(sleeptime, out st))
            {
                MessageBox.Show("查询间隔有误");
                return;
            }
            if (st < 1)
            {
                MessageBox.Show("拼音域名长度必须大于1");
                return;
            }
            List<string> dn = null;
            if (this.listBoxDomain.SelectedItems.Count > 0)
            {
                dn =new  List<string>();
                foreach (var item in this.listBoxDomain.SelectedItems)
                    dn.Add(item.ToString());
                domainext = dn.ToArray();
            }
            this.button1.Enabled = false;
            toolStripStatusLabel1.Text = "数据正在初始化";
            CallBackDelegate cbd = Callback;
            tcd = new ThreadingCheckDomain(st, nl, ll, pl, domainext, this.textBoxPre.Text, this.textBoxSuf.Text);
            Thread th = new Thread(tcd.Start);
            th.Start(cbd);
        }

        #region 回调函数
        /// <summary>
        /// 回调函数
        /// </summary>
        /// <param name="msg"></param>
        private void Callback(string msg)
        {
            if (msg.StartsWith("length"))
            {
                domainlength = int.Parse(msg.Substring(6));
            }
            else
            {
                if (!msg.Contains("不可用"))
                {
                    var d = msg.Substring(0, msg.Length - 2);
                    result.Add(d);
                    this.listBox1.Items.Add(d);
                    if (result.Count % 100 == 0)
                    {
                        FileStreamHelper.SaveText(currentpath, string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMddhhmmss")), string.Join(@"
", result));
                        result.Clear();
                    }
                }
                //未完成
                if (searchindex + 1 < domainlength)
                {
                    toolStripProgressBar1.Value = domainlength > 0 ? (searchindex + 1) * toolStripProgressBar1.Maximum / domainlength : toolStripProgressBar1.Maximum;
                    toolStripStatusLabel1.Text = string.Format("({1}/{2}){0}", msg, searchindex + 1, domainlength);
                }
                //已完成
                else
                {
                    toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                    toolStripStatusLabel1.Text = string.Format("({1}/{2}){0} 全部完成", msg, searchindex + 1, domainlength);
                    if (result.Count > 0)
                    {
                        FileStreamHelper.SaveText(currentpath, string.Format("{0}.txt", DateTime.Now.ToString("yyyyMMddhhmmss")), string.Join(@"
", result));
                        result.Clear();
                    }
                }
                searchindex++;
            }
        }
        #endregion

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            tcd.stop = true;
        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            tcd.stop = true;
        }
    }

    public class ThreadingCheckDomain
    {
        #region 私有变量
        int Sleeptime = 0, nl = 0, ll = 0, pl = 0;
        List<string> Domains = new List<string>();
        /// <summary>
        /// 重试次数
        /// </summary>
        int errorcount = 3;
        /// <summary>
        /// api地址
        /// </summary>
        string domaincheckurl = "http://panda.www.net.cn/cgi-bin/check.cgi?area_domain={0}";
        /// <summary>
        /// api允许一次最多查询的域名数量
        /// </summary>
        int apimaxdomain = 100;
        WebClient client = new WebClient();
        string successStr = "210";
        string resultsuccess = "可用", resultfail = "不可用";
        string[] domainext = new string[] { "com", "cn", "com.cn" };
        string domainfomart = "{0}{1}{2}.{3}";
        string domainpre = "", domainsuf = "";
        #endregion

        #region 公共属性
        public bool stop = false;
        public int DomainLength
        {
            get
            {
                return Domains.Count;
            }
        }
        #endregion

        #region 构造函数
        public ThreadingCheckDomain(int sleeptime, List<string> domains, string[] domainn = null, string pre = "", string suf = "")
        {
            Sleeptime = sleeptime;
            Domains = domains;
            if (domainn != null)
                domainext = domainn;
            domainpre = pre;
            domainsuf = suf;
        }
        public ThreadingCheckDomain(int sleeptime, int numberlength, int letterlength, int pinyinlength, string[] domainn = null, string pre = "", string suf = "")
        {
            Sleeptime = sleeptime;
            nl = numberlength;
            ll = letterlength;
            pl = pinyinlength;
            if (domainn != null)
                domainext = domainn;
            domainpre = pre;
            domainsuf = suf;
        }
        #endregion

        public void Start(object o)
        {
            if (nl > 0 || ll > 0 || pl > 0)
            {
                var list = NumberDomain(nl);
                list.AddRange(PinYinDomain(pl));
                list.AddRange(LetterDomain(ll));
                Domains = list;
            }
            Form1.CallBackDelegate cbd = o as Form1.CallBackDelegate;
            cbd(string.Concat("length", Domains.Count.ToString()));
            for (int i = 0; i < Domains.Count / apimaxdomain + (Domains.Count % apimaxdomain == 0 ? 0 : 1); i++)
            {
                var items = Domains.Where((c, j) => j >= i * apimaxdomain && j < (i + 1) * apimaxdomain).ToList();
                var result = CheckDomain(items);
                foreach (var r in result)
                {
                    cbd(string.Concat(r.Key, r.Value ? resultsuccess : resultfail));
                    Thread.Sleep(Sleeptime / apimaxdomain);
                    if (stop)
                        break;
                }
                if (stop)
                    break;
            }
        }

        #region 检查域名可用性
        /// <summary>
        /// 检查域名可用性
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private bool CheckDomain(string domain)
        {
            XmlHelper xh = null;
            int ec = 0;
            while (ec < errorcount)
            {
                var str = "";
                try
                {
                    str = client.DownloadString(string.Format(domaincheckurl, domain));
                    xh = new XmlHelper(str);
                    break;
                }
                catch (Exception ex)
                {
                    LogWriter.Error(ex, string.Format("查找域名{0}出错，结果：{1}", domain, str));
                    ec++;
                }
            }
            if (xh == null)
            {
                LogWriter.Info(string.Format("查找域名{0}出错三次，跳过", domain));
            }
            else
            {
                var ele = xh.QueryEle("/property/original");
                if (ele != null)
                {
                    return ele.InnerText.Contains(successStr);
                }
                else
                {
                    LogWriter.Info(string.Format("查找域名{0}出错三次，跳过", domain));
                }
            }
            return false;
        }

        /// <summary>
        /// 检查域名可用性
        /// </summary>
        /// <param name="domain"></param>
        /// <returns></returns>
        private Dictionary<string, bool> CheckDomain(List<string> domain)
        {
            int ec = 0;
            var str = "";
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            while (ec < errorcount)
            {
                try
                {
                    str = client.DownloadString(string.Format(domaincheckurl, string.Join(",", domain)));
                    //("com|baidu.com|211|Domain exists#com|qq.com|211|Domain exists#co1m|;360.co1m|211|unsupport tld type.#com.cn|baidu.com.cn|211|In use")
                    if (domainext.Any(c => str.StartsWith(string.Format("(\"{0}|", c))))
                        break;
                }
                catch (Exception ex)
                {
                    LogWriter.Error(ex, string.Format("查找域名{0}出错，结果：{1}", string.Join(",", domain), str));
                    ec++;
                }
            }
            if (domainext.All(c => !str.StartsWith(string.Format("(\"{0}|", c))))
            {
                LogWriter.Info(string.Format("查找域名{0}出错三次，跳过", domain));
            }
            else
            {
                str.Trim();
                LogWriter.Info(str);
                var resultlist = str.Substring(2, str.Length - 4).Split('#').Select(c => c.Split('|'));
                foreach (var item in resultlist)
                    result.Add(item.ElementAt(1), item.ElementAt(2) == successStr);
            }
            return result;
        }
        #endregion

        #region 域名策略
        #region 数字域名
        /// <summary>
        /// 数字域名
        /// </summary>
        /// <param name="length">数字长度</param>
        /// <returns></returns>
        private List<string> NumberDomain(int length)
        {
            List<string> list = new List<string>();
            int start = 1;
            while (start <= length)
            {
                for (int i = 0; i < Math.Pow(10, start); i++)
                {
                    list.AddRange(domainext.Select(c => string.Format(domainfomart, domainpre, preStr(i.ToString(), '0', start), domainsuf, c)));
                }
                start++;
            }
            return list;
        }
        #endregion

        #region 字母域名
        /// <summary>
        /// 字母域名
        /// </summary>
        /// <param name="length">字母长度</param>
        /// <returns></returns>
        private List<string> LetterDomain(int length)
        {
            List<string> list = new List<string>();
            int start = 1;
            while (start <= length)
            {
                for (int i = 0; i < Math.Pow(26, start); i++)
                {
                    list.AddRange(domainext.Select(c => string.Format(domainfomart
                        , domainpre, preStr(BaseConvert(i.ToString(), "0123456789", "abcdefghijklmnopqrstuvwxyz"), 'a', start), domainsuf, c)));
                }
                start++;
            }
            return list;
        }
        #endregion

        #region 拼音域名
        /// <summary>
        /// 拼音域名(支持1到1个拼音)
        /// </summary>
        /// <param name="length">长度</param>
        /// <returns></returns>
        private List<string> PinYinDomain(int length)
        {
            List<string> list = new List<string>();
            if (length > 0)
                list.AddRange(domainext.SelectMany(c => pyName.Select(d => string.Format(domainfomart
                        , domainpre, d, domainsuf, c))));
            if (length > 1)
                for (int i = 0; i < pyName.Count(); i++)
                    for (int j = 0; j < pyName.Count(); j++)
                        list.AddRange(domainext.Select(c => string.Format(domainfomart
                        , domainpre, string.Concat(pyName[i], pyName[j]), domainsuf, c)));
            if (length > 2)
                for (int i = 0; i < pyName.Count(); i++)
                    for (int j = 0; j < pyName.Count(); j++)
                        for (int k = 0; k < pyName.Count(); k++)
                            list.AddRange(domainext.Select(c => string.Format(domainfomart
                        , domainpre, string.Concat(pyName[i], pyName[j], pyName[k]), domainsuf, c)));
            return list;
        }

        private string[] pyName
        {
            get
            {
                return new string[]
    {
    "A","Ai","An","Ang","Ao","Ba","Bai","Ban","Bang","Bao","Bei","Ben",
    "Beng","Bi","Bian","Biao","Bie","Bin","Bing","Bo","Bu","Cai","Can",
    "Cang","Cao","Ce","Ceng","Cha","Chai","Chan","Chang","Chao","Che","Chen","Cheng",
    "Chi","Chong","Chou","Chu","Chuai","Chuan","Chuang","Chui","Chun","Chuo","Ci","Cong",
    "Cou","Cu","Cuan","Cui","Cun","Cuo","Da","Dai","Dan","Dang","Dao","De",
    "Deng","Di","Dian","Diao","Die","Ding","Diu","Dong","Dou","Du","Duan","Dui",
    "Dun","Duo","E","En","Er","Fa","Fan","Fang","Fei","Fen","Feng","Fo",
    "Fou","Fu","Ga","Gai","Gan","Gang","Gao","Ge","Gei","Gen","Geng","Gong",
    "Gou","Gu","Gua","Guai","Guan","Guang","Gui","Gun","Guo","Ha","Hai","Han",
    "Hang","Hao","He","Hei","Hen","Heng","Hong","Hou","Hu","Hua","Huai","Huan",
    "Huang","Hui","Hun","Huo","Ji","Jia","Jian","Jiang","Jiao","Jie","Jin","Jing",
    "Jiong","Jiu","Ju","Juan","Jue","Jun","Ka","Kai","Kan","Kang","Kao","Ke",
    "Ken","Keng","Kong","Kou","Ku","Kua","Kuai","Kuan","Kuang","Kui","Kun","Kuo",
    "La","Lai","Lan","Lang","Lao","Le","Lei","Leng","Li","Lia","Lian","Liang",
    "Liao","Lie","Lin","Ling","Liu","Long","Lou","Lu","Lv","Luan","Lue","Lun",
    "Luo","Ma","Mai","Man","Mang","Mao","Me","Mei","Men","Meng","Mi","Mian",
    "Miao","Mie","Min","Ming","Miu","Mo","Mou","Mu","Na","Nai","Nan","Nang",
    "Nao","Ne","Nei","Nen","Neng","Ni","Nian","Niang","Niao","Nie","Nin","Ning",
    "Niu","Nong","Nu","Nv","Nuan","Nue","Nuo","O","Ou","Pa","Pai","Pan",
    "Pang","Pao","Pei","Pen","Peng","Pi","Pian","Piao","Pie","Pin","Ping","Po",
    "Pu","Qi","Qia","Qian","Qiang","Qiao","Qie","Qin","Qing","Qiong","Qiu","Qu",
    "Quan","Que","Qun","Ran","Rang","Rao","Re","Ren","Reng","Ri","Rong","Rou",
    "Ru","Ruan","Rui","Run","Ruo","Sa","Sai","San","Sang","Sao","Se","Sen",
    "Seng","Sha","Shai","Shan","Shang","Shao","She","Shen","Sheng","Shi","Shou","Shu",
    "Shua","Shuai","Shuan","Shuang","Shui","Shun","Shuo","Si","Song","Sou","Su","Suan",
    "Sui","Sun","Suo","Ta","Tai","Tan","Tang","Tao","Te","Teng","Ti","Tian",
    "Tiao","Tie","Ting","Tong","Tou","Tu","Tuan","Tui","Tun","Tuo","Wa","Wai",
    "Wan","Wang","Wei","Wen","Weng","Wo","Wu","Xi","Xia","Xian","Xiang","Xiao",
    "Xie","Xin","Xing","Xiong","Xiu","Xu","Xuan","Xue","Xun","Ya","Yan","Yang",
    "Yao","Ye","Yi","Yin","Ying","Yo","Yong","You","Yu","Yuan","Yue","Yun",
    "Za", "Zai","Zan","Zang","Zao","Ze","Zei","Zen","Zeng","Zha","Zhai","Zhan",
    "Zhang","Zhao","Zhe","Zhen","Zheng","Zhi","Zhong","Zhou","Zhu","Zhua","Zhuai","Zhuan",
    "Zhuang","Zhui","Zhun","Zhuo","Zi","Zong","Zou","Zu","Zuan","Zui","Zun","Zuo"
    }.Select(c => c.ToLower()).ToArray();
            }
        }
        #endregion
        #endregion

        #region 私有函数
        /// <summary>
        /// 字符串前端填充字符
        /// </summary>
        /// <param name="source">初始字符串</param>
        /// <param name="pre">填充字符</param>
        /// <param name="length">最终长度</param>
        /// <returns>填充后的结果</returns>
        private string preStr(string source, char pre, int length)
        {
            while (source.Length < length)
                source = string.Format("{0}{1}", pre, source);
            return source;
        }

        /// <summary>
        /// 将一个大数字符串从M进制转换成N进制
        /// </summary>
        /// <param name="sourceValue">M进制数字字符串</param>
        /// <param name="sourceBaseChars">M进制字符集</param>
        /// <param name="newBaseChars">N进制字符集</param>
        /// <returns>N进制数字字符串</returns>
        public static string BaseConvert(string sourceValue, string sourceBaseChars, string newBaseChars)
        {
            //M进制
            var sBase = sourceBaseChars.Length;
            //N进制
            var tBase = newBaseChars.Length;
            //M进制数字字符串合法性判断（判断M进制数字字符串中是否有不包含在M进制字符集中的字符）
            if (sourceValue.Any(s => !sourceBaseChars.Contains(s))) return null;
            //将M进制数字字符串的每一位字符转为十进制数字依次存入到LIST中
            var intSource = new List<int>();
            intSource.AddRange(sourceValue.Select(c => sourceBaseChars.IndexOf(c)));
            //余数列表
            var res = new List<int>();
            //开始转换（判断十进制LIST是否为空或只剩一位且这个数字小于N进制）
            while (!((intSource.Count == 1 && intSource[0] < tBase) || intSource.Count == 0))
            {
                //每一轮的商值集合
                var ans = new List<int>();
                var y = 0;
                //十进制LIST中的数字逐一除以N进制（注意：需要加上上一位计算后的余数乘以M进制）
                foreach (var t in intSource)
                {
                    //当前位的数值加上上一位计算后的余数乘以M进制
                    y = y * sBase + t;
                    //保存当前位与N进制的商值
                    ans.Add(y / tBase);
                    //计算余值
                    y %= tBase;
                }
                //将此轮的余数添加到余数列表
                res.Add(y);
                //将此轮的商值（去除0开头的数字）存入十进制LIST做为下一轮的被除数
                var flag = false;
                intSource.Clear();
                foreach (var a in ans.Where(a => a != 0 || flag))
                {
                    flag = true;
                    intSource.Add(a);
                }
            }

            //如果十进制LIST还有数字，需将此数字添加到余数列表后
            if (intSource.Count > 0) res.Add(intSource[0]);
            //将余数列表反转，并逐位转换为N进制字符
            var nValue = string.Empty;
            for (var i = res.Count - 1; i >= 0; i--)
            {
                nValue += newBaseChars[res[i]].ToString();
            }
            return nValue;
        }
        #endregion
    }
}
