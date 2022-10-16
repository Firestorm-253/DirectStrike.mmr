using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Serialization;

public class PacketWriter : BinaryWriter
{
    private MemoryStream _ms;

    private BinaryFormatter _bf;

    public PacketWriter()
    {
        _ms = new MemoryStream();
        _bf = new BinaryFormatter();
        OutStream = _ms;
    }

    public void WriteT(object obj)
    {
        _bf.Serialize(_ms, obj);
    }

    public byte[] GetBytes(bool clear = false, bool close = true)
    {
        byte[] bytes = _ms.ToArray();
        if (close) {
            Close();
        } else if (clear) {
            this.Clear();
        }
        return bytes;
    }

    public void Clear()
    {
        //var buffer = this._ms.GetBuffer();
        //Array.Clear(buffer, 0, buffer.Length);
        this._ms.Position = 0;
        this._ms.SetLength(0);
        this._ms.Capacity = 0; // <<< this one ******
    }
}

public class PacketReader : BinaryReader
{
    private BinaryFormatter _bf;
    public PacketReader(byte[] data)
        : base(new MemoryStream(data))
    {
        _bf = new BinaryFormatter();
    }

    public T ReadObject<T>()
    {
        return (T)_bf.Deserialize(BaseStream);
    }
}