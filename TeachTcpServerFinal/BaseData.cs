using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

// 服务端副本：内容与客户端 Assets/Scripts/Net/Base/BaseData.cs 一致，仅去掉 using UnityEngine（控制台工程无此程序集）。
public abstract class BaseData
{
    public abstract int GetBytesNum();

    public abstract byte[] Writing();

    public abstract int Reading(byte[] bytes, int beginIndex = 0);

    protected void WriteInt(byte[] bytes, int value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(int);
    }
    protected void WriteShort(byte[] bytes, short value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(short);
    }
    protected void WriteLong(byte[] bytes, long value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(long);
    }
    protected void WriteFloat(byte[] bytes, float value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(float);
    }
    protected void WriteByte(byte[] bytes, byte value, ref int index)
    {
        bytes[index] = value;
        index += sizeof(byte);
    }
    protected void WriteBool(byte[] bytes, bool value, ref int index)
    {
        BitConverter.GetBytes(value).CopyTo(bytes, index);
        index += sizeof(bool);
    }
    protected void WriteString(byte[] bytes, string value, ref int index)
    {
        byte[] strBytes = Encoding.UTF8.GetBytes(value);
        WriteInt(bytes, strBytes.Length, ref index);
        strBytes.CopyTo(bytes, index);
        index += strBytes.Length;
    }
    protected void WriteData(byte[] bytes, BaseData data, ref int index)
    {
        data.Writing().CopyTo(bytes, index);
        index += data.GetBytesNum();
    }

    protected int ReadInt(byte[] bytes, ref int index)
    {
        int value = BitConverter.ToInt32(bytes, index);
        index += sizeof(int);
        return value;
    }
    protected short ReadShort(byte[] bytes, ref int index)
    {
        short value = BitConverter.ToInt16(bytes, index);
        index += sizeof(short);
        return value;
    }
    protected long ReadLong(byte[] bytes, ref int index)
    {
        long value = BitConverter.ToInt64(bytes, index);
        index += sizeof(long);
        return value;
    }
    protected float ReadFloat(byte[] bytes, ref int index)
    {
        float value = BitConverter.ToSingle(bytes, index);
        index += sizeof(float);
        return value;
    }
    protected byte ReadByte(byte[] bytes, ref int index)
    {
        byte value = bytes[index];
        index += sizeof(byte);
        return value;
    }
    protected bool ReadBool(byte[] bytes, ref int index)
    {
        bool value = BitConverter.ToBoolean(bytes, index);
        index += sizeof(bool);
        return value;
    }
    protected string ReadString(byte[] bytes, ref int index)
    {
        int length = ReadInt(bytes, ref index);
        string value = Encoding.UTF8.GetString(bytes, index, length);
        index += length;
        return value;
    }
    protected T ReadData<T>(byte[] bytes, ref int index) where T : BaseData, new()
    {
        T value = new T();
        index += value.Reading(bytes, index);
        return value;
    }
}
