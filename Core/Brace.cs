using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MUI.Components.ECMAScriptDealer
{
    /// <summary>
    /// 一个大括号单元映像
    /// </summary>
    public class Brace
    {
        public Brace()
        {
        }
        public Brace(Brace parent)
        {
            if (parent == null) return;
            this._parent = parent;
            this._parent.AddChild(this);
        }
        public static void Init()
        {
            if (IsVariablePre && string.IsNullOrEmpty(VariablePre)) VariablePre = "O";
            RandomNameBox.BDigit = IsVariablePre;
            RandomNameBox.VarLength = VarLength;
            RandomNameBox.Seed = Seed;
            RandomNameBox.CreateAllRandom();
        }
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
            this._children.Clear();
        }
        #endregion
        public static Brace Root { get; set; }
        public Brace Parent { get { return this._parent; } set { this._parent = value; } }
        public List<Brace> Children { get { return this._children; } set { this._children = value; } }
        public Brace BraceType { get; set; }
        public string FunctionName { 
            //单元是否属于函数名或类名变量
            get { return this._functionName; }
            set
            {
                this._functionName = value;
                Variable v=this.GetVar(this._functionName, false);
                if(v!=null) v.VBrace = this;
            }
        }
        public static bool IsConfuseVariable = true;//混淆变量名
        public static bool IsConfuseFunctionName = true;//混淆函数名或类构造函数名
        public static bool IsConfuseClassMember = true;//混淆类的成员
        public static string NoVariable;//不混淆的变量名称(用","分隔)
        public static string NoClassMember;//不混淆的类成员名称(用","分隔)
        public static bool IsConfuseRootGlobalVar = true;//是否混淆根全局变量
        public static bool IsConfusePre_ = true;//是否混淆以下划线起始的变量
        public static bool IsConfusePreS = true;//是否混淆以美元符($)起始的标识符
        public static bool IsVariablePre = false;//是否变量名前加前缀
        public static string VariablePre = string.Empty;//变量名前前缀
        public static int VarLength = 0;//变量位数
        public static string Seed = string.Empty;//混淆种子

        private string createRandomName(Variable var)
        {
            if (var is Variable_ClassPro || var is Variable_ClassMethod)
            {
                if (Brace.IsConfusePre_ == false && var.Name.StartsWith("_")) return var.Name;
                if (Brace.IsConfusePreS == false && var.Name.StartsWith("$")) return var.Name;
                if (Brace.IsConfuseClassMember)
                {
                    if (!string.IsNullOrEmpty(NoClassMember) && Helper.Contains(NoClassMember, var.Name)) return var.Name;
                    return this._rnBox_menber.Get(var.Name);
                }
                else
                    return var.Name;
            }
            else
            {
                //if (!IsConfuseVariable && (var is Variable_Common || var is Variable_ObjectClass)) return var.Name;
                //else if (!IsConfuseFunctionName && var is Variable_Function) return var.Name;
                //else if (!string.IsNullOrEmpty(NoVariable) && Helper.Contains(NoVariable, var.Name)) return var.Name;

                //string rn = this._rnBox_var.Get();
                //while (this.ExistsRandomName(rn) || Helper.Contains(NoVariable, rn))
                //{
                //    rn = this._rnBox_var.Get();
                //}
                //this.listNonMemberVarRandomName.Add(rn);
                //return rn;
                return this.createNonMemberRandomName(var);
            }
        }
        private string createNonMemberRandomName(Variable var)
        {
            if (Brace.IsConfusePre_ == false && var.Name.StartsWith("_")) return var.Name;
            if (Brace.IsConfusePreS == false && var.Name.StartsWith("$")) return var.Name;
            if (!IsConfuseVariable && (var is Variable_Common || var is Variable_ObjectClass)) { this.listNonMemberVarNotToRandomName.Add(var.Name); return var.Name; }
            else if (!IsConfuseFunctionName && var is Variable_Function) { this.listNonMemberVarNotToRandomName.Add(var.Name); return var.Name; }
            else if (!string.IsNullOrEmpty(NoVariable) && Helper.Contains(NoVariable, var.Name)) return var.Name;

            int i = 0;
            string rn = this._rnBox_var.Get(var.Name);
            while (this.ExistsRandomName(rn) || Helper.Contains(NoVariable, rn) || this.ExistsNotToRandomName(rn))
            {
                rn = this._rnBox_var.Get(var.Name+(++i).ToString());
            }
            this.listNonMemberVarRandomName.Add(rn);
            return rn;
        }
        public void AddNonMemberNotToRandomName(string word)
        {
            if (!this.Exists(word))
            {
                if (!this.listNonMemberVarNotToRandomName.Contains(word))
                    this.listNonMemberVarNotToRandomName.Add(word);
            }
        }
        public string GetResultName(string word, bool bMember)
        {
            string rword = string.Empty;
            Brace b = this;
            while (b != null)
            {
                if (bMember)
                {
                    if (b.dicMemberVariable.ContainsKey(word))
                    {
                        rword = b.dicMemberVariable[word].RandomName;
                        break;
                    }
                }
                else
                {
                    if (b.dicNonMemberVariable.ContainsKey(word))
                    {
                        rword = b.dicNonMemberVariable[word].RandomName;
                        break;
                    }
                }
                b = b.Parent;
            }
            if (string.IsNullOrEmpty(rword)) return word;
            if (word.Equals(rword)) return word;
            if (Brace.IsVariablePre && !rword.Equals(string.Empty))
            {
                return string.Concat(Brace.VariablePre, rword);
            }
            return rword;
        }
        public Brace GetBrace(string word, bool bMember)
        {
            Brace b = this;
            while (b != null)
            {
                if (bMember)
                {
                    if (b.dicMemberVariable.ContainsKey(word)) return b;
                }
                else
                {
                    if (b.dicNonMemberVariable.ContainsKey(word)) return b;
                }
                b = b.Parent;
            }
            return b;
        }
        public Brace GetVBrace(string className)
        {
            Brace b = this;
            while (b != null)
            {
                if (b.dicNonMemberVariable.ContainsKey(className)) return b.dicNonMemberVariable[className].VBrace;
                b = b.Parent;
            }
            return b;
        }
        public Variable GetVar(string word, bool bMember)
        {
            Brace b = this;
            while (b != null)
            {
                if (bMember)
                {
                    if (b.dicMemberVariable.ContainsKey(word)) return b.dicMemberVariable[word];
                }
                else
                {
                    if (b.dicNonMemberVariable.ContainsKey(word)) return b.dicNonMemberVariable[word];
                }
                b = b.Parent;
            }
            return null;
        }
        public void Add(Variable variable)
        {
            this.Add(variable, null);
        }
        public void Add(Variable variable, string className)
        {
            if (this.Parent == null)
            {
                //根全局变量是否混淆
                if (!IsConfuseRootGlobalVar && (variable is Variable_ObjectClass || variable is Variable_Common)) return;
            }
            if (variable is Variable_ClassPro || variable is Variable_ClassMethod)
            {
                if (this.dicMemberVariable.ContainsKey(variable.Name)) return;
                this.dicMemberVariable.Add(variable.Name, variable);
                variable.RandomName = createRandomName(variable);
                this.memberVariableCount++;
            }
            else
            {
                if (this.dicNonMemberVariable.ContainsKey(variable.Name)) return;
                this.dicNonMemberVariable.Add(variable.Name, variable);
                //variable.RandomName = createRandomName(variable);//在SetAllNonMemberVarRandomName中统一设置
                if (variable is Variable_Function)
                {
                }
                else if (variable is Variable_ObjectClass)
                {
                    variable.VBrace = this.GetVBrace(className);
                    ((Variable_ObjectClass)variable).ClassName = className;
                }
                this.nonMemberVariableCount++;
                this.count++;
            }
        }
        public void Modify(string word, Type t)
        {
            Modify(word, t, null);
        }
        public void Modify(string word, Type t,string className)
        {
            Brace b=this;
            while (b != null)
            {
                if (b.dicNonMemberVariable.ContainsKey(word))
                {
                    if (b.dicNonMemberVariable[word] is Variable_Function) break;
                    if (t.Name == Type.GetType("MUI.Components.ECMAScriptDealer.Variable_Function").Name)
                    {
                        Variable_Function vf = new Variable_Function(word);
                        vf.RandomName = b.dicNonMemberVariable[word].RandomName;
                        b.dicNonMemberVariable[word] = vf;
                        break;
                    }
                    else if (t.Name == Type.GetType("MUI.Components.ECMAScriptDealer.Variable_ObjectClass").Name)
                    {
                        Variable_ObjectClass vf = new Variable_ObjectClass(word);
                        vf.VBrace = this.GetVBrace(className);
                        vf.RandomName = b.dicNonMemberVariable[word].RandomName;
                        b.dicNonMemberVariable[word] = vf;
                        break;
                    }
                }
                b=b.Parent;
            }
        }
        public void Check(string word)
        {
            //根全局变量是否混淆
            if (IsConfuseRootGlobalVar)
            {
                //未用var定义的变量
                if (this.Exists(word)) return;
                else
                {
                   //this.getRoot().Add(new Variable_Common(word));
                }

            }
        }
        public Variable FindVariable(string word)
        {
            Brace b = this;
            while (b != null)
            {
                if (b.dicNonMemberVariable.ContainsKey(word)) return b.dicNonMemberVariable[word];
                b = b.Parent;
            }
            return null;
        }
        public bool Exists(string word)
        {
            Brace b = this;

            while (b != null)
            {
                if (b.dicNonMemberVariable.ContainsKey(word)) return true;
                b = b.Parent;
            }
            return false;
        }
        public bool ExistsRandomName(string name)
        {
            Brace b = this.Parent;
            while (b != null)
            {
                if (b.listNonMemberVarRandomName.Contains(name)) return true;
                b = b.Parent;
            }
            return false;
        }
        public bool ExistsNotToRandomName(string name)
        {
            Brace b = this;
            while (b != null)
            {
                if (b.listNonMemberVarNotToRandomName.Contains(name)) return true;
                b = b.Parent;
            }
            return false;
        }
        public Brace Find(string functionName)
        {
            Brace b = this.GetBrace(functionName, false);
            return b;
        }
        //public string GetRandomName(string word,bool isMember)
        //{
        //    if (isMember)
        //    {
        //        return dicMemberVariable[word].RandomName;
        //    }
        //    else
        //    {
        //        return dicNonMemberVariable[word].RandomName;
        //    }
        //}
        ////未用
        //public string GetRandomName()
        //{
        //    return this._rnBox_var.Get();
        //}
        public void AddChild(Brace brace)
        {
            this.Children.Add(brace);
        }        
        public void SetAllNonMemberVarRandomName()
        {
            foreach (string key in this.dicNonMemberVariable.Keys)
            {
                this.dicNonMemberVariable[key].RandomName = this.createNonMemberRandomName(this.dicNonMemberVariable[key]);
            }
            foreach (Brace b in this.Children)
            {
                b.SetAllNonMemberVarRandomName();
            }
        }
        private Brace getRoot()
        {
            //Brace b = this;
            //while (b.Parent != null)
            //{
            //    b = b.Parent;
            //}
            //return b;
            return Brace.Root;
        }
        string _functionName;//
        List<Brace> _children=new List<Brace>();//子集
        Brace _parent;//父辈
        Brace _braceType;//类型
        int count;//定义变量数
        int memberVariableCount = 0;//非类成员变量数Variable_Common、Variable_Function、Variable_ObjectClass三者总和
        int nonMemberVariableCount = 0;//类成员变量数Variable_ClassPro、Variable_ClassMethod二者和
        RandomNameBox _rnBox_var = new RandomNameBox();
        RandomNameBox _rnBox_menber = new RandomNameBox();
        Dictionary<string, Variable> dicMemberVariable = new Dictionary<string, Variable>();
        Dictionary<string, Variable> dicNonMemberVariable = new Dictionary<string, Variable>();
        List<string> listNonMemberVarRandomName = new List<string>();
        List<string> listNonMemberVarNotToRandomName = new List<string>();
    }

    public enum BraceType
    {
        Function=1,
        Constructor=2,
        Event=3,
        Other=4
    }

}
