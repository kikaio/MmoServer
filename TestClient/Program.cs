using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace TestClient
{
    class Program
    {
        static void Main(string[] args)
        {
            Client.Inst.ReadyToStart();
            Client.Inst.Start();

            while (Client.Inst.isDown == false)
            {
                Thread.Sleep(1000);
            }

            Console.WriteLine("test client is down");
        }
    }
}
