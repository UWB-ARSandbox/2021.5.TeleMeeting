using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

abstract class BasePacket
{
    public PacketList PacketID { get; private set; }

    public BasePacket(PacketList packet_id)
    {
        this.PacketID = packet_id;
    }

    public abstract void Serialize(BinaryWriter serializationStream);
    
    public static BasePacket Deserialize(BinaryReader serializationStream)
    {
        _ = serializationStream;
        return null;
    }
}
