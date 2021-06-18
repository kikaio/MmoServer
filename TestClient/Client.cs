using CoreNet.Jobs;
using CoreNet.Networking;
using CoreNet.Protocols;
using CoreNet.Sockets;
using MmoCore.Packets;
using MmoCore.Protocols;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace TestClient
{
    class Client : CoreNetwork
    {
        public static Client Inst { get; private set; } = new Client();
        public TestSession mSession;
        private Dictionary<string, Worker> wDict = new Dictionary<string, Worker>();

        public override void ReadyToStart()
        {
            Translate.Init();
            MmoTranslate.Init();

            wDict["hb"] = new Worker("hb");
            wDict["recv"] = new Worker("recv");

            wDict["hb"].PushJob(new JobOnce(DateTime.UtcNow, () =>
            {
                while (isDown == false)
                {
                    Task.Delay(CoreSession.hbDelayMilliSec);
                }
            }));

            wDict["recv"].PushJob(new JobOnce(DateTime.UtcNow, async () => {
                while (isDown == false)
                {
                    var p = await mSession.OnRecvTAP();
                    if (p != default(Packet))
                    {
                        p.ReadPacketType();
                        var mp = new MmoCorePacket(p);
                        switch (mp.pType)
                        {
                            case Packet.PACKET_TYPE.NOTI:
                                mSession.Dispatch_Noti(mp);
                                break;
                            case Packet.PACKET_TYPE.REQ:
                                mSession.Dispatch_Req(mp);
                                break;
                            case Packet.PACKET_TYPE.ANS:
                                mSession.Dispatch_Ans(mp);
                                break;
                            case Packet.PACKET_TYPE.TEST:
                                mSession.Dispatch_Test(mp);
                                break;
                            default:
                                break;
                        }
                    }
                }
            }));
        }

        public override void Start()
        {
            string ipStr = "127.0.0.1";
            int port = 30000;

            CoreTCP s = new CoreTCP();
            s.Sock.Connect(new IPEndPoint(IPAddress.Parse(ipStr), port));
            mSession = new TestSession(-1, s);
            foreach (var w in wDict)
            {
                logger.WriteDebug($"worker[{w.Key}] is start");
                w.Value.WorkStart();
            }

            Task.Factory.StartNew(async () => {
                while(mSession.isWelcomed == false)
                {
                    HelloReq hp = new HelloReq();
                    hp.SerWrite();
                    await mSession.OnSendTAP(hp);
                    await Task.Delay(1000);
                }
            });
        }

        protected override void Analizer_Ans(CoreSession _s, Packet _p)
        {
            throw new NotImplementedException();
        }

        protected override void Analizer_Noti(CoreSession _s, Packet _p)
        {
            throw new NotImplementedException();
        }

        protected override void Analizer_Req(CoreSession _s, Packet _p)
        {
            throw new NotImplementedException();
        }

        protected override void Analizer_Test(CoreSession _s, Packet _p)
        {
            throw new NotImplementedException();
        }
    }
}
