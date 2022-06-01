using System;
using System.Security.Cryptography; 
using System.Web.Security;
using System.IO;
using System.Text;

namespace MUI.Components.Encryption
{
	//�����㷨ö�٣��Լ����ڸ�д��
	public enum EncryptionAlgorithm {Des = 1, Rc2, Rijndael, TripleDes};

	#region ��EncryptTransformer
	/// <summary>
	/// EncryptTransformer ��ժҪ˵����
	/// </summary>
	internal class EncryptTransformer
	{
		private EncryptionAlgorithm algorithmID;
		private byte[] initVec;
		private byte[] encKey;

		internal EncryptTransformer(EncryptionAlgorithm algId)
		{
			//��������ʹ�õ��㷨��
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
			// ѡȡ�ṩ����
			switch (algorithmID)
			{
				case EncryptionAlgorithm.Des:
				{
					DES des = new DESCryptoServiceProvider();
					des.Mode = CipherMode.CBC;

					// �鿴�Ƿ��ṩ����Կ
					if (null == bytesKey)
					{
						encKey = des.Key;
					}
					else
					{
						des.Key = bytesKey;
						encKey = des.Key;
					}
					// �鿴�ͻ����Ƿ��ṩ�˳�ʼ������
					if (null == initVec)
					{ // ���㷨����һ��
						initVec = des.IV;
					}
					else
					{ //���������ṩ���㷨
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
					// �鿴�ͻ����Ƿ��ṩ�˳�ʼ������
					if (null == initVec)
					{ //�ǣ����㷨����һ��
						initVec = des3.IV;
					}
					else
					{ //���������ṩ���㷨��
						des3.IV = initVec;
					}
					return des3.CreateEncryptor();
				}
				case EncryptionAlgorithm.Rc2:
				{
					RC2 rc2 = new RC2CryptoServiceProvider();
					rc2.Mode = CipherMode.CBC;
					// �����Ƿ��ṩ����Կ
					if (null == bytesKey)
					{
						encKey = rc2.Key;
					}
					else
					{
						rc2.Key = bytesKey;
						encKey = rc2.Key;
					}
					// �鿴�ͻ����Ƿ��ṩ�˳�ʼ������
					if (null == initVec)
					{ //�ǣ����㷨����һ��
						initVec = rc2.IV;
					}
					else
					{ //���������ṩ���㷨��
						rc2.IV = initVec;
					}
					return rc2.CreateEncryptor();
				}
				case EncryptionAlgorithm.Rijndael:
				{
					Rijndael rijndael = new RijndaelManaged();
					rijndael.Mode = CipherMode.CBC;
					// �����Ƿ��ṩ����Կ
					if(null == bytesKey)
					{
						encKey = rijndael.Key;
					}
					else
					{
						rijndael.Key = bytesKey;
						encKey = rijndael.Key;
					}
					// �鿴�ͻ����Ƿ��ṩ�˳�ʼ������
					if(null == initVec)
					{ //�ǣ����㷨����һ��
						initVec = rijndael.IV;
					}
					else
					{ //���������ṩ���㷨��
						rijndael.IV = initVec;
					}
					return rijndael.CreateEncryptor();
				} 
				default:
				{
					throw new CryptographicException("�㷨 ID '" + algorithmID + 
						"' ����֧�֡�");
				}
			}
		}
	}
	#endregion

