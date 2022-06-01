using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;
using System.Security.Cryptography;
using System.Management;

namespace MUI.Components.ECMAScriptDealer
{
    public class Helper
    {
        public static bool IsDigit(object obj)
        {
            //为指定的正则表达式初始化并编译 Regex 类的实例
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^(\d*)$");
            //在指定的输入字符串中搜索 Regex 构造函数中指定的正则表达式匹配项
            System.Text.RegularExpressions.Match mc = rg.Match(obj.ToString());
            //指示匹配是否成功
            return (mc.Success);
        }
        public static bool IsNumber(object obj)
        {
            //为指定的正则表达式初始化并编译 Regex 类的实例
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex(@"^-?(\d*)$");
            //在指定的输入字符串中搜索 Regex 构造函数中指定的正则表达式匹配项
            System.Text.RegularExpressions.Match mc = rg.Match(obj.ToString());
            //指示匹配是否成功
            return (mc.Success);
        }
        #region 校验数据是否能转化为指定类型
        /// <summary>
        /// 数据校验
        /// </summary>
        /// <param name="obj">所要校验的数据</param>
        /// <param name="Type">数据类型</param>
        /// <returns></returns>
        public static bool CheckValue(object obj, char Type)
        {
            try
            {
                switch (Type)
                {
                    case 'N':
                        System.Convert.ToInt64(obj);
                        break;
                    case 'F':
                        System.Convert.ToDouble(obj);
                        break;
                    case 'D':
                        System.Convert.ToDecimal(obj);
                        break;
                    case 'T':
                        System.Convert.ToDateTime(obj);
                        break;
                }
                return true;
            }
            catch//(Exception e)
            {
                return false;
            }
        }
        #endregion
        //字符串转换为16进制形式
        public static string StringToHex(string str)
        {
            if (str == null || str == string.Empty)
            {
                return string.Empty;
            }
            StringBuilder sb = new StringBuilder();
            for (int k = 0; k < str.Length; k++)
            {
                string tmp = Convert.ToString(str[k], 16);
                if (Convert.ToInt32(tmp, 16) < 256)
                {
                    sb.Append(string.Concat("\\x", tmp));
                }
                else
                {
                    sb.Append(string.Concat("\\u", tmp));
                }
            }
            return sb.ToString();
        }
        public static string StringToHex2(string str)
        {
            return StringToHex2(str, false);
        }
        public static string StringToHex2(string str,bool onlyChinese)
        {
            StringBuilder sb = new StringBuilder();
            for (int k = 0, l = str.Length; k < l; k++)
            {
                if (str[k].ToString() == "\\")
                {
                    if (str[k + 1].ToString() == "x" && k < l - 3)
                    {
                        sb.Append(str[k].ToString());
                        sb.Append(str[k + 1].ToString());
                        sb.Append(str[k + 2].ToString());
                        sb.Append(str[k + 3].ToString());
                        k++; k++; k++;
                        continue;
                    }
                    else if (str[k + 1].ToString() == "u" && k < l - 5)
                    {
                        sb.Append(str[k].ToString());
                        sb.Append(str[k + 1].ToString());
                        sb.Append(str[k + 2].ToString());
                        sb.Append(str[k + 3].ToString());
                        sb.Append(str[k + 4].ToString());
                        sb.Append(str[k + 5].ToString());
                        k++; k++; k++; k++; k++;
                        continue;
                    }
                    else
                    {
                        sb.Append(str[k].ToString());
                        sb.Append(str[k + 1].ToString());
                        k++;
                        continue;
                    }
                }
                string tmp = Convert.ToString(str[k], 16);
                if (Convert.ToInt32(tmp, 16) < 256)
                {
                    if (onlyChinese)
                        sb.Append(str[k]);
                    else
                        sb.Append(string.Concat("\\x", tmp));
                }
                else
                {
                    sb.Append(string.Concat("\\u", tmp));
                }
            }
            return sb.ToString();
        }
        public static string RestoreEncodedStr(string str)
        {
            return HexToString(UnicodeToString(str));
        }
        public static string HexToString(string str)
        {
            return new Regex("\\\\x([\\w]{2})").Replace(str, new MatchEvaluator(me_replaceHex));
        }
        public static string UnicodeToString(string str)
        {
            return new Regex("\\\\u([\\w]{4})").Replace(str, new MatchEvaluator(me_replaceUnicode));
        }
        private static string me_replaceHex(Match match)
        {
            string word = match.Groups[1].Value;
            byte[] codes = new byte[1];
            int code = System.Convert.ToInt32(word.Substring(0, 2), 16);
            codes[0] = (byte)code;
            return Encoding.ASCII.GetString(codes);
        }
        private static string me_replaceUnicode(Match match)
        {
            string word = match.Groups[1].Value;
            byte[] codes = new byte[2];
            int code = System.Convert.ToInt32(word.Substring(0, 2), 16);
            int code2 = System.Convert.ToInt32(word.Substring(2), 16);
            codes[0] = (byte)code2;
            codes[1] = (byte)code;
            return Encoding.Unicode.GetString(codes);
        }
        //获得转义字符整数值
        public static int GetCharInteger(char ch)
        {
            string tmp = Convert.ToString(ch, 16);
            return Convert.ToInt32(tmp, 16);
        }
        //整数转化为十六进制形式
        public static string IntegerToHex(object num)
        {
            return "0x" + Convert.ToString(Convert.ToInt64(num), 16);
        }
        //
        public static bool Contains(string src, string substr)
        {
            return new Regex("(^|,)" + substr + "($|,)").IsMatch(src);
        }
        public static bool Contains2(string str1, string str2)
        {
            string[] arr = str1.Split(new char[] { ',' });
            for (int k = 0; k < arr.Length; k++)
            {
                if (arr[k] == str2) return true;
            }
            return false;
        }
        #region 获取机器第一个网卡 MAC 地址
        public static string GetFirstOfMACAddress()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            ArrayList strArr = new ArrayList();
            string sReturnValue = null;

