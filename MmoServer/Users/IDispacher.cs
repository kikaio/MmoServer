using MmoCore.Packets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MmoServer.Users
{
    public interface IDIspatcher
    {
        void Dispatch_Ans(MmoCorePacket _mp);
        void Dispatch_Req(MmoCorePacket _mp);
        void Dispatch_Noti(MmoCorePacket _mp);
        void Dispatch_Test(MmoCorePacket _mp);
    }
}
