using CoreNet.Jobs;
using CoreNet.Networking;
using CoreNet.Protocols;
using CoreNet.Sockets;
using CoreNet.Utils.Loggers;
using MmoCore.Packets;
using MmoCore.Protocols;
using MmoServer.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MmoServer
{
    using WorkerDict = Dictionary<string, Worker>;
    using CSToken = CancellationTokenSource;
    public class Server : CoreNetwork
    {
        CoreTCP mListener = new CoreTCP(AddressFamily.InterNetwork);
        WorkerDict mWorkerDict = new WorkerDict();
        CSToken shutdownTokenSource = new CSToken();

        public static Server Inst { get; private set; } = new Server();

        // port fix 30000
        public Server()
            : base("MMO")
        {
            logger = new ConsoleLogger();
            logger.WriteDebug("Server Start");
            shutdownAct = () => {
                logger.WriteDebugWarn("Server shutdown called");
                shutdownTokenSource.Cancel();
                foreach (var _w in mWorkerDict.Values.ToList())
                {
                    logger.WriteDebugWarn($"[worker{_w.workerName}] work fin requested");
                    _w.WorkFinish();
                }
                mWorkerDict.Clear();

                logger.WriteDebugWarn($"call CloseAllSession");
                SessionMgr.Inst.CloseAllSession();

                Console.Write("Server is down");
            };
        }

        private void ReadyTranslate()
        {
            //coreNet translate init
            Translate.Init();

            //mmoCore translate init
            MmoTranslate.Init();
        }

        private void BindAndListen()
        {
            ep = new IPEndPoint(IPAddress.Any, port);
            mListener.Sock.NoDelay = true;
            mListener.Sock.Bind(ep);
            mListener.Sock.Listen(100);
        }

        private void HbCheck()
        {
            var expireList = new List<CoreSession>();
            foreach (var ele in SessionMgr.Inst.ToSessonList())
            {
                if (ele.Sock.Sock.Connected == false)
                    expireList.Add(ele);
                if (ele.HeartBeat < DateTime.UtcNow)
                    expireList.Add(ele);
            }

            foreach (var ele in expireList)
            {
                var removed = default(CoreSession);
                SessionMgr.Inst.CloseSession(ele.SessionId, out removed);
                if (removed != null)
                {
                    logger.WriteDebugWarn($"session {ele.SessionId} is expired");
                }
            }
        }

        private void ReadyWorkers()
        {
            mWorkerDict["pkg"] = new Worker("pkg", true);
            //mWorkerDict["cmd"] = new Worker("cmd", true);
            
            //todo : make personal packageQ thread
            var pkgJob = new JobInfinity(async () => {
                if (IsShutdownRequested())
                    return;
                packageQ.Swap();
                while (true)
                {
                    var pkg = packageQ.pop();
                    if (pkg == default(Package))
                        break;
                    await Task.Factory.StartNew(async () => {
                        PackageDispatcher(pkg);
                    });
                }
                HbCheck();
            });

            //var cmdJob = new JobOnce(DateTime.MinValue, async () =>
            //{
            //    while (shutdownTokenSource.IsCancellationRequested == false)
            //    {
            //        string inputs = Console.ReadLine().ToUpper();
            //        string[] cmds = inputs.Split(' ');
            //        if (cmds.Length < 1)
            //            return;
            //        switch (cmds[0])
            //        {
            //            case "EXIT":
            //                shutdownAct?.Invoke();
            //                break;
            //        }
            //    }
            //});

            mWorkerDict["pkg"].PushJob(pkgJob);
            //mWorkerDict["cmd"].PushJob(cmdJob);
        }

        public override void ReadyToStart()
        {
            ReadyTranslate();
            BindAndListen();
            ReadyWorkers();
        }

        public override void Start()
        {
            Task.Factory.StartNew(()=> {
                logger.WriteDebug($"Session Accept Task start ");
                while (shutdownTokenSource.IsCancellationRequested == false)
                {
                    var sock = mListener.Sock.Accept();
                    var newSid = SessionMgr.Inst.GetNextSessionId();
                    var s = new UserPeer(newSid, new CoreTCP(sock));
                    SessionMgr.Inst.AddSession(s);
                    logger.WriteDebug($"New Session Connected : {newSid}");
                    Task.Factory.StartNew(async () => {
                        logger.WriteDebug($"{s.SessionId} Recv Start");
                        while (shutdownTokenSource.IsCancellationRequested == false)
                        {
                            if (s.Sock.Sock.Connected == false)
                            {
                                break;
                            }
                            Packet p = await s.OnRecvTAP();
                            if (p != default(Packet))
                            {
                                //todo : 추후에 개별 dispatcher로 구분할 것.
                                if (p.GetHeader() == 0)
                                    s.UpdateHeartBeat();
                                else
                                {
                                    logger.WriteDebug("packet recved");
                                    packageQ.Push(new Package(s, p));
                                }
                            }
                        }
                    }, TaskCreationOptions.DenyChildAttach);
                }
            }, TaskCreationOptions.DenyChildAttach);

            foreach (var ele in mWorkerDict)
            {
                logger.WriteDebug($"{ele.Key} work is start");
                ele.Value.WorkStart();
            }
        }

        protected override void Analizer_Ans(CoreSession _s, Packet _p)
        {
            MmoCorePacket mp = new MmoCorePacket(_p);
            var up = (UserPeer)_s;
            up.Dispatch_Ans(mp);
        }

        protected override void Analizer_Noti(CoreSession _s, Packet _p)
        {
            MmoCorePacket mp = new MmoCorePacket(_p);
            var up = (UserPeer)_s;
            up.Dispatch_Noti(mp);
        }

        protected override void Analizer_Req(CoreSession _s, Packet _p)
        {
            MmoCorePacket mp = new MmoCorePacket(_p);
            var up = (UserPeer)_s;
            up.Dispatch_Req(mp);
        }

        protected override void Analizer_Test(CoreSession _s, Packet _p)
        {
            MmoCorePacket mp = new MmoCorePacket(_p);
            var up = (UserPeer)_s;
            up.Dispatch_Test(mp);
        }

        public bool IsShutdownRequested()
        {
            return shutdownTokenSource.IsCancellationRequested;
        }

        public void BroadCastAllUsers(MmoCorePacket _mp)
        {
            Task.Factory.StartNew(async (_obj) => {
                foreach (var s in SessionMgr.Inst.ToSessonList())
                {
                    MmoCorePacket copyPacket = new MmoCorePacket(_mp);
                    if (s.Sock.Sock.Connected == false)
                        continue;
                    await s.OnSendTAP(copyPacket);
                }
            }, _mp);
        }
    }
}
