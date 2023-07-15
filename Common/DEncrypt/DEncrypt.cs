using System;
using System.IO;
using System.Security.Cryptography;  
using System.Text;
namespace Roim.Common.DEncrypt
{
	/// <summary>
	/// Encrypt 的摘要说明。
    /// Copyright (C) c84f4f52-fbf3-4411-ad92-d4864d4e24d2
	/// </summary>
	public class DEncrypt
	{
		/// <summary>
		/// 构造方法
		/// </summary>
		public DEncrypt()  
		{  
		} 

		#region 使用 缺省密钥字符串 加密/解密string

		/// <summary>
		/// 使用缺省密钥字符串加密string
		/// </summary>
		/// <param name="original">明文</param>
		/// <returns>密文</returns>
		public static string Encrypt(string original)
		{
			return Encrypt(original,"c84f4f52-fbf3-4411-ad92-d4864d4e24d2");
		}
		/// <summary>
		/// 使用缺省密钥字符串解密string
		/// </summary>
		/// <param name="original">密文</param>
		/// <returns>明文</returns>
		public static string Decrypt(string original)
		{
			return Decrypt(original,"c84f4f52-fbf3-4411-ad92-d4864d4e24d2",System.Text.Encoding.Default);
		}

		#endregion

		#region 使用 给定密钥字符串 加密/解密string
		/// <summary>
		/// 使用给定密钥字符串加密string
		/// </summary>
		/// <param name="original">原始文字</param>
		/// <param name="key">密钥</param>
		/// <param name="encoding">字符编码方案</param>
		/// <returns>密文</returns>
		public static string Encrypt(string original, string key)  
		{
            if (original == null)
                return null;
			byte[] buff = System.Text.Encoding.Default.GetBytes(original);  
			byte[] kb = System.Text.Encoding.Default.GetBytes(key);
			return Convert.ToBase64String(Encrypt(buff,kb));      
		}
		/// <summary>
		/// 使用给定密钥字符串解密string
		/// </summary>
		/// <param name="original">密文</param>
		/// <param name="key">密钥</param>
		/// <returns>明文</returns>
		public static string Decrypt(string original, string key)
		{
			return Decrypt(original,key,System.Text.Encoding.Default);
		}

		/// <summary>
		/// 使用给定密钥字符串解密string,返回指定编码方式明文
		/// </summary>
		/// <param name="encrypted">密文</param>
		/// <param name="key">密钥</param>
		/// <param name="encoding">字符编码方案</param>
		/// <returns>明文</returns>
		public static string Decrypt(string encrypted, string key,Encoding encoding)  
		{       
			byte[] buff = Convert.FromBase64String(encrypted);  
			byte[] kb = System.Text.Encoding.Default.GetBytes(key);
			return encoding.GetString(Decrypt(buff,kb));      
		}  
		#endregion

		#region 使用 缺省密钥字符串 加密/解密/byte[]
		/// <summary>
		/// 使用缺省密钥字符串解密byte[]
		/// </summary>
		/// <param name="encrypted">密文</param>
		/// <param name="key">密钥</param>
		/// <returns>明文</returns>
		public static byte[] Decrypt(byte[] encrypted)  
		{  
			byte[] key = System.Text.Encoding.Default.GetBytes("c84f4f52-fbf3-4411-ad92-d4864d4e24d2"); 
			return Decrypt(encrypted,key);     
		}
		/// <summary>
		/// 使用缺省密钥字符串加密
		/// </summary>
		/// <param name="original">原始数据</param>
		/// <param name="key">密钥</param>
		/// <returns>密文</returns>
		public static byte[] Encrypt(byte[] original)  
		{  
			byte[] key = System.Text.Encoding.Default.GetBytes("c84f4f52-fbf3-4411-ad92-d4864d4e24d2"); 
			return Encrypt(original,key);     
		}  
		#endregion

		#region  使用 给定密钥 加密/解密/byte[]

		/// <summary>
		/// 生成MD5摘要
		/// </summary>
		/// <param name="original">数据源</param>
		/// <returns>摘要</returns>
		public static byte[] MakeMD5(byte[] original)
		{
			MD5CryptoServiceProvider hashmd5 = new MD5CryptoServiceProvider();   
			byte[] keyhash = hashmd5.ComputeHash(original);       
			hashmd5 = null;  
			return keyhash;
		}


		/// <summary>
		/// 使用给定密钥加密
		/// </summary>
		/// <param name="original">明文</param>
		/// <param name="key">密钥</param>
		/// <returns>密文</returns>
		public static byte[] Encrypt(byte[] original, byte[] key)  
		{  
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();       
			des.Key =  MakeMD5(key);
			des.Mode = CipherMode.ECB;  
     
			return des.CreateEncryptor().TransformFinalBlock(original, 0, original.Length);     
		}  

		/// <summary>
		/// 使用给定密钥解密数据
		/// </summary>
		/// <param name="encrypted">密文</param>
		/// <param name="key">密钥</param>
		/// <returns>明文</returns>
		public static byte[] Decrypt(byte[] encrypted, byte[] key)  
		{  
			TripleDESCryptoServiceProvider des = new TripleDESCryptoServiceProvider();  
			des.Key =  MakeMD5(key);    
			des.Mode = CipherMode.ECB;  

			return des.CreateDecryptor().TransformFinalBlock(encrypted, 0, encrypted.Length);
		}

		#endregion


		/// <summary>
		/// DES 加解密
		/// </summary>
		/// <param name="origal">加解密字符串</param>
		/// <param name="sign">标识加密或者解密位,默认加密</param>
		/// <param name="key">key</param>
		/// <param name="iv">iv</param>
		/// <returns>加解密后的结果</returns>
		public static string DES(string origal, int sign = 0, string key = "roim1QaZ", string iv = "roim1QaZ")
		{
			byte[] origalByteArray = sign == 0 ? Encoding.Default.GetBytes(origal) : Convert.FromBase64String(origal);
			DESCryptoServiceProvider des = new DESCryptoServiceProvider();
			des.Key = Encoding.ASCII.GetBytes(key);
			des.IV = Encoding.ASCII.GetBytes(iv);
			if (sign != 0)
			{
				des.Padding = PaddingMode.None;
			}
			string result = string.Empty;
			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, sign == 0 ? des.CreateEncryptor() : des.CreateDecryptor(), CryptoStreamMode.Write))
				{
					cs.Write(origalByteArray, 0, origalByteArray.Length);
					cs.FlushFinalBlock();
					result = sign == 0 ? Convert.ToBase64String(ms.ToArray()) : System.Text.Encoding.Default.GetString(ms.ToArray());

				}
			}
			if (result == string.Empty)
			{
				throw new ArgumentException();
			}
			return result;
		}

	}
}
