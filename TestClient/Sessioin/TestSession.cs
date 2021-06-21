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
using TestClient.Sessioin;

namespace TestClient
{

    public class TestSession : CoreSession
    {
        private CoreLogger logger = new ConsoleLogger();
        public bool isWelcomed = false;

        private Dictionary<SESSION_STATE, SessionState> stateDict = new Dictionary<SESSION_STATE, SessionState>();
        public SESSION_STATE curState { get; protected set; } = SESSION_STATE.SEND_HELLO;


        public TestSession(long _sid, CoreSock _sock) : base(_sid, _sock)
        {
            stateDict[SESSION_STATE.SEND_HELLO] = new Session_SEND_HELLO(this);
            stateDict[SESSION_STATE.CHECK_AUTH] = new Session_CHECK_AUTH(this);
            stateDict[SESSION_STATE.IN_LOBBY] = new Session_IN_LOBBY(this);

        }

        public void ChangeState(SESSION_STATE _nextState)
        {
            if (curState == _nextState)
                return;
            curState = _nextState;
        }

        public void Dispatch_Ans(MmoCorePacket _mp)
        {
            stateDict[curState].Dispatch_Ans(_mp);
        }

        public void Dispatch_Noti(MmoCorePacket _mp)
        {
            stateDict[curState].Dispatch_Noti(_mp);
        }

        public void Dispatch_Req(MmoCorePacket _mp)
        {
            stateDict[curState].Dispatch_Req(_mp);
        }

        public void Dispatch_Test(MmoCorePacket _mp)
        {
            stateDict[curState].Dispatch_Test(_mp);
        }
    }
}
