// 服务端副本：内容与客户端 Assets/Scripts/Net/Base/BaseMsg.cs 一致，仅去掉 using UnityEngine。
public class BaseMsg : BaseData
{
    public override int GetBytesNum()
    {
        throw new System.NotImplementedException();
    }

    public override int Reading(byte[] bytes, int beginIndex = 0)
    {
        throw new System.NotImplementedException();
    }

    public override byte[] Writing()
    {
        throw new System.NotImplementedException();
    }

    public virtual int GetID()
    {
        return 0;
    }
}
