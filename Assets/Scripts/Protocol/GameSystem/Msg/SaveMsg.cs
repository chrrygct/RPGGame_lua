using System;
using System.Collections.Generic;
using System.Text;
namespace GameSystem
{
	public class SaveMsg : BaseMsg
	{
		public string account;
		public byte[] saveData;
		public override int GetBytesNum()
		{
			int num = 8;
			num += 4 + Encoding.UTF8.GetByteCount(account);
			num += 2;
			for (int i = 0; i < saveData.Length; ++i)
				num += 1;
			return num;
		}
		public override byte[] Writing()
		{
			int index = 0;
			byte[] bytes = new byte[GetBytesNum()];
			WriteInt(bytes, GetID(), ref index);
			WriteInt(bytes, bytes.Length - 8, ref index);
			WriteString(bytes, account, ref index);
			WriteShort(bytes, (short)saveData.Length, ref index);
			for (int i = 0; i < saveData.Length; ++i)
				WriteByte(bytes, saveData[i], ref index);
			return bytes;
		}
		public override int Reading(byte[] bytes, int beginIndex = 0)
		{
			int index = beginIndex;
			account = ReadString(bytes, ref index);
			short saveDataLength = ReadShort(bytes, ref index);
			saveData = new byte[saveDataLength];
			for (int i = 0; i < saveDataLength; ++i)
				saveData[i] = ReadByte(bytes, ref index);
			return index - beginIndex;
		}
		public override int GetID()
		{
			return 1007;
		}
	}
}