            //网卡数 
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    sReturnValue = mo["MacAddress"].ToString().Replace(":", "");
                    break;
                }
            }
            moc.Dispose();
            return sReturnValue;
        }
        #endregion

        #region 获取机器所有网卡 MAC 地址
        /// <summary>得到网卡 MAC 地址</summary> 
        /// <returns>返回地址数组，字符类型</returns> 
        /// 例： 
        /// ArrayList myAddr = ArLi.CommonPrj.GetOfMACAddr.GetIt(); 
        /// for (int i=0; i<myAddr.Count; i++) MessageBox.Show(myAddr[i].ToString()); 
        /// 
        public static ArrayList GetAllMACAddresses()
        {
            ManagementClass mc = new ManagementClass("Win32_NetworkAdapterConfiguration");
            ManagementObjectCollection moc = mc.GetInstances();
            ArrayList strArr = new ArrayList();

            //网卡数 
            foreach (ManagementObject mo in moc)
            {
                if ((bool)mo["IPEnabled"])
                {
                    strArr.Add(mo["MacAddress"].ToString().Replace(":", ""));
                }
                mo.Dispose();
            }

            return strArr;
        }
        #endregion
        #region 获得本机第一个IP地址
        public static string GetLocalIP()
        {
            string hostName = System.Net.Dns.GetHostName();
            string ip = System.Net.Dns.GetHostByName(hostName).AddressList[0].ToString();
            return ip;
        }
        #endregion
        public static string GetHash(string data)
        {
            byte[] b = System.Text.Encoding.Default.GetBytes(data);

            return GetHash(b);
        }

        public static string GetHash(byte[] data)
        {
            string sMD5HashHexa = "";

            string[] tabStringHexa = new string[16];

            // This is one implementation of the abstract class MD5.
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] result = md5.ComputeHash(data);

            for (int i = 0; i < result.Length; i++)
            {
                tabStringHexa[i] = (result[i]).ToString("x");
                sMD5HashHexa += tabStringHexa[i];
            }

            return sMD5HashHexa;
        }

        public static string GetHash2(string data)
        {
            byte[] b = System.Text.Encoding.Default.GetBytes(data);

            return GetHash2(b);
        }

        public static string GetHash2(byte[] data)
        {
            string sMD5HashHexa = "";

            string[] tabStringHexa = new string[16];

            // This is one implementation of the abstract class MD5.
            MD5 md5 = new MD5CryptoServiceProvider();

            byte[] result = md5.ComputeHash(data);

            for (int i = 0; i < result.Length; i++)
            {
                tabStringHexa[i] = (result[i]).ToString("x");
                sMD5HashHexa += tabStringHexa[i];
            }

            return ("3at94" + sMD5HashHexa.Insert(3, "are").Insert(9, "23d").Insert(15, "ak9").Insert(21, "5hj").Insert(27, "l0p").Substring(0, 24) + "6k2f8j3s8sfsfs").Substring(0, 32);
        }
        // 是否符合变量命名字首规则
        public static bool IsVar(string val)
        {
            //if (val == "_" || val == "$") return false;
            return new Regex("^[a-zA-Z_\\$].*$").IsMatch(val);
        }
        public static string GetParam(string str, string key)
        {
            System.Text.RegularExpressions.Regex rg = new System.Text.RegularExpressions.Regex("(^|&|/)" + key + "=([^&]*)(&|$)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            System.Text.RegularExpressions.Match mc = rg.Match(str);
            if (!mc.Success) return null;
            return mc.Groups[2].Value;
        }
    }
}
