using System;
using System.Security.Cryptography; 
using System.Web.Security;
using System.IO;
using System.Text;

namespace MUI.Components.Encryption
{
	//加密算法枚举（自己日期改写）
	public enum EncryptionAlgorithm {Des = 1, Rc2, Rijndael, TripleDes};

	#region 类EncryptTransformer
	/// <summary>
	/// EncryptTransformer 的摘要说明。
	/// </summary>
	internal class EncryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;
		private byte[] encKey;

		internal EncryptTransformer(EncryptionAlgorithm algId)
		{
			//保存正在使用的算法。
			algorithmID = algId;
		}
		internal byte[] IV
		{
			get{return initVec;}
			set{initVec = value;}
		}
		internal byte[] Key
		{
			get{return encKey;}
		}
		internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
		{
			// 选取提供程序。
			switch (algorithmID)
			{
				case EncryptionAlgorithm.Des:
				{
					DES des = new DESCryptoServiceProvider();
					des.Mode = CipherMode.CBC;

					// 查看是否提供了密钥
					if (null == bytesKey)
					{
						encKey = des.Key;
					}
					else
					{
						des.Key = bytesKey;
						encKey = des.Key;
					}
					// 查看客户端是否提供了初始化向量
					if (null == initVec)
					{ // 让算法创建一个
						initVec = des.IV;
					}
					else
					{ //不，将它提供给算法
						des.IV = initVec;
					}
					return des.CreateEncryptor();
				}
				case EncryptionAlgorithm.TripleDes:
				{
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;
					// See if a key was provided
					if (null == bytesKey)
					{
						encKey = des3.Key;
					}
					else
					{
						des3.Key = bytesKey;
						encKey = des3.Key;
					}
					// 查看客户端是否提供了初始化向量
					if (null == initVec)
					{ //是，让算法创建一个
						initVec = des3.IV;
					}
					else
					{ //不，将它提供给算法。
						des3.IV = initVec;
					}
					return des3.CreateEncryptor();
				}
				case EncryptionAlgorithm.Rc2:
				{
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					// 测试是否提供了密钥
					if (null == bytesKey)
					{
						encKey = rc2.Key;
					}
					else
					{
						rc2.Key = bytesKey;
						encKey = rc2.Key;
					}
					// 查看客户端是否提供了初始化向量
					if (null == initVec)
					{ //是，让算法创建一个
						initVec = rc2.IV;
					}
					else
					{ //不，将它提供给算法。
						rc2.IV = initVec;
					}
					return rc2.CreateEncryptor();
				}
				case EncryptionAlgorithm.Rijndael:
				{
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;
					// 测试是否提供了密钥
					if(null == bytesKey)
					{
						encKey = rijndael.Key;
					}
					else
					{
						rijndael.Key = bytesKey;
						encKey = rijndael.Key;
					}
					// 查看客户端是否提供了初始化向量
					if(null == initVec)
					{ //是，让算法创建一个
						initVec = rijndael.IV;
					}
					else
					{ //不，将它提供给算法。
						rijndael.IV = initVec;
					}
					return rijndael.CreateEncryptor();
				} 
				default:
				{
					throw new CryptographicException("算法 ID '" + algorithmID + 
						"' 不受支持。");
				}
			}
		}
	}
	#endregion

	#region 类DecryptTransformer
	/// <summary>
	/// DecryptTransformer 的摘要说明。
	/// </summary>
	internal class DecryptTransformer 
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;

		internal DecryptTransformer(EncryptionAlgorithm deCryptId)
		{
			algorithmID = deCryptId;
		}

		internal byte[] IV
		{
			set{initVec = value;}
		}
		internal ICryptoTransform GetCryptoServiceProvider(byte[] bytesKey)
		{
			// Pick the provider.
			switch (algorithmID)
			{
				case EncryptionAlgorithm.Des:
				{
					DES des = new DESCryptoServiceProvider();
					des.Mode = CipherMode.CBC;
					des.Key = bytesKey;
					des.IV = initVec;
					return des.CreateDecryptor();
				}
				case EncryptionAlgorithm.TripleDes:
				{
					TripleDES des3 = new TripleDESCryptoServiceProvider();
					des3.Mode = CipherMode.CBC;
					return des3.CreateDecryptor(bytesKey, initVec);
				}
				case EncryptionAlgorithm.Rc2:
				{
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					return rc2.CreateDecryptor(bytesKey, initVec);
				}
				case EncryptionAlgorithm.Rijndael:
				{
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;
					return rijndael.CreateDecryptor(bytesKey, initVec);
				} 
				default:
				{
					throw new CryptographicException("算法 ID '" + algorithmID + 
						"' 不支持。");
				}
			}
		} //end GetCryptoServiceProvider
	}
	#endregion

	#region 类Encryptor
	/// <summary>
	/// Encryptor 的摘要说明。
	/// </summary>
	public class Encryptor
	{
		private EncryptTransformer transformer;
		private byte[] initVec;
		private byte[] encKey;

		public Encryptor(EncryptionAlgorithm algId)
		{
			transformer = new EncryptTransformer(algId);
		}
		public byte[] IV
		{
			get{return initVec;}
			set{initVec = value;}
		}

		public byte[] Key
		{
			get{return encKey;}
		}

		public byte[] Encrypt(byte[] bytesData, byte[] bytesKey)
		{
			//设置将保存加密数据的流。
			MemoryStream memStreamEncryptedData = new MemoryStream();

			transformer.IV = initVec;
			ICryptoTransform transform = transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream encStream = new CryptoStream(memStreamEncryptedData,transform,CryptoStreamMode.Write);
			try
			{
				//加密数据，并将它们写入内存流。
				encStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("将加密数据写入流时出错： \n"  + ex.Message);
			}
			//为客户端进行检索设置初始化向量和密钥
			encKey = transformer.Key;
			initVec = transformer.IV;
			encStream.FlushFinalBlock();
			encStream.Close();

			//发送回数据。
			return memStreamEncryptedData.ToArray();
		}//end Encrypt
	}
	#endregion

	#region 类Decryptor
	/// <summary>
	/// Decryptor 的摘要说明。
	/// </summary>
	public class Decryptor
	{
		private DecryptTransformer transformer;
		private byte[] initVec;

		public byte[] IV
		{
			set{initVec = value;}
		}

		public Decryptor(EncryptionAlgorithm algId)
		{
			transformer = new DecryptTransformer(algId);
		}

		public byte[] Decrypt(byte[] bytesData, byte[] bytesKey)
		{
			//为解密数据设置内存流。
			MemoryStream memStreamDecryptedData = new MemoryStream();

			//传递初始化向量。
			transformer.IV = initVec;
			ICryptoTransform transform = transformer.GetCryptoServiceProvider
				(bytesKey);
			CryptoStream decStream = new CryptoStream(memStreamDecryptedData, 
				transform, 
				CryptoStreamMode.Write);
			try
			{
				decStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("将加密数据写入流时出错： \n"  + ex.Message);
			}
			decStream.FlushFinalBlock();
			decStream.Close();
			// 发送回数据。
			return memStreamDecryptedData.ToArray();
		} //end Decrypt


	}
	#endregion
	
	#region 加密和解密类（网上拷贝）
	public sealed class EncryptDecrypt
	{
		private EncryptDecrypt(){}
		//默认密钥向量
		private static byte[] Keys = {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF};
		private static string defaultEncryptKey="12345678";//密钥只能是8位
		#region 对用户密码加密(不可逆),不能解密
		/// <summary>
		/// 密码加密
		/// </summary>
		/// <param name="PasswordString">要加密的密码</param>
		/// <param name="EncryptArithmetic">加密算法("SHA1"或"MD5")</param>
		/// <returns></returns>
		public static string EncryptPassword(string PasswordString) 
		{
			return EncryptPassword(PasswordString,"MD5");
		}
		public static string EncryptPassword(string PasswordString,string EncryptAlgorithm) 
		{ 
			if (EncryptAlgorithm=="SHA1")
			{ 
				return FormsAuthentication.HashPasswordForStoringInConfigFile(PasswordString ,"SHA1"); 
			} 
			else if (EncryptAlgorithm=="MD5") 
			{
				return FormsAuthentication.HashPasswordForStoringInConfigFile(PasswordString ,"MD5"); 
			} 
			else 
			{ 
				return PasswordString; 
			} 
		}
		#endregion
		#region 利用MD5对字符串进行加密
		/// <summary>
		/// 利用MD5对字符串进行加密
		/// </summary>
		/// <param name="encryptString">待加密的字符串</param>
		/// <returns>返回加密后的字符串</returns>
		public static string EncryptMD5(string encryptString)
		{
			MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
			UTF8Encoding Encode = new UTF8Encoding();
			byte[] HashedBytes = md5Hasher.ComputeHash(Encode.GetBytes(encryptString));
			return Encode.GetString(HashedBytes);
		}
		#endregion
		#region DES加密字符串
		/// <summary>
		/// DES加密字符串
		/// </summary>
		/// <param name="encryptString">待加密的字符串</param>
		/// <param name="encryptKey">加密密钥,要求为8位</param>
		/// <returns>加密成功返回加密后的字符串，失败返回源串</returns>
		public static string EncryptDES(string encryptString)
		{
			return EncryptDES(encryptString,defaultEncryptKey);
		}
		public static string EncryptDES(string encryptString,string encryptKey)
		{
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0,8));
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Encoding.UTF8.GetBytes(encryptString);
				DESCryptoServiceProvider dCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream,dCSP.CreateEncryptor(rgbKey,rgbIV),CryptoStreamMode.Write);
				cStream.Write(inputByteArray,0,inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Convert.ToBase64String(mStream.ToArray());
			}
			catch
			{
				return encryptString;
			}
		}
		#endregion
		#region DES解密字符串
		/// <summary>
		/// DES解密字符串
		/// </summary>
		/// <param name="decryptString">待解密的字符串</param>
		/// <param name="decryptKey">解密密钥,要求为8位,和加密密钥相同</param>
		/// <returns>解密成功返回解密后的字符串，失败返源串</returns>
		public static string DecryptDES(string decryptString)
		{
			return DecryptDES(decryptString,defaultEncryptKey);
		}
		/// <summary>
		/// DES解密字符串
		/// </summary>
		/// <param name="decryptString"></param>
		/// <param name="decryptKey">密钥只能是8位</param>
		/// <returns></returns>
		public static string DecryptDES(string decryptString,string decryptKey)
		{
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey);
				byte[] rgbIV = Keys;
				byte[] inputByteArray = Convert.FromBase64String(decryptString);
				DESCryptoServiceProvider DCSP = new DESCryptoServiceProvider();
				MemoryStream mStream = new MemoryStream();
				CryptoStream cStream = new CryptoStream(mStream,DCSP.CreateDecryptor(rgbKey,rgbIV),CryptoStreamMode.Write);
				cStream.Write(inputByteArray,0,inputByteArray.Length);
				cStream.FlushFinalBlock();
				return Encoding.UTF8.GetString(mStream.ToArray());
			}
			catch
			{
                //return decryptString;
                return "Invalid";
			}
		}
		#endregion
		#region DES加密文件
		/// <summary>
		/// DES加密文件
		/// </summary>
		/// <param name="inFilePath">待加密文件</param>
		/// <param name="outFilePath">加密后的文件</param>
		/// <param name="encryptKey">加密密钥</param>
		/// <returns></returns>
		public static bool EncryptDES(string inFilePath,string outFilePath,string encryptKey)
		{  
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0,8));
				//读入的流
				FileStream inFs = new FileStream(inFilePath, FileMode.Open, FileAccess.Read);
				//待写的流
				FileStream outFs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
				outFs.SetLength(0);
				//创建一个变量来帮助读写
				byte[] byteIn  = new byte[100]; //临时存放读入的流
				long readLen  = 0;              //读入流的长度
				long totalLen = inFs.Length;    //总共读入流的长度
				int  everyLen;                  //每次读入流动长度
				//读入InFs，加密后写入OutFs
				DES des = new DESCryptoServiceProvider();          
				CryptoStream encStream = new CryptoStream(outFs, des.CreateEncryptor(rgbKey, rgbIV), CryptoStreamMode.Write); 
				while(readLen < totalLen)
				{
					everyLen = inFs.Read(byteIn, 0, 100);
					encStream.Write(byteIn, 0, everyLen);
					readLen = readLen + everyLen; 
				}
				encStream.Close();  
				outFs.Close();
				inFs.Close();
				return true;//加密成功
			}
			catch
			{
				return false;//加密失败 
			}
		}
		#endregion
		#region DES解密文件
		/// <summary>
		/// DES解密文件
		/// </summary>
		/// <param name="inFilePath">待解密文件</param>
		/// <param name="outFilePath">待加密文件</param>
		/// <param name="decryptKey">解密密钥</param>
		/// <returns></returns>
		public static bool DecryptDES(string inFilePath,string outFilePath,string decryptKey)
		{
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0,8));
				//读入的流
				FileStream inFs = new FileStream(inFilePath, FileMode.Open, FileAccess.Read);
				//待写的流
				FileStream outFs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
				outFs.SetLength(0);
				//创建一个变量来帮助读写
				byte[] byteIn  = new byte[100]; //临时存放读入的流
				long readLen  = 0;              //读入流的长度
				long totalLen = inFs.Length;    //总共读入流的长度
				int  everyLen;                  //每次读入流动长度
				//读入InFs，解密后写入OutFs
				DES des = new DESCryptoServiceProvider();          
				CryptoStream encStream = new CryptoStream(outFs, des.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write); 
				while(readLen < totalLen)
				{
					everyLen = inFs.Read(byteIn, 0, 100);
					encStream.Write(byteIn, 0, everyLen);
					readLen = readLen + everyLen; 
				}
				encStream.Close();  
				outFs.Close();
				inFs.Close();
				return true;//解密成功
			}
			catch
			{
				return false;//解密失败 
			}
		}
		#endregion
		#region DES解密文件内容 返回字符串
		/// <summary>
		/// DES解密文件
		/// </summary>
		/// <param name="inFilePath">待解密文件</param>
		/// <param name="outFilePath">待加密文件</param>
		/// <param name="decryptKey">解密密钥</param>
		/// <returns></returns>
		public static string DecryptDES_Content(string fileContent,string decryptKey)
		{
			string ret="";
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0,8));
				//读入的流
				System.IO.MemoryStream inFs = new MemoryStream(System.Text.Encoding.Default.GetBytes(fileContent));
				//待写的流
				System.IO.MemoryStream outFs = new MemoryStream(System.Text.Encoding.Default.GetBytes(""),true);
				outFs.SetLength(0);
				//创建一个变量来帮助读写
				byte[] byteIn  = new byte[100]; //临时存放读入的流
				long readLen  = 0;              //读入流的长度
				long totalLen = inFs.Length;    //总共读入流的长度
				int  everyLen;                  //每次读入流动长度
				//读入InFs，解密后写入OutFs
				DES des = new DESCryptoServiceProvider();          
				CryptoStream encStream = new CryptoStream(outFs, des.CreateDecryptor(rgbKey, rgbIV), CryptoStreamMode.Write); 
				while(readLen < totalLen)
				{
					everyLen = inFs.Read(byteIn, 0, 100);
					encStream.Write(byteIn, 0, everyLen);
					readLen = readLen + everyLen; 
				}
				encStream.Close();  
				ret=System.Text.Encoding.Default.GetString(outFs.GetBuffer());
				outFs.Close();
				inFs.Close();
				return ret;//解密成功
			}
			catch
			{
				return null;//解密失败 
			}
		}
		#endregion
	}
	#endregion
	
	#region Base64Encoder编码器
	public class Base64Encoder
	{
		byte[] source;
		int length,length2;
		int blockCount;
		int paddingCount;
		public Base64Encoder(byte[] input)
		{
			source=input;
			length=input.Length;
			if((length % 3)==0)
			{
				paddingCount=0;
				blockCount=length/3;
			}
			else
			{
				paddingCount=3-(length % 3);//need to add padding
				blockCount=(length+paddingCount) / 3;
			}
			length2=length+paddingCount;//or blockCount *3
		}

		public char[] GetEncoded()
		{
			byte[] source2;
			source2=new byte[length2];
			//copy data over insert padding
			for (int x=0; x<length2;x++)
			{
				if (x<length)
				{
					source2[x]=source[x];
				}
				else
				{
					source2[x]=0;
				}
			}
      
			byte b1, b2, b3;
			byte temp, temp1, temp2, temp3, temp4;
			byte[] buffer=new byte[blockCount*4];
			char[] result=new char[blockCount*4];
			for (int x=0;x<blockCount;x++)
			{
				b1=source2[x*3];
				b2=source2[x*3+1];
				b3=source2[x*3+2];

				temp1=(byte)((b1 & 252)>>2);//first

				temp=(byte)((b1 & 3)<<4);
				temp2=(byte)((b2 & 240)>>4);
				temp2+=temp; //second

				temp=(byte)((b2 & 15)<<2);
				temp3=(byte)((b3 & 192)>>6);
				temp3+=temp; //third

				temp4=(byte)(b3 & 63); //fourth

				buffer[x*4]=temp1;
				buffer[x*4+1]=temp2;
				buffer[x*4+2]=temp3;
				buffer[x*4+3]=temp4;

			}

			for (int x=0; x<blockCount*4;x++)
			{
				result[x]=sixbit2char(buffer[x]);
			}

			//covert last "A"s to "=", based on paddingCount
			switch (paddingCount)
			{
				case 0:break;
				case 1:result[blockCount*4-1]='=';break;
				case 2:result[blockCount*4-1]='=';
					result[blockCount*4-2]='=';
					break;
				default:break;
			}
			return result;
		}

		private char sixbit2char(byte b)
		{
			char[] lookupTable=new char[64]
		  {
			  'A','B','C','D','E','F','G','H','I','J','K','L','M',
			  'N','O','P','Q','R','S','T','U','V','W','X','Y','Z',
			  'a','b','c','d','e','f','g','h','i','j','k','l','m',
			  'n','o','p','q','r','s','t','u','v','w','x','y','z',
			  '0','1','2','3','4','5','6','7','8','9','+','/'};

			if((b>=0) &&(b<=63))
			{
				return lookupTable[(int)b];
			}
			else
			{
				//should not happen;
				return ' ';
			}
		}
	}
}
#endregion

