using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

class LoadScenePacket : BasePacket
{
    public string SceneID { get; private set; }

    public LoadScenePacket(string sceneID)
        : base(PacketList.LOAD_SCENE)
    {
        this.SceneID = sceneID;
    }

    public static new LoadScenePacket Deserialize(BinaryReader serializationStream)
    {
        return new LoadScenePacket(serializationStream.ReadString());
    }

    public override void Serialize(BinaryWriter serializationStream)
    {
        serializationStream.Write(this.SceneID);
    }
}
