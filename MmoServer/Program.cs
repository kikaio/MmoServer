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
            CoreLogger logger = new Log4Logger();

            Server mServer = new Server();
            mServer.ReadyToStart();
            mServer.Start();
            Task.Factory.StartNew(async () => {

                var data = await IPUtils.GetIPData();
                data.public_port = 30000;
                data.private_port = 30000;
            });

            while (mServer.IsShutdownRequested() == false)
            {
                Thread.Sleep(1000);
            }
            Console.WriteLine("programe down, press any key");
        }
    }
}
