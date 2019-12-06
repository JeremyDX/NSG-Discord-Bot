using System.IO;
using System;
using System.Text;

public class CacheReader
{
	private static BinaryReader stream;
	private static BinaryWriter writer;

	private byte[] buffer;
	private int readerIndex = 0;
	private int writerIndex = 0;

	public void SkipBytes(int skipped)
	{
		writerIndex += skipped;
	}

	public void SeekInTo(int index)
	{
		readerIndex += index;
	}

	public void Seek(int index)
	{
		readerIndex = index;
	}

	public int ReadableBytes()
	{
		return (buffer.Length - readerIndex);
	}

	public int ReaderIndex()
	{
		return readerIndex;
	}

	public int WriterIndex()
	{
		return writerIndex;
	}

	public int Capacity()
	{
		return buffer.Length;
	}

	public int GetBasePosition()
	{
		return (int)stream.BaseStream.Position;
	}

	public byte[] Array()
	{
		return buffer;
	}

	public CacheReader()
	{
		Open();
	}

	public CacheReader(int length, int capacity)
	{
		readerIndex = length;
		writerIndex = length;
		buffer = new byte[capacity];
	}

	public CacheReader(byte[] buffer, int length)
	{
		this.buffer = buffer;
		this.writerIndex = length;
	}

	public int WriteLength()
	{
		int size = writerIndex;
		writerIndex = 0;
		WriteShort(size);
		return size;
	}

	public void WriteBoolean(bool b)
	{
		buffer[writerIndex++] = (byte)(b ? 1 : 0);
	}

	public void WriteByte(byte data)
	{
		buffer[writerIndex++] = data;
	}

	public void WriteShort(int data)
	{
		buffer[writerIndex++] = (byte)(data >> 8);
		buffer[writerIndex++] = (byte)(data);
	}

	public void WriteInt(int data)
	{
		buffer[writerIndex++] = (byte)(data >> 24);
		buffer[writerIndex++] = (byte)(data >> 16);
		buffer[writerIndex++] = (byte)(data >> 8);
		buffer[writerIndex++] = (byte)(data);
	}

	public void WriteLong(long data)
	{
		buffer[writerIndex++] = (byte)(data >> 56);
		buffer[writerIndex++] = (byte)(data >> 48);
		buffer[writerIndex++] = (byte)(data >> 40);
		buffer[writerIndex++] = (byte)(data >> 32);
		buffer[writerIndex++] = (byte)(data >> 24);
		buffer[writerIndex++] = (byte)(data >> 16);
		buffer[writerIndex++] = (byte)(data >> 8);
		buffer[writerIndex++] = (byte)(data);
	}

	public bool WriteLongNotContained(ulong data)
	{
		try
		{
			int length = writerIndex / 8;
			readerIndex = 0;
			for (int j = 0; j < length; ++j)
			{
				if (ReadLong() == (long)(data))
					return false;
			}
			WriteLong((long)data);
			Save();
			return true;
		}
		catch (Exception e)
		{
			return false;
			System.Console.WriteLine("Error: " + e.StackTrace.ToString());
		}
	}

	public void WriteString(string s)
	{
		WriteString(s.ToCharArray());
	}

	public void WriteString(char[] data)
	{
		for (int i = 0; i < data.Length; i++)
			buffer[writerIndex++] = (byte)data[i];
		buffer[writerIndex++] = 0;
	}

	public bool ReadBoolean()
	{
		return buffer[readerIndex++] == 1;
	}

	public byte ReadByte()
	{
		return buffer[readerIndex++];
	}

	public sbyte ReadSignedByte()
	{
		return (sbyte)buffer[readerIndex++];
	}

	public int ReadShort()
	{
		return (ReadByte() << 8) | ReadByte();
	}

	public int ReadSignedShort()
	{
		return (ReadSignedByte() << 8) | ReadByte();
	}

