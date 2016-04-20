using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Formula.Struct;

namespace Formula.Interfaces
{
    public interface IServerFunctions
    {
#warning 由于通过接口异步调用暂时不成功，因此暂不使用HproseCallback

        void SendHeartBeat(string clientID);//发送心跳包给服务器

        void SendHeartBeat(string clientID, DateTime time);//发送心跳包给服务器

        void OnClientError(string clientID, string function, string errorMessage);//客户端执行失败后通知服务器
    }
}
