using CoreNet.Networking;
using CoreNet.Sockets;
using CoreNet.Utils;
using CoreNet.Utils.Loggers;
using MmoCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    internal interface IDispatch
    {
        void Dispatch_Req(MmoCorePacket _mp);
        void Dispatch_Ans(MmoCorePacket _mp);
        void Dispatch_Noti(MmoCorePacket _mp);
        void Dispatch_Test(MmoCorePacket _mp);
    }


    class TestSession : CoreSession, IDispatch
    {
        private CoreLogger logger = new ConsoleLogger();
        public bool isWelcomed = false;
        public TestSession(long _sid, CoreSock _sock) : base(_sid, _sock)
        {
        }

        public void Dispatch_Ans(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.WELCOME:
                    var ans = new WelcomeAns(_mp);
                    ans.SerRead();
                    isWelcomed = true;
                    logger.WriteDebug($"recv welcome, my session id is {ans.sId}");
                    break;
                default:
                    break;
            }
        }

        public void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public void Dispatch_Req(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }
}
