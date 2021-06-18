using CoreNet.Utils;
using CoreNet.Utils.Loggers;
using MmoServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MmoServer
{
    class Program
    {
        static void Main(string[] args)
        {
            CoreLogger logger = new ConsoleLogger();

            Server.Inst.ReadyToStart();
            Server.Inst.Start();
            Task.Factory.StartNew(async () => {

                var data = await IPUtils.GetIPData();
                data.public_port = 30000;
                data.private_port = 30000;
            });

            while (Server.Inst.IsShutdownRequested() == false)
            {
                Thread.Sleep(1000);
            }
            logger.WriteDebug("programe down, press any key");
        }
    }
}
