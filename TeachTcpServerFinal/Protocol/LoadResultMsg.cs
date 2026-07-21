using System;
using System.Collections.Generic;
using System.Text;
namespace GameSystem
{
	public class LoadResultMsg : BaseMsg
	{
		public int result;
		public string info;
		public byte[] saveData;
		public override int GetBytesNum()
		{
			int num = 8;
			num += 4;
			num += 4 + Encoding.UTF8.GetByteCount(info);
			num += 2;
			if (saveData != null)
				num += saveData.Length;
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
			WriteShort(bytes, (short)(saveData != null ? saveData.Length : 0), ref index);
			if (saveData != null)
				for (int i = 0; i < saveData.Length; ++i)
					WriteByte(bytes, saveData[i], ref index);
			return bytes;
		}
		public override int Reading(byte[] bytes, int beginIndex = 0)
		{
			int index = beginIndex;
			result = ReadInt(bytes, ref index);
			info = ReadString(bytes, ref index);
			short saveDataLength = ReadShort(bytes, ref index);
			saveData = new byte[saveDataLength];
			for (int i = 0; i < saveDataLength; ++i)
				saveData[i] = ReadByte(bytes, ref index);
			return index - beginIndex;
		}
		public override int GetID()
		{
			return 1010;
		}
	}
}
