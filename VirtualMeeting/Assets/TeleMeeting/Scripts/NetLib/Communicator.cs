using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class Communicator : MonoBehaviour
{
    private static Communicator _instance = null;

    public static Communicator Instance
    {
        get
        {
            return _instance;
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            _instance = this;

            this.gameObject.GetComponent<ASL.ASLObject>()._LocallySetFloatCallback(OnPacketRecieved);

            this._packets = new Dictionary<PacketList, string>();
            this._listeners = new Dictionary<PacketList, Action<BasePacket>>();

            // Packets
            this._packets.Add(PacketList.LOAD_SCENE, typeof(LoadScenePacket).Name);
            // End packets
        }
    }

    // A list of all packets we want to register
    private Dictionary<PacketList, string> _packets = null;

    // A dictionary mapping of all of our callback listeners
    private Dictionary<PacketList, Action<BasePacket>> _listeners = null;

    private Communicator()
    {
    }

    public bool SendPacket(BasePacket packet)
    {
        MemoryStream ms = new MemoryStream();
        BinaryWriter bw = new BinaryWriter(ms);

        // Serialize packet to our new writer
        packet.Serialize(bw);

        // Convert our byte array to a float array to send
        byte[] serialized_packet = ms.ToArray();
        float[] serialized_packet_float = new float[serialized_packet.Length + 2];

        Debug.Log("Queued new packet with ID " + packet.PacketID + ", size " + serialized_packet.Length);

        // Cannot send float arrays over 1K
        if (serialized_packet_float.Length > 1000)
        {
            return false;
        }

        serialized_packet_float[0] = (float) packet.PacketID;
        serialized_packet_float[1] = serialized_packet.Length;

        for (int i = 0; i < serialized_packet.Length; i++)
            serialized_packet_float[2 + i] = serialized_packet[i];

        this.gameObject.GetComponent<ASL.ASLObject>().SendAndSetClaim(() =>
        {
            this.gameObject.GetComponent<ASL.ASLObject>().SendFloatArray(serialized_packet_float);
        });

        Debug.Log("Sent packet with ID " + packet.PacketID);
        
        return true;
    }

    public void RegisterPacketListener(PacketList type, Action<BasePacket> callback)
    {
        Debug.Log("Added new packet listener for type " + type);
        this._listeners.Add(type, callback);
    }

    private void OnPacketRecieved(string _id, float[] _f)
    {
        PacketList id = (PacketList) _f[0];
        Debug.Log("Got packet with ID " + id);

        byte[] serialized_packet = new byte[(int) _f[1]];

        for (int i = 0; i < serialized_packet.Length; i++)
            serialized_packet[i] = (byte) _f[2 + i];

        object[] param = new object[1] { new BinaryReader(new MemoryStream(serialized_packet)) };

        // Deserialize the packet with the discovered ID
        BasePacket packet = (BasePacket) Type.GetType(this._packets[id]).GetMethod("Deserialize").Invoke(null, param);

        if (this._listeners.ContainsKey(id))
        {
            Debug.Log("Calling listener for packet with ID " + id);
            this._listeners[id](packet);
            return;
        }
        
        Debug.Log("No listener added for packet ID " + id);
    }
}
