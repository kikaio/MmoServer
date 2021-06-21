using CoreNet.Utils;
using CoreNet.Utils.Loggers;
using MmoCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmoServer.Users
{
    public class UserPeerState : IDIspatcher
    {
        public UserPeer mPeer { get; protected set; }
        protected CoreLogger logger = new ConsoleLogger();

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

    public class UserPeer_NONE : UserPeerState
    {
        public UserPeer_NONE(UserPeer _p)
        {
            mPeer = _p;
        }


        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }

    public class UserPeer_TRY_WELLCOME : UserPeerState
    {
        public UserPeer_TRY_WELLCOME(UserPeer _p)
        {
            mPeer = _p;
        }


        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            //only recv hello request packet from client
            logger.WriteDebug($"{mPeer.SessionId} recv content : {_mp.cType.ToString()}");
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.HELLO:
                    //change state from "try welcome" to "in lobby"
                    Task.Factory.StartNew(async () => {
                        //send welcome packet to client
                        var ret = new WelcomeAns();
                        ret.sId = mPeer.SessionId;
                        ret.SerWrite();
                        //change state after send complete 
                        if (await mPeer.OnSendTAP(ret))
                        {
                            logger.WriteDebug("Peer Change to IN_LOBBY");
                            mPeer.UpdateState(UserPeer.STATE.CHECK_AUTH);
                        }
                        else
                        {
                            //todo : action for closed session
                        }
                    });
                    break;
                default:
                    break;
            }
        }

        public override void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }

    public class UserPeer_IN_LOBBY : UserPeerState
    {
        public UserPeer_IN_LOBBY(UserPeer _p)
        {
            mPeer = _p;
        }


        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            // send chatting packet to everyone 
            switch (_mp.cType)
            {
                case MmoCore.Enums.CONTENT_TYPE.TEST:
                    break;
                case MmoCore.Enums.CONTENT_TYPE.CHAT:
                    Server.Inst.BroadCastAllUsers(_mp);
                    break;
                default:
                    break;
            }
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            //some req like join match queue?
            throw new NotImplementedException();
        }

        public override void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }

    public class UserPeer_IN_SELECT : UserPeerState
    {
        public UserPeer_IN_SELECT(UserPeer _p)
        {
            mPeer = _p;
        }


        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }

    public class UserPeer_IN_BATTLE: UserPeerState
    {
        public UserPeer_IN_BATTLE(UserPeer _p)
        {
            mPeer = _p;
        }


        public override void Dispatch_Ans(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Noti(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Req(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }

        public override void Dispatch_Test(MmoCorePacket _mp)
        {
            throw new NotImplementedException();
        }
    }
}
