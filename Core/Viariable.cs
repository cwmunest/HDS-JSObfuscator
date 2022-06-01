using System;
using System.Collections.Generic;
using System.Text;

namespace MUI.Components.ECMAScriptDealer
{
    /// <summary>
    /// 变量类
    /// </summary>
    public abstract class Variable
    {
        public Variable(string name)
        {
            this._name = name;
        }
        protected string _name;
        protected string _randomName;
        protected DataType _dataType;
        public string Name { get { return this._name; } set { this._name = value; } }
        public string RandomName { get; set; }
        public DataType DataType { get { return this._dataType; } set { this._dataType = value; } }
        public Brace VBrace { get; set; }//变量所拥有的大括号单元，如类变量和类对象变量
        public string ClassName { get; set; }

        #region 释放资源
        #region IDisposable 成员
        public void Dispose()
        {
            // TODO:  添加 Class.Dispose 实现
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
        /// <summary>
        /// 释放资源
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
        }
        #endregion
    }
    /// <summary>
    /// 变量类别枚举
    /// </summary>
    public enum VariableType
    {
        common = 1,//普通变量名
        function = 2,//函数名或类名
        object_class = 3,//类对象名
        pro_class,//类成员属性名
        mothod_class//类成员方法名
    }

    public enum DataType
    {
        number = 1,
        strings = 2,
        boolean = 3,
        array=4,
        objects=5,
        function = 6,
        undefined = 7
    }

    public class Variable_Common : Variable
    {
        public Variable_Common(string name)
            : base(name)
        {
        }
    }
    public class Variable_Function : Variable
    {
        public Variable_Function(string name):base(name)
        {
        }
    }
    public class Variable_ObjectClass : Variable
    {
        public Variable_ObjectClass(string name)
            : base(name)
        {
        }
        //public string ClassName { get; set; }
    }
    public class Variable_ClassPro : Variable
    {
        public Variable_ClassPro(string name)
            : base(name)
        {
        }
        //public string ClassName { get; set; }
    }
    public class Variable_ClassMethod : Variable
    {
        public Variable_ClassMethod(string name)
            : base(name)
        {
        }
    }

    /// <summary>
    /// 混淆名产生器
    /// </summary>
    public class RandomNameBox
    {
        //static RandomNameBox()
        //{
        //    CreateAllRandom();
        //}
        static string LLetter26 = "abcdefghijklmnopqrstuvwxyz";
        static string ULetter26 = LLetter26.ToUpper();
        static string Arab10 = "0123456789";
        static string LLetter36 = LLetter26 + Arab10;
        static string ULetter36 = ULetter26 + Arab10;
        static string Letter62 = LLetter26+ULetter26 + Arab10;
        static string[] arrayAllRandom = null;

        public static bool BDigit = false;
        public static int VarLength = 0;
        public static string Seed = string.Empty;
        public static void CreateAllRandom()
        {
            List<string> list = new List<string>();
            if (VarLength < 2)
            {
                if (BDigit)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        list.Add(Arab10[i].ToString());
                    }
                }
                for (int i = 0; i < 26; i++)
                {
                    list.Add(LLetter26[i].ToString());
                }
                for (int i = 0; i < 26; i++)
                {
                    list.Add(ULetter26[i].ToString());
                }
            }
            if (BDigit)
            {
                for (int i = 0, m = Arab10.Length; i < m; i++)
                {
                    for (int j = 0, n = Letter62.Length; j < n; j++)
                    {
                        list.Add(Arab10[i].ToString() + Letter62[j].ToString());
                    }
                }
            }
            for (int i = 0, m = LLetter26.Length; i < m; i++)
            {
                for (int j = 0, n = Arab10.Length; j < n; j++)
                {
                    list.Add(LLetter26[i].ToString() + Arab10[j].ToString());
                }
            }
            for (int i = 0, m = LLetter26.Length; i < m; i++)
            {
                for (int j = 0, n = LLetter26.Length; j < n; j++)
                {
                    list.Add(LLetter26[i].ToString() + LLetter26[j].ToString());
                }
            }
            for (int i = 0, m = LLetter26.Length; i < m; i++)
            {
                for (int j = 0, n = ULetter26.Length; j < n; j++)
                {
                    list.Add(LLetter26[i].ToString() + ULetter26[j].ToString());
                }
            }
            for (int i = 0, m = ULetter26.Length; i < m; i++)
            {
                for (int j = 0, n = Arab10.Length; j < n; j++)
                {
                    list.Add(ULetter26[i].ToString() + Arab10[j].ToString());
                }
            }
            for (int i = 0, m = ULetter26.Length; i < m; i++)
            {
                for (int j = 0, n = LLetter26.Length; j < n; j++)
                {
                    list.Add(ULetter26[i].ToString() + LLetter26[j].ToString());
                }
            }
            for (int i = 0, m = ULetter26.Length; i < m; i++)
            {
                for (int j = 0, n = ULetter26.Length; j < n; j++)
                {
                    list.Add(ULetter26[i].ToString() + ULetter26[j].ToString());
                }
            }
            arrayAllRandom = list.ToArray();//total=3276
        }
        int _count = -1;
        public string Get()
        {
            this._count++;
            if (VarLength < 3)
            {
                if (this._count < arrayAllRandom.Length)
                {
                    return arrayAllRandom[this._count];
                }
                else
                {
                    string ret = Helper.GetHash(Seed + this._count).Substring(0, 3);
                    if (!BDigit && Helper.IsDigit(ret.Substring(0, 1))) ret = "O" + ret.Substring(0, ret.Length - 1);
                    return ret;
                }
            }
            else
            {
                string ret = Helper.GetHash(Seed + this._count).Substring(0, VarLength);
                if (!BDigit && Helper.IsDigit(ret.Substring(0, 1))) ret = "O" + ret.Substring(0, ret.Length - 1);
                return ret;
            }
        }
        public string Get(string key)
        {
            this._count++;
            if (VarLength < 3)
            {
                if (this._count < arrayAllRandom.Length)
                {
                    return arrayAllRandom[this._count];
                }
                else
                {
                    string ret = Helper.GetHash(Seed + key).Substring(0, 3);
                    if (!BDigit && Helper.IsDigit(ret.Substring(0, 1))) ret = "O" + ret.Substring(0, ret.Length - 1);
                    return ret;
                }
            }
            else
            {
                string ret = Helper.GetHash(Seed + key).Substring(0, VarLength);
                if (!BDigit && Helper.IsDigit(ret.Substring(0, 1))) ret = "O" + ret.Substring(0, ret.Length - 1);
                return ret;
            }
        }
    }
}