	public int ReadInt()
	{
		return (ReadByte() << 24) | (ReadByte() << 16) | (ReadByte() << 8) | ReadByte();
	}

	public int ReadSignedInt()
	{
		return (ReadSignedByte() << 24) | (ReadSignedByte() << 16) | (ReadSignedByte() << 8) | ReadByte();
	}

	public long ReadLong()
	{
		return ((0xffffffffL & (long)ReadInt()) << 32) + (0xffffffffL & (long)ReadInt());
	}

	public string ReadString()
	{
		StringBuilder sb = new StringBuilder();
		byte b;
		while ((b = buffer[readerIndex++]) != 0)
		{
			sb.Append((char)b);
		}
		return sb.ToString();
	}

	public char[] ReadCharArray()
	{
		int src = readerIndex;
		while (buffer[readerIndex++] != 0) ;
		char[] c = new char[readerIndex - src];
		for (int i = 0; i < c.Length; ++i)
			c[i] = (char)buffer[src + i];
		return c;
	}

	public void Save()
	{
		FileStream fs = File.OpenWrite("cache.dat");
		writer = new BinaryWriter(fs);
		Console.WriteLine("Size: " + buffer.Length);
		writer.Write(buffer, 0, writerIndex);
		writer.Close();
		writer.Dispose();
	}

	public static uint Open()
	{
		FileStream fs = File.OpenRead("cache.dat");
		stream = new BinaryReader(fs);
		return (uint)stream.BaseStream.Length;
	}

	public static void Close()
	{
		stream.Close();
		stream.Dispose();
	}

	public static CacheReader SeekCache(int seek, int length, int capacity)
	{
		byte[] bytes = new byte[length + capacity];
		stream.BaseStream.Position = seek;
		stream.Read(bytes, 0, length);
		return new CacheReader(bytes, length);
	}

	public static CacheReader SeekCacheSizeInt()
	{
		int length = 0x4;
		byte[] bytes = new byte[length];
		stream.Read(bytes, 0, length);
		length = ReadInt(bytes);
		bytes = new byte[length];
		stream.Read(bytes, 0, length);
		return new CacheReader(bytes, length);
	}

	public static CacheReader SeekCacheSizeInt(int seek)
	{
		int length = 0x4;
		byte[] bytes = new byte[length];
		stream.BaseStream.Position = seek;
		stream.Read(bytes, 0, length);
		length = ReadInt(bytes);
		stream.BaseStream.Position = length;
		stream.Read(bytes, 0, 0x4);
		length = ReadInt(bytes);
		bytes = new byte[length];
		stream.Read(bytes, 0, length);
		return new CacheReader(bytes, length);
	}

	public static CacheReader SeekCacheSizeShort(int seek)
	{
		int length = 0x2;
		byte[] bytes = new byte[length];
		stream.BaseStream.Position = seek;
		stream.Read(bytes, 0, length);
		length = ReadShort(bytes);
		bytes = new byte[length];
		stream.BaseStream.Position = seek + 0x2;
		stream.Read(bytes, 0, length);
		return new CacheReader(bytes, length);
	}

	public static CacheReader slice(int seek)
	{
		stream.BaseStream.Position = seek;
		byte[] bytes = new byte[0x4];
		stream.Read(bytes, 0, 4);
		int length = ReadInt(bytes);
		stream.BaseStream.Position = length;
		stream.Read(bytes, 0, 4);
		length = ReadInt(bytes);
		bytes = new byte[length];
		stream.Read(bytes, 0, length);
		return new CacheReader(bytes, length);
	}

	private static int ReadShort(byte[] b)
	{
		return ((b[0x0] & 0xFF) << 0x8) | (b[0x1] & 0xFF);
	}

	private static int ReadInt(byte[] b)
	{
		return ((b[0x0] & 0xFF) << 0x18) | ((b[0x1] & 0xFF) << 0x10) | ((b[0x2] & 0xFF) << 0x8) | (b[0x3] & 0xFF);
	}

}