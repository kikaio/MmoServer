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
            logger = new Log4Logger();
            logger.WriteDebug("Server Start");
            ep = new IPEndPoint(IPAddress.Any, port);
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
            mListener.Sock.Bind(ep);
            mListener.Sock.Listen(100);
        }
        private void ReadyWorkers()
        {
            mWorkerDict["pkg"] = new Worker("pkg", true);
            mWorkerDict["hb"] = new Worker("hb", true);
            mWorkerDict["cmd"] = new Worker("cmd", true);

            //todo : make personal packageQ thread
            var pkgJob = new JobOnce(DateTime.MinValue, () => {
                while (shutdownTokenSource.IsCancellationRequested == false)
                {
                    packageQ.Swap();
                    var pkg = packageQ.pop();
                    while (pkg != default(Package))
                    {
                        //do async dispatch for each package
                        Task.Factory.StartNew(async() => {
                            PackageDispatcher(pkg);
                        });
                        pkg = packageQ.pop();
                    }
                    Thread.Sleep(100);
                }
            });

            long hbCehckTicks = TimeSpan.FromMilliseconds(CoreSession.hbDelayMilliSec).Ticks;
            var hbJob = new JobNormal(DateTime.MinValue, DateTime.MaxValue
                , hbCehckTicks, () => {
                    if (shutdownTokenSource.IsCancellationRequested)
                        return;
                    var delList = new List<CoreSession>();
                    foreach (var s in SessionMgr.Inst.ToSessonList())
                    {
                        if (s.HeartBeat < DateTime.UtcNow)
                            delList.Add(s);
                    }

                    foreach (var s in delList)
                    {
                        var del = default(CoreSession);
                        if (SessionMgr.Inst.ForceCloseSession(s.SessionId, out del) == false)
                            logger.Error($"session[{s.SessionId}] force close is failed");
                    }
                });
            var cmdJob = new JobOnce(DateTime.MinValue, () =>
            {
                while (shutdownTokenSource.IsCancellationRequested == false)
                {
                    string inputs = Console.ReadLine().ToUpper();
                    string[] cmds = inputs.Split(' ');
                    if (cmds.Length < 1)
                        return;
                    switch (cmds[0])
                    {
                        case "EXIT":
                            shutdownAct?.Invoke();
                            break;
                    }
                }
            });

            mWorkerDict["pkg"].PushJob(pkgJob);
            mWorkerDict["hb"].PushJob(hbJob);
            mWorkerDict["cmd"].PushJob(cmdJob);
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
                while (shutdownTokenSource.IsCancellationRequested == false)
                {
                    var sock = mListener.Sock.Accept();
                    var newSid = SessionMgr.Inst.GetNextSessionId();
                    var s = new UserPeer(newSid, new CoreTCP(sock));
                    SessionMgr.Inst.AddSession(s);
                    Task.Factory.StartNew(async () => {
                        while (shutdownTokenSource.IsCancellationRequested == false)
                        {
                            if (s.Sock.Sock.Connected == false)
                                break;
                            Packet p = await s.OnRecvTAP();
                            if (p != default(Packet))
                            {
                                //todo : 추후에 개별 dispatcher로 구분할 것.
                                packageQ.Push(new Package(s, p));
                            }
                        }
                    }, TaskCreationOptions.DenyChildAttach);
                }
            });

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
    }
}
