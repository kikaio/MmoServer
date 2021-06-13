using CoreNet.Networking;
using CoreNet.Sockets;
using MmoCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmoServer.Users
{
    public class UserPeer : CoreSession, IDIspatcher
    {
        public enum STATE
        {
            NONE,
            TRY_WELCOME,
            IN_LOBBY,
            //need to dh key swap using rsa
            IN_SELECT,
            IN_BATTLE,
            END
        }

        protected STATE curState = STATE.TRY_WELCOME;
        protected Dictionary<STATE, UserPeerState> mStateDict = new Dictionary<STATE, UserPeerState>();

        public UserPeer(long _sid, CoreSock _sock) : base(_sid, _sock)
        {
        }

        private void InitState()
        {
            mStateDict[STATE.NONE] = new UserPeer_NONE(this);
            mStateDict[STATE.TRY_WELCOME] = new UserPeer_TRY_WELLCOME(this);
            mStateDict[STATE.IN_LOBBY] = new UserPeer_IN_LOBBY(this);
            mStateDict[STATE.IN_SELECT] = new UserPeer_IN_SELECT(this);
            mStateDict[STATE.IN_BATTLE] = new UserPeer_IN_BATTLE(this);
        }

        public void Dispatch_Ans(MmoCorePacket _mp)
        {
            mStateDict[curState].Dispatch_Ans(_mp);
        }

        public void Dispatch_Req(MmoCorePacket _mp)
        {
            mStateDict[curState].Dispatch_Req(_mp);
        }

        public void Dispatch_Noti(MmoCorePacket _mp)
        {
            if (_mp.cType == MmoCore.Enums.CONTENT_TYPE.HB_CHECK)
            {
                //update heartbeat date time 
                UpdateHeartBeat();
            }
            else
                mStateDict[curState].Dispatch_Noti(_mp);
        }

        public void Dispatch_Test(MmoCorePacket _mp)
        {
            mStateDict[curState].Dispatch_Test(_mp);
        }

        public void UpdateState(STATE _nextState)
        {
            if (_nextState == curState)
            {
                //todo : logging
                return;
            }
            curState = _nextState;
            //todo : logging about change state
            return;

            //switch (_nextState)
            //{
            //    case STATE.NONE:
            //        break;
            //    case STATE.TRY_WELCOME:
            //        break;
            //    case STATE.IN_LOBBY:
            //        break;
            //    case STATE.IN_SELECT:
            //        break;
            //    case STATE.IN_BATTLE:
            //        break;
            //    case STATE.END:
            //        break;
            //    default:
            //        break;
            //}
        }
    }
}