	#region ��DecryptTransformer
	/// <summary>
	/// DecryptTransformer ��ժҪ˵����
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
					throw new CryptographicException("�㷨 ID '" + algorithmID + 
						"' ��֧�֡�");
				}
			}
		} //end GetCryptoServiceProvider
	}
	#endregion

	#region ��Encryptor
	/// <summary>
	/// Encryptor ��ժҪ˵����
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
			//���ý�����������ݵ�����
			MemoryStream memStreamEncryptedData = new MemoryStream();

			transformer.IV = initVec;
			ICryptoTransform transform = transformer.GetCryptoServiceProvider(bytesKey);
			CryptoStream encStream = new CryptoStream(memStreamEncryptedData,transform,CryptoStreamMode.Write);
			try
			{
				//�������ݣ���������д���ڴ�����
				encStream.Write(bytesData, 0, bytesData.Length);
			}
			catch(Exception ex)
			{
				throw new Exception("����������д����ʱ���� \n"  + ex.Message);
			}
			//Ϊ�ͻ��˽��м������ó�ʼ����������Կ
			encKey = transformer.Key;
			initVec = transformer.IV;
			encStream.FlushFinalBlock();
			encStream.Close();

			//���ͻ����ݡ�
			return memStreamEncryptedData.ToArray();
		}//end Encrypt
	}
	#endregion

	#region ��Decryptor
	/// <summary>
	/// Decryptor ��ժҪ˵����
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
			//Ϊ�������������ڴ�����
			MemoryStream memStreamDecryptedData = new MemoryStream();

			//���ݳ�ʼ��������
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
				throw new Exception("����������д����ʱ���� \n"  + ex.Message);
			}
			decStream.FlushFinalBlock();
			decStream.Close();
			// ���ͻ����ݡ�
			return memStreamDecryptedData.ToArray();
		} //end Decrypt


	}
	#endregion
	
	#region ���ܺͽ����ࣨ���Ͽ�����
	public sealed class EncryptDecrypt
	{
		private EncryptDecrypt(){}
		//Ĭ����Կ����
		private static byte[] Keys = {0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF};
		private static string defaultEncryptKey="12345678";//��Կֻ����8λ
		#region ���û��������(������),���ܽ���
		/// <summary>
		/// �������
		/// </summary>
		/// <param name="PasswordString">Ҫ���ܵ�����</param>
		/// <param name="EncryptArithmetic">�����㷨("SHA1"��"MD5")</param>
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
		#region ����MD5���ַ������м���
		/// <summary>
		/// ����MD5���ַ������м���
		/// </summary>
		/// <param name="encryptString">�����ܵ��ַ���</param>
		/// <returns>���ؼ��ܺ���ַ���</returns>
		public static string EncryptMD5(string encryptString)
		{
			MD5CryptoServiceProvider md5Hasher = new MD5CryptoServiceProvider();
			UTF8Encoding Encode = new UTF8Encoding();
			byte[] HashedBytes = md5Hasher.ComputeHash(Encode.GetBytes(encryptString));
			return Encode.GetString(HashedBytes);
		}
		#endregion
		#region DES�����ַ���
		/// <summary>
		/// DES�����ַ���
		/// </summary>
		/// <param name="encryptString">�����ܵ��ַ���</param>
		/// <param name="encryptKey">������Կ,Ҫ��Ϊ8λ</param>
		/// <returns>���ܳɹ����ؼ��ܺ���ַ�����ʧ�ܷ���Դ��</returns>
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
		#region DES�����ַ���
		/// <summary>
		/// DES�����ַ���
		/// </summary>
		/// <param name="decryptString">�����ܵ��ַ���</param>
		/// <param name="decryptKey">������Կ,Ҫ��Ϊ8λ,�ͼ�����Կ��ͬ</param>
		/// <returns>���ܳɹ����ؽ��ܺ���ַ�����ʧ�ܷ�Դ��</returns>
		public static string DecryptDES(string decryptString)
		{
			return DecryptDES(decryptString,defaultEncryptKey);
		}
		/// <summary>
		/// DES�����ַ���
		/// </summary>
		/// <param name="decryptString"></param>
		/// <param name="decryptKey">��Կֻ����8λ</param>
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
		#region DES�����ļ�
		/// <summary>
		/// DES�����ļ�
		/// </summary>
		/// <param name="inFilePath">�������ļ�</param>
		/// <param name="outFilePath">���ܺ���ļ�</param>
		/// <param name="encryptKey">������Կ</param>
		/// <returns></returns>
		public static bool EncryptDES(string inFilePath,string outFilePath,string encryptKey)
		{  
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0,8));
				//�������
				FileStream inFs = new FileStream(inFilePath, FileMode.Open, FileAccess.Read);
				//��д����
				FileStream outFs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
				outFs.SetLength(0);
				//����һ��������������д
				byte[] byteIn  = new byte[100]; //��ʱ��Ŷ������
				long readLen  = 0;              //�������ĳ���
				long totalLen = inFs.Length;    //�ܹ��������ĳ���
				int  everyLen;                  //ÿ�ζ�����������
				//����InFs�����ܺ�д��OutFs
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
				return true;//���ܳɹ�
			}
			catch
			{
				return false;//����ʧ�� 
			}
		}
		#endregion
		#region DES�����ļ�
		/// <summary>
		/// DES�����ļ�
		/// </summary>
		/// <param name="inFilePath">�������ļ�</param>
		/// <param name="outFilePath">�������ļ�</param>
		/// <param name="decryptKey">������Կ</param>
		/// <returns></returns>
		public static bool DecryptDES(string inFilePath,string outFilePath,string decryptKey)
		{
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0,8));
				//�������
				FileStream inFs = new FileStream(inFilePath, FileMode.Open, FileAccess.Read);
				//��д����
				FileStream outFs = new FileStream(outFilePath, FileMode.OpenOrCreate, FileAccess.Write);
				outFs.SetLength(0);
				//����һ��������������д
				byte[] byteIn  = new byte[100]; //��ʱ��Ŷ������
				long readLen  = 0;              //�������ĳ���
				long totalLen = inFs.Length;    //�ܹ��������ĳ���
				int  everyLen;                  //ÿ�ζ�����������
				//����InFs�����ܺ�д��OutFs
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
				return true;//���ܳɹ�
			}
			catch
			{
				return false;//����ʧ�� 
			}
		}
		#endregion
		#region DES�����ļ����� �����ַ���
		/// <summary>
		/// DES�����ļ�
		/// </summary>
		/// <param name="inFilePath">�������ļ�</param>
		/// <param name="outFilePath">�������ļ�</param>
		/// <param name="decryptKey">������Կ</param>
		/// <returns></returns>
		public static string DecryptDES_Content(string fileContent,string decryptKey)
		{
			string ret="";
			byte[] rgbIV= Keys;
			try
			{
				byte[] rgbKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0,8));
				//�������
				System.IO.MemoryStream inFs = new MemoryStream(System.Text.Encoding.Default.GetBytes(fileContent));
				//��д����
				System.IO.MemoryStream outFs = new MemoryStream(System.Text.Encoding.Default.GetBytes(""),true);
				outFs.SetLength(0);
				//����һ��������������д
				byte[] byteIn  = new byte[100]; //��ʱ��Ŷ������
				long readLen  = 0;              //�������ĳ���
				long totalLen = inFs.Length;    //�ܹ��������ĳ���
				int  everyLen;                  //ÿ�ζ�����������
				//����InFs�����ܺ�д��OutFs
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
				return ret;//���ܳɹ�
			}
			catch
			{
				return null;//����ʧ�� 
			}
		}
		#endregion
	}
	#endregion
	
	#region Base64Encoder������
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

#region Base64Decoder������
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


