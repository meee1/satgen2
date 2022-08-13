using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace Racelogic.Core;

public class TripleDESEncryption
{
	public void Encrypt(MemoryStream fin, string outName, byte[] tdesKey, byte[] tdesIV)
	{
		using FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
		EncryptData(fin, fout, tdesKey, tdesIV);
	}

	public byte[] Encrypt(MemoryStream fin, byte[] tdesIV)
	{
		byte[] array = null;
		byte[] tdesKey = null;
		byte[] tdesIV2 = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV2);
		byte[] buffer = new byte[100];
		long num = 0L;
		long length = fin.Length;
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using MemoryStream memoryStream = new MemoryStream();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESCryptoServiceProvider.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
		int num2;
		for (; num < length; num += num2)
		{
			num2 = fin.Read(buffer, 0, 100);
			cryptoStream.Write(buffer, 0, num2);
		}
		cryptoStream.FlushFinalBlock();
		memoryStream.Flush();
		memoryStream.Position = 0L;
		return memoryStream.ToArray();
	}

	public void Encrypt(MemoryStream fin, string outName)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		Encrypt(fin, outName, tdesKey, tdesIV);
	}

	public void Encrypt(string inName, string outName, byte[] tdesKey, byte[] tdesIV)
	{
		using FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
		using FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
		EncryptData(fin, fout, tdesKey, tdesIV);
	}

	public void Encrypt(string inName, string outName)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		Encrypt(inName, outName, tdesKey, tdesIV);
	}

	public byte[] Encrypt(byte[] dataIn)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		return Encrypt(dataIn, tdesKey, tdesIV);
	}

	public byte[] Encrypt(byte[] dataIn, byte[] tdesKey, byte[] tdesIV)
	{
		byte[] buffer = new byte[100];
		long num = 0L;
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using MemoryStream memoryStream = new MemoryStream(dataIn);
		long length = memoryStream.Length;
		using MemoryStream memoryStream2 = new MemoryStream();
		memoryStream2.SetLength(0L);
		using CryptoStream cryptoStream = new CryptoStream(memoryStream2, tripleDESCryptoServiceProvider.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
		int num2;
		for (; num < length; num += num2)
		{
			num2 = memoryStream.Read(buffer, 0, 100);
			cryptoStream.Write(buffer, 0, num2);
		}
		cryptoStream.FlushFinalBlock();
		memoryStream2.Position = 0L;
		byte[] array = new byte[memoryStream2.Length];
		memoryStream2.Read(array, 0, (int)memoryStream2.Length);
		return array;
	}

	private void EncryptData(Stream fin, FileStream fout, byte[] tdesKey, byte[] tdesIV)
	{
		fout.SetLength(0L);
		byte[] buffer = new byte[100];
		long num = 0L;
		long length = fin.Length;
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using CryptoStream cryptoStream = new CryptoStream(fout, tripleDESCryptoServiceProvider.CreateEncryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
		fin.Position = 0L;
		int num2;
		for (; num < length; num += num2)
		{
			num2 = fin.Read(buffer, 0, 100);
			cryptoStream.Write(buffer, 0, num2);
		}
		cryptoStream.FlushFinalBlock();
	}

	public void Decrypt(string inName, MemoryStream fout, byte[] tdesKey, byte[] tdesIV)
	{
		using FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
		DecryptData(fin, fout, tdesKey, tdesIV);
	}

	public void Decrypt(string inName, MemoryStream fout)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		using FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
		DecryptData(fin, fout, tdesKey, tdesIV);
	}

	public void Decrypt(string inName, string outName, byte[] tdesKey, byte[] tdesIV)
	{
		using FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
		using FileStream fout = new FileStream(outName, FileMode.OpenOrCreate, FileAccess.Write);
		DecryptData(fin, fout, tdesKey, tdesIV);
	}

	public void Decrypt(string inName, string outName)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		using FileStream fin = new FileStream(inName, FileMode.Open, FileAccess.Read);
		using FileStream fout = new FileStream(outName, FileMode.Create, FileAccess.Write);
		DecryptData(fin, fout, tdesKey, tdesIV);
	}

	public List<byte> Decrypt(List<byte> data)
	{
		return Decrypt(data.ToArray()).ToList();
	}

	public byte[] Decrypt(byte[] data)
	{
		byte[] tdesKey = null;
		byte[] tdesIV = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV);
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = tdesKey;
		tripleDESCryptoServiceProvider.IV = tdesIV;
		byte[] result = tripleDESCryptoServiceProvider.CreateDecryptor().TransformFinalBlock(data, 0, data.Length);
		tripleDESCryptoServiceProvider.Clear();
		return result;
	}

	public byte[] Decrypt(Stream fin, byte[] tdesIV)
	{
		byte[] array = null;
		byte[] tdesKey = null;
		byte[] tdesIV2 = null;
		CreateKeyAndIV(ref tdesKey, ref tdesIV2);
		using MemoryStream memoryStream = new MemoryStream();
		byte[] buffer = new byte[100];
		long num = 0L;
		long length = fin.Length;
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESCryptoServiceProvider.CreateDecryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
		int num2;
		for (; num < length; num += num2)
		{
			num2 = fin.Read(buffer, 0, 100);
			cryptoStream.Write(buffer, 0, num2);
		}
		cryptoStream.FlushFinalBlock();
		return memoryStream.ToArray();
	}

	public byte[] Decrypt(byte[] dataIn, byte[] tdesKey, byte[] tdesIV)
	{
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		tripleDESCryptoServiceProvider.Key = tdesKey;
		tripleDESCryptoServiceProvider.IV = tdesIV;
		byte[] result = tripleDESCryptoServiceProvider.CreateDecryptor().TransformFinalBlock(dataIn, 0, dataIn.Length);
		tripleDESCryptoServiceProvider.Clear();
		return result;
	}

	private void DecryptData(Stream fin, Stream fout, byte[] tdesKey, byte[] tdesIV)
	{
		fout.SetLength(0L);
		using MemoryStream memoryStream = new MemoryStream();
		byte[] buffer = new byte[100];
		long num = 0L;
		long length = fin.Length;
		TripleDESCryptoServiceProvider tripleDESCryptoServiceProvider = new TripleDESCryptoServiceProvider();
		using CryptoStream cryptoStream = new CryptoStream(memoryStream, tripleDESCryptoServiceProvider.CreateDecryptor(tdesKey, tdesIV), CryptoStreamMode.Write);
		fin.Position = 0L;
		try
		{
			int num2;
			for (; num < length; num += num2)
			{
				num2 = fin.Read(buffer, 0, 100);
				cryptoStream.Write(buffer, 0, num2);
			}
		}
		catch
		{
		}
		cryptoStream.FlushFinalBlock();
		memoryStream.WriteTo(fout);
	}

	private void CreateKeyAndIV(ref byte[] tdesKey, ref byte[] tdesIV)
	{
		TripleDES.Create();
		tdesKey = new byte[24]
		{
			44, 20, 207, 250, 76, 94, 111, 246, 137, 138,
			141, 30, 214, 35, 153, 63, 69, 8, 200, 184,
			144, 76, 109, 229
		};
		tdesIV = new byte[8] { 87, 81, 199, 101, 180, 246, 239, 99 };
	}
}
