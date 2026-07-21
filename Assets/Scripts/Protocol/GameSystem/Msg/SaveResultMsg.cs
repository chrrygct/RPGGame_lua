using System;
using System.Collections.Generic;
using System.Text;
namespace GameSystem
{
	public class SaveResultMsg : BaseMsg
	{
		public int result;
		public string info;
		public override int GetBytesNum()
		{
			int num = 8;
			num += 4;
			num += 4 + Encoding.UTF8.GetByteCount(info);
			return num;
		}
		public override byte[] Writing()
		{
			int index = 0;
			byte[] bytes = new byte[GetBytesNum()];
			WriteInt(bytes, GetID(), ref index);
			WriteInt(bytes, bytes.Length - 8, ref index);
			WriteInt(bytes, result, ref index);
			WriteString(bytes, info, ref index);
			return bytes;
		}
		public override int Reading(byte[] bytes, int beginIndex = 0)
		{
			int index = beginIndex;
			result = ReadInt(bytes, ref index);
			info = ReadString(bytes, ref index);
			return index - beginIndex;
		}
		public override int GetID()
		{
			return 1008;
		}
	}
}