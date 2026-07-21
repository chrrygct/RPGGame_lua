using System;
using System.Collections.Generic;
using System.Text;
namespace GameSystem
{
	public class LoadMsg : BaseMsg
	{
		public string account;
		public override int GetBytesNum()
		{
			int num = 8;
			num += 4 + Encoding.UTF8.GetByteCount(account);
			return num;
		}
		public override byte[] Writing()
		{
			int index = 0;
			byte[] bytes = new byte[GetBytesNum()];
			WriteInt(bytes, GetID(), ref index);
			WriteInt(bytes, bytes.Length - 8, ref index);
			WriteString(bytes, account, ref index);
			return bytes;
		}
		public override int Reading(byte[] bytes, int beginIndex = 0)
		{
			int index = beginIndex;
			account = ReadString(bytes, ref index);
			return index - beginIndex;
		}
		public override int GetID()
		{
			return 1009;
		}
	}
}