#region Base64Decoder解码器
public class Base64Decoder
{
	char[] source;
	int length, length2, length3;
	int blockCount;
	int paddingCount;
	public Base64Decoder(char[] input)
	{
		int temp=0;
		source=input;
		length=input.Length;

		//find how many padding are there
		for (int x=0;x<2;x++)
		{
			if(input[length-x-1]=='=')
				temp++;
		}
		paddingCount=temp;
		//calculate the blockCount;
		//assuming all whitespace and carriage returns/newline were removed.
		blockCount=length/4;
		length2=blockCount*3;
	}

	public byte[] GetDecoded()
	{
		byte[] buffer=new byte[length];//first conversion result
		byte[] buffer2=new byte[length2];//decoded array with padding

		for(int x=0;x<length;x++)
		{
			buffer[x]=char2sixbit(source[x]);
		}

		byte b, b1,b2,b3;
		byte temp1, temp2, temp3, temp4;

		for(int x=0;x<blockCount;x++)
		{
			temp1=buffer[x*4];
			temp2=buffer[x*4+1];
			temp3=buffer[x*4+2];
			temp4=buffer[x*4+3];        

			b=(byte)(temp1<<2);
			b1=(byte)((temp2 & 48)>>4);
			b1+=b;

			b=(byte)((temp2 & 15)<<4);
			b2=(byte)((temp3 & 60)>>2);
			b2+=b;

			b=(byte)((temp3 & 3)<<6);
			b3=temp4;
			b3+=b;

			buffer2[x*3]=b1;
			buffer2[x*3+1]=b2;
			buffer2[x*3+2]=b3;
		}
		//remove paddings
		length3=length2-paddingCount;
		byte[] result=new byte[length3];

		for(int x=0;x<length3;x++)
		{
			result[x]=buffer2[x];
		}

		return result;
	}

	private byte char2sixbit(char c)
	{
		char[] lookupTable=new char[64]
		  {  

			  'A','B','C','D','E','F','G','H','I','J','K','L','M','N',
			  'O','P','Q','R','S','T','U','V','W','X','Y', 'Z',
			  'a','b','c','d','e','f','g','h','i','j','k','l','m','n',
			  'o','p','q','r','s','t','u','v','w','x','y','z',
			  '0','1','2','3','4','5','6','7','8','9','+','/'};
		if(c=='=')
			return 0;
		else
		{
			for (int x=0;x<64;x++)
			{
				if (lookupTable[x]==c)
					return (byte)x;
			}
			//should not reach here
			return 0;
		}

	}
	#endregion
}


