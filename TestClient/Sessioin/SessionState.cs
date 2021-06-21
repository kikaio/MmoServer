using CoreNet.Utils;
using CoreNet.Utils.Loggers;
using MmoCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TestClient.Sessioin
{
    public enum SESSION_STATE
    {
        SEND_HELLO,
        CHECK_AUTH,
        IN_LOBBY,
    }

    internal interface IDispatch
    {
        void Dispatch_Req(MmoCorePacket _mp);
        void Dispatch_Ans(MmoCorePacket _mp);
        void Dispatch_Noti(MmoCorePacket _mp);
        void Dispatch_Test(MmoCorePacket _mp);
    }

    public class SessionState : IDispatch
    {
        public TestSession session { get; private set; }

        protected CoreLogger logger = new ConsoleLogger();

        public SessionState(TestSession _ts)
        {
            session = _ts;
        }

        public virtual void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispatch_Req(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public virtual void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }

    public class Session_SEND_HELLO : SessionState
    {
        public Session_SEND_HELLO(TestSession _ts)
            : base(_ts)
        {
        }

        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.WELCOME:
                    var ans = new WelcomeAns(_mp);
                    ans.SerRead();
                    logger.WriteDebug($"recv welcome, my session id is {ans.sId}");
                    break;
                default:
                    break;
            }
        }
    }

    public class Session_CHECK_AUTH : SessionState
    {
        public Session_CHECK_AUTH(TestSession _ts)
           : base(_ts)
        {
        }

        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.NONE:
                    break;
                default:
                    break;
            }
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.HELLO:
                    break;
                default:
                    break;
            }
        }
    }

    public class Session_IN_LOBBY: SessionState
    {
        public Session_IN_LOBBY(TestSession _ts)
           : base(_ts)
        {
        }

        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.NONE:
                    break;
                default:
                    break;
            }
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.NONE:
                    break;
                default:
                    break;
            }
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.CHAT:
                    break;
                default:
                    break;
            }
        }
    }
}
