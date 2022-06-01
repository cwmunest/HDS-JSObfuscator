using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MUI.Components.ECMAScriptDealer
{
    #region struct ConfusionOptions
    public struct ConfusionOptions
    {
        public bool IsMoveStr;
        public bool IsEncodeStr;
        public bool IsEncodeOnlyChineseStr;
        public bool IsIntToHex;
        public bool IsConfuseVariable;
        public bool IsConfuseFunctionName;
        public bool IsConfuseClassMember;
        public string NoVariable;
        public string NoClassMember;

        public bool IsConfuseRootGlobalVar;
        public bool IsConfusePre_;
        public bool IsConfusePreS;
        public bool IsVariablePre;
        public string VariablePre;
        public int VarLength;
        public string Seed;

        public bool IsUseSquareBracket;
        public bool IsIncludeSystemObject;

    }
    #endregion
    public class Confusion
    {
		public Confusion()
		{
        }
        public Confusion(string code)
        {
            this.Code = code;
        }
        public Confusion(string code,ConfusionOptions opts)
        {
            this.Code = code;
            //this.IsMoveStr = opts.IsMoveStr;
            //this.IsEncodeStr = opts.IsEncodeStr;
            //this.IsIntToHex = opts.IsIntToHex;
            //this.IsConfuseVariable = opts.IsConfuseVariable;
            //this.IsConfuseFunctionName = opts.IsConfuseFunctionName;
            //this.IsConfuseClassMember = opts.IsConfuseClassMember;
            //this.NoVariable = opts.NoVariable;
            //this.NoClassMember = opts.NoClassMember;
            SetConfusionOptions(opts);
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
            this.list_word.Clear();
            this.list_out.Clear();
            this.list_strings.Clear();
            //this.list_regexp.Clear();
            this.bm.Clear();
        }
        #endregion
        public string Code { get; set; }
        public string IgnoreString { get { return this._ignore; } set { this._ignore = value; } }
        public Dictionary<string, string> IgnoreSentence = new Dictionary<string, string>();
        public int FileNum { get; set; }//混淆文件数

        public Dictionary<int, string> DicFilename = new Dictionary<int, string>();//文件名
        public bool _caseSensitive = true;//是否大小写敏感

        public bool IsMoveStr = true;//移动字符串
        public bool IsMerge = true;//是否合并文件
        public bool IsEncodeStr = true;//转义字符串
        public bool IsEncodeOnlyChineseStr = false;//只转义中文字符串
        public bool IsIntToHex = true;//是否将整数用"0x"十六进制形式表示
        public bool IsConfuseVariable = true;//混淆变量名
        public bool IsConfuseFunctionName = true;//混淆函数名或类构造函数名
        public bool IsConfuseClassMember = true;//混淆类的成员
        public string NoVariable;//不混淆的变量名称(用","分隔)
        public string NoClassMember;//不混淆的类成员名称(用","分隔)


        public bool IsConfuseRootGlobalVar = true;//是否混淆根全局变量
        public bool IsConfusePre_ = true;//是否混淆以下划线起始的变量
        public bool IsConfusePreS = true;//是否混淆以美元符($)起始的标识符
        public bool IsVariablePre = false;//是否变量名前加前缀
        public string VariablePre = string.Empty;//变量名前前缀
        public int VarLength = 0;//变量位数
        public string Seed = string.Empty;//混淆种子


        public bool IsUseSquareBracket = false;//是否使用中括号访问成员
        public bool IsIncludeSystemObject = false;//是否包括系统对象成员
        ECMAScriptPacker sp = new ECMAScriptPacker();
        public void SetConfusionOptions(ConfusionOptions opts)
        {
            this.IsMoveStr = opts.IsMoveStr;
            this.IsEncodeStr = opts.IsEncodeStr;
            this.IsEncodeOnlyChineseStr = opts.IsEncodeOnlyChineseStr;
            this.IsIntToHex = opts.IsIntToHex;
            this.IsConfuseVariable = opts.IsConfuseVariable;
            this.IsConfuseFunctionName = opts.IsConfuseFunctionName;
            this.IsConfuseClassMember = opts.IsConfuseClassMember;
            this.NoVariable = opts.NoVariable;
            this.NoClassMember = opts.NoClassMember;

            this.IsConfuseRootGlobalVar = opts.IsConfuseRootGlobalVar;
            this.IsConfusePre_ = opts.IsConfusePre_;
            this.IsConfusePreS = opts.IsConfusePreS;
            this.IsVariablePre = opts.IsVariablePre;
            this.VariablePre = opts.VariablePre;
            this.VarLength = opts.VarLength;
            this.Seed = opts.Seed;

            this.IsUseSquareBracket = opts.IsUseSquareBracket;
            this.IsIncludeSystemObject = opts.IsIncludeSystemObject;
        }
        public string Exec()
        {
            return this.Exec(false);
        }
        public string Exec(bool bEncrypt)
        {
            this._wordDelimiters = this._wordDelimiters2;
            this.list_strings.Clear();
            //this.list_regexp.Clear();
            if (this.IsMoveStr && !this.IsMerge && this.FileNum > 1)
            {
                this.dic_strings = new Dictionary<int, List<string>>();
                for (int i = 1; i <= this.FileNum; i++)
                {
                    this.dic_strings.Add(i, new List<string>());
                }
            }

            sp.IsRestore = false;
            sp.AddSemicolon = false;
            string script = this.Code;
            script = this.replaceTSJZ(script);
            script = this.sp.replaceTSFH(script);
            script = sp.Compression(script);
            //this.list_strings = sp.ListStrings;
            //this.list_regexp = sp.ListRegexp;

            this.SplitScript(script + "\n");
            this.GetBraceMap();
            this.Translate();
            script = string.Join("", this.list_out.ToArray());

            script = this.sp.restoreTSFH(script);
            script = this.restoreTSJZ(script);

            if (this.IsMoveStr && (this.IsMerge || this.FileNum == 1))
                script = this.getMoveStringArrays(script);

            if (bEncrypt)
            {
                sp.HaveCompressed = true;
                script = sp.Pack(script);
            }
            return script;
        }
        //替换特殊句子
        public string replaceTSJZ(string input)
        {
            foreach (string key in IgnoreSentence.Keys)
            {
                input = input.Replace(IgnoreSentence[key], key);
            }
            return input;
        }
        public string restoreTSJZ(string input)
        {
            foreach (string key in IgnoreSentence.Keys)
            {
                input = input.Replace(key, IgnoreSentence[key]);
            }
            return input;
        }
        public string GetFileMoveStringArrays(int fileNo)
        {
            string ret = "";
            if (this.IsMoveStr && this.dic_strings[fileNo].Count > 0)
            {
                if (this.IsEncodeStr)
                {
                    for (int i = 0, l = this.dic_strings[fileNo].Count; i < l; i++)
                    {
                        this.dic_strings[fileNo][i] = this.getEncodeStrings(this.dic_strings[fileNo][i]);
                    }
                }
                ret = string.Format("var {0}{1}=[{2}];",this.arrayNameToMovestr, isMD5FileName? DicFilename[fileNo]: fileNo.ToString(), string.Join(",", this.dic_strings[fileNo].ToArray()));
            }
            return ret;
        }

        private bool isMD5FileName = false;
        private string arrayNameToMovestr = "_$";//存放移动字符串的数据变量名
        #region
        private string getMoveStringArrays(string input)
        {
            if (this.IsMoveStr && this.list_strings.Count > 0)
            {
                if (this.IsEncodeStr)
                {
                    for (int i = 0, l = this.list_strings.Count; i < l; i++)
                    {
                        this.list_strings[i] = this.getEncodeStrings(this.list_strings[i]);
                    }
                }
                input = string.Concat("var ",this.arrayNameToMovestr, "=[", string.Join(",", this.list_strings.ToArray()), "];", input);
            }
            return input;
        }
        private string AddGetStrArrMember(string str,bool isAddQuot)
        {
            if (isAddQuot) str = string.Format("\"{0}\"", str);
            if (!this.IsMerge && this.FileNum > 1)
            {
                if (this.dic_strings[this._fileNo].Contains(str))
                {
                    return string.Format("{0}{1}[{2}]", this.arrayNameToMovestr, isMD5FileName ? DicFilename[this._fileNo] : this._fileNo.ToString(), this.dic_strings[this._fileNo].IndexOf(str));
                }
                else
                {
                    this.dic_strings[this._fileNo].Add(str);
                    return string.Format("{0}{1}[{2}]", this.arrayNameToMovestr, isMD5FileName ? DicFilename[this._fileNo] : this._fileNo.ToString(), this.dic_strings[this._fileNo].Count - 1);
                }
            }
            else
            {
                if (this.list_strings.Contains(str))
                {
                    return string.Format("{0}[{1}]", this.arrayNameToMovestr, this.list_strings.IndexOf(str));
                }
                else
                {
                    this.list_strings.Add(str);
                    return string.Format("{0}[{1}]", this.arrayNameToMovestr, this.list_strings.Count - 1);
                }
            }
        }
        private string getEncodeStrings(string str)
        {
            //转义字符串未处理
            //return string.Concat(str.Substring(0, 1), new Regex("([^\\\\]*)(\\\\([^x]|([x][0-9a-zA-Z]{2})))*").Replace((str.Substring(1, str.Length - 2)), this.me_stringToHex), str.Substring(str.Length - 1));
            return string.Concat(str.Substring(0, 1),Helper.StringToHex2(str.Substring(1, str.Length - 2),this.IsEncodeOnlyChineseStr), str.Substring(str.Length - 1));
        }
        //字符串转换为16进制形式
        private string toHex(string str)
        {
            if (!this.IsEncodeStr) return str;
            return Helper.StringToHex2(str);
        }
        #region 无用
        //转义字符串 排除转义字符和十六进制表达式
        private string me_stringToHex(Match match)
        {
            return Helper.StringToHex(match.Groups[1].Value) + match.Groups[2].Value;
        }
        private string restoreStrings(string input)
        {
            if (this.IsEncodeStr)
            {
                for (int i = 0, l = this.list_strings.Count; i < l; i++)
                {
                    //转义字符串未处理
                    this.list_strings[i] = string.Concat(this.list_strings[i].Substring(0, 1), new Regex("([^\\\\]*)(\\\\([^x]|([x][0-9a-zA-Z]{2})))*").Replace((this.list_strings[i].Substring(1, this.list_strings[i].Length - 2)), this.me_stringToHex), this.list_strings[i].Substring(this.list_strings[i].Length - 1));
                }

            }
            if (this.IsMoveStr && this.list_strings.Count>0)
            {
                this.arrayNameToMovestr = "_$";//Brace.Root.GetRandomName();
                input = string.Concat("var ",this.arrayNameToMovestr, "=[", string.Join(",", this.list_strings.ToArray()), "];", input);
                //input = string.Concat("var ", this.arrayNameToMovestr, "=[", this.sp.restoreStringZYFH(string.Join(",", this.list_strings.ToArray())), "];", input);
                return new Regex("ArrayStrings_MUI\\[(\\d+)\\]").Replace(input, new MatchEvaluator(this.me_restoreStrings1));
            }
            else
            {
                return this.sp.restoreStrings(input);
            }
        }
        private string me_restoreStrings1(Match match)
        {
            return match.Groups[0].Value.Replace("ArrayStrings_MUI", this.arrayNameToMovestr);
        }
        #endregion
        #endregion

        #region 私有变量
        string _wordDelimiters = "　 ,.?!;:\\/<>(){}[]\"'\r\n\t=+-|*%@#$^&~";//分割字符  
        string _wordDelimiters2 = "　 ,.?!;:\\/<>(){}[]\"'\r\n\t=+-|*%@#^&~";//分割字符
        string _wordEnd = " ,?!;:\\/<>(){}[]=+-|*";//分割字符
        string _keywords = "break,case,catch,continue,debugger,default,delete,do,else,false,finally,for,function,Function,if,in,instanceof,new,null,return,switch,this,throw,true,try,typeof,var,void,while,with,window";//系统关键字
        //string _function = "function,Function";//声明一个新的函数	
        string _function = "function";//声明一个新的函数	
        string _windowObject = "alert,escape,unescape,document,parseInt,parseFloat,console,onerror";//window的属性和方法		execScript,
        string _commonObjects = "ActiveXObject,Array,Boolean,Date,Enumerator,Error,Global,Math,Number,Object,RegExp,String";//内建对象
        string _executeMothod = "eval,execScript";//字符串当作脚本执行函数
        //string[] _flowKeywords="else,catch,finally,while".Split(new char[]{','});//关键字
        string _var = "var";//声明变量关键字
        string _beginBlock = "{";//块开始字符
        string _endBlock = "}";//块结束字符
        string _quotation = "\",'";//引用字符		
        string _lineComment = "//";//单行注释字符		
        string _escape = "\\";//转义字符
        string _commentOn = "/*";//多行注释开始		
        string _commentOff = "*/";//多行注释结束
        string _call = ".";//引用调用字符
        string _varPause = "=";//变量赋值字符
        string _varContinue = ",";//连续声明变量字符
        string _ignore = "";//忽略的字符串,用于文件内容间分隔符,以分割符开始
        int _fileNo = 1;//混淆第几个文件

        int word_num = 0; 
        List<string> list_word = new List<string>();//用于存储分隔后的字符串数组
        List<string> list_out = new List<string>();//用于存储输出结果的数组
        List<string> list_strings = new List<string>();//移动字符串时存储所有字符串的数组
        Dictionary<int, List<string>> dic_strings = null;//单独文件 移动字符串时存储所有字符串的数组
        //List<string> list_regexp = new List<string>();//临时存储所有正则表达式的数组
        BraceManager bm = new BraceManager();
        #endregion

        // 变量初始化
        private void SplitScript(string script)
        {
            //清除变量
            this.list_word.Clear();
            this.list_out.Clear();
            this.bm.Clear();

            //建立分隔后的字符串数组(分词)
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < script.Length; i++)
            {
                if (this._wordDelimiters.IndexOf(script[i]) == -1)
                {
                    //找不到关键字
                    sb.Append(script[i]);
                }
                else
                {
                    if (sb.Length > 0) this.list_word.Add(sb.ToString());
                    this.list_word.Add(script[i].ToString());
                    sb.Length=0;
                }
            }
            word_num = this.list_word.Count-1;

        }
        //获得代码层级映像（即大括号层级），按分割字，分块显示
        private void GetBraceMap()
        {
            Brace.IsConfuseVariable = this.IsConfuseVariable;
            Brace.IsConfuseFunctionName = this.IsConfuseFunctionName;
            Brace.IsConfuseClassMember = this.IsConfuseClassMember;
            Brace.NoVariable = this.NoVariable;
            Brace.NoClassMember = this.NoClassMember;
            Brace.IsConfuseRootGlobalVar = this.IsConfuseRootGlobalVar;
            Brace.IsConfusePre_ = this.IsConfusePre_;
            Brace.IsConfusePreS = this.IsConfusePreS;
            Brace.IsVariablePre = this.IsVariablePre;
            Brace.VariablePre = this.VariablePre;
            Brace.VarLength = this.VarLength;
            Brace.Seed = this.Seed;
            Brace.Init();

            Brace cur_brace = new Brace();//根级的上级为null
            Brace.Root = cur_brace;
            this.bm.Add(0, cur_brace);

            bool var_opened = false;
            bool var_pause = false;
            bool function_opened = false;
            bool parameter_opened = false;
            bool execute_opened = false;//处理eval后的字符串
            bool braket_opened = false;//()
            bool square_braket_opened = false;//[]
            bool call_opened = false;
			string call_string = "";
            string word = "";
            string tmp = "";
            string tmp2 = "";

            bool quote_opened = false; //引用标记
            string quote_char = ""; //引用标记类型
            bool regexp_opened = false;//正则表达式

            for (int i = 0; i < word_num; i++)
            {
                word = (string)this.list_word[i]; 
                if (quote_opened)
                {
                    if (Helper.Contains2(this._quotation, word))
                    {
                        //处理引号里的引号
                        if (quote_char == word)
                        {
                            //是相应的引号
                            quote_opened = false;
                            quote_char = "";
                        }
                        else
                        {
                        }
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        if (i < word_num - 1)
                        {
                            i = i + 1;
                            continue;
                        }
                    }
                    else
                    {
                    }
                }
                else if (regexp_opened)
                {
                    //处理转义字符
                    if (word == "/")
                    {
                        regexp_opened = false;
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        if (i < word_num - 1)
                        {
                            i = i + 1;
                            continue;
                        }
                    }
                    else
                    {
                    }
                }
                else
                {
                    if (call_opened)
                    {
                        //单独处理指针引用
                        if (call_string.Length == 0)
                        {
                            call_string += this.list_word[i];
                        }
                        else
                        {
                            if (!isVar(word))
                            {
                                tmp = this.getPreNumNoBlankStr(i, 3);
                                if (tmp == "window" && (word == "=" || this.getNextNoBlankStr(i) == "="))
                                {
                                    //诸如"window._State=1;"、"window._System = new System();"形式地定义变量
                                    if (this.getNextNumNoBlankStr(i - 1, 2) == "new")
                                    {
                                        //添加类对象变量
                                        Brace.Root.Add(new Variable_ObjectClass(call_string), this.getNextNumNoBlankStr(i - 1, 3));
                                    }
                                    else if (this.getNextNumNoBlankStr(i - 1, 2) == "function")
                                    {
                                        //添加函数或类定义变量
                                        if (this.isWindowObject(call_string) == false)
                                            Brace.Root.Add(new Variable_Function(call_string));
                                    }
                                    else
                                    {
                                        //添加普通变量
                                        //变量符合命名规则，并且（变量前为空格或制表符或逗号如：var a;var a;var a,b;，还有如果是函数参数,如：function(a,b,c)
                                        if (this.isWindowObject(call_string) == false)
                                            Brace.Root.Add(new Variable_Common(call_string));
                                    }
                                    call_string = "";
                                    call_opened = false;
                                }
                                else
                                {
                                    if (tmp == "this")
                                    {
                                        if (cur_brace.FunctionName != null && this.getNextNoBlankStr(i - 1) != "(")
                                        {
                                            //if (word == "=" && this.getNextNoBlankStr(i) == "function")
                                            //    cur_brace.Add(new Variable_ClassMethod(call_string));
                                            //else
                                            //    cur_brace.Add(new Variable_ClassPro(call_string));

                                            if (word == "=" && this.getNextNoBlankStr(i) == "function")
                                                cur_brace.Add(new Variable_ClassMethod(call_string), cur_brace.FunctionName);
                                            else
                                                cur_brace.Add(new Variable_ClassPro(call_string), cur_brace.FunctionName);
                                        }
                                    }
                                    else if (tmp == "prototype")
                                    {
                                        tmp = this.getPreNumNoBlankStr(i, 5);
                                        Variable_ClassMethod vcm = new Variable_ClassMethod(call_string);
                                        vcm.ClassName = tmp;
                                        Brace b = cur_brace.GetVBrace(vcm.ClassName);
                                        if (b != null) b.Add(vcm);
                                    }
                                    //										else if(tmp=="_p")
                                    //										{
                                    //											if(cur_function!=null && this.hashFunction.ContainsKey(cur_function))
                                    //											{
                                    //												tmp=this.getRandName(call_string);
                                    //												((ClassMembers)this.hashFunction[cur_function]).Add(call_string,tmp);
                                    //												call_string=tmp;
                                    //											}
                                    //										}
                                    else
                                    {
                                    }
                                }
                                call_string = "";
                                if (word != this._call)
                                {
                                    call_opened = false;
                                }
                                goto Label1;
                            }
                        }
                        continue;
                    }

                Label1:
                    if (word == " " || word == "\t" || word == "\n" || word == "\r") continue;
                    else if (word == this._var)//var
                    {
                        var_opened = true;
                        var_pause = false;
                    }
                    else if (word == this._varPause)//=
                    {
                        var_pause = true;
                    }
                    else if (word == this._varContinue)//,
                    {
                        if (!((braket_opened && function_opened == false) || square_braket_opened))
                            var_pause = false;
                    }
                    else if (word == "function")
                    {
                        function_opened = true;
                        var_pause = false;
                    }
                    else if (word == ";")
                    {
                        var_opened = false;
                        var_pause = false;
                        execute_opened = false;
                    }
                    else if (word == "(")
                    {
                        braket_opened = true;
                        if (function_opened == true)
                        {
                            var_opened = true;
                            parameter_opened = true;
                            function_opened = false;
                            cur_brace = new Brace(cur_brace);
                            this.bm.Add(i, cur_brace);
                            tmp = this.getPreNoBlankStr(i);
                            if (tmp == "function")
                            {
                                tmp = this.getPreNumNoBlankStr(i, 2);
                                //if ((tmp == "=" || tmp == ":") && this.getPreNumNoBlankStr(i, 4) != "." && !this.isStartWith("this.", i - 5) && !this.isStartWith("prototype.", i - 5))
                                if ((tmp == "=") && this.getPreNumNoBlankStr(i, 4) != "." && !this.isStartWith("this.", i - 5) && !this.isStartWith("prototype.", i - 5))
                                {
                                    cur_brace.FunctionName = this.getPreNumNoBlankStr(i, 3);
                                }
                            }
                            else
                            {
                                cur_brace.FunctionName = tmp;
                            }
                        }
                    }
                    else if (word == ")")
                    {
                        braket_opened = false;
                        execute_opened = false;
                        if (parameter_opened)
                        {
                            var_opened = false;
                            var_pause = false;
                        }
                    }
                    else if (word == "[")
                    {
                        square_braket_opened = true;
                    }
                    else if (word == "]")
                    {
                        square_braket_opened = false;
                    }
                    else if (word == this._beginBlock)
                    {
                        //?
                        if (function_opened)
                            var_opened = false;
                        if (parameter_opened)
                        {
                            //函数参数应包括到下一个层级
                            parameter_opened = false;
                        }
                        else
                        {
                            cur_brace = new Brace(cur_brace);
                            this.bm.Add(i, cur_brace);
                        }
                    }
                    else if (word == this._endBlock)
                    {
                        cur_brace = cur_brace.Parent;
                    }
                    else if (word == this._call)
                    {
                        //处理引用调用
                        //判断引用(.)后面第一个是否为字母_$
                        if (!function_opened)
                        {
                            if (isVar(this.list_word[i + 1].ToString()))
                            {
                                call_opened = true;
                            }
                        }
                    }
                    else if (this.isKeyword(word))
                    {
                        //处理关键字
                        if (word == "in") var_opened = false;
                    }
                    else if (this.isExecuteMothod(word))
                    {
                        //处理eval后的字符串
                        execute_opened = true;

                    }
                    else if (this.isWindowObject(word))
                    {
                        //window内置对象						
                    }
                    else if (this.isCommonObject(word))
                    {
                        //处理普通对象
                    }
                    else if (Helper.Contains2(this._quotation, word) && regexp_opened == false)
                    {
                        //处理引号（引号前不能为转义字符）
                        quote_opened = true;
                        quote_char = word;
                    }
                    else if (word == "/")
                    {
                        //正则表达式
                        tmp = this.getPreNoBlankStr(i);
                        if (regexp_opened == false && (tmp == ")" || tmp == "]" || Helper.IsVar(tmp) || Helper.IsNumber(tmp))) continue;
                        if (regexp_opened)
                            regexp_opened = false;
                        else
                            regexp_opened = true;
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        if (i < word_num - 1)
                        {
                            i = i + 1;
                            continue;
                        }
                    }
                    else if (isVar(word))
                    {
                            tmp=this.getNextNumNoBlankStr(i, 1);
                            if(i>0) tmp2=this.list_word[i - 1].ToString();
                        if (function_opened)
                        {
                            //添加函数或类定义变量
                            if (tmp2 == " " || tmp2 == "\t")
                            {
                                cur_brace.Add(new Variable_Function(word));
                            }
                        }
                        else if (var_opened && !var_pause && this.list_word[i + 1].ToString() != "." && (tmp2 == " " ||
                            tmp2 == "\t" ||
                            tmp2 == this._varContinue || parameter_opened))
                        {
                            if (tmp == "=" && this.getNextNumNoBlankStr(i, 2) == "new")
                            {
                                //添加类对象变量
                                cur_brace.Add(new Variable_ObjectClass(word), this.getNextNumNoBlankStr(i, 3));
                            }
                            else if (tmp == "=" && this.getNextNumNoBlankStr(i, 2) == "function")
                            {
                                //添加函数或类定义变量
                                cur_brace.Add(new Variable_Function(word));
                            }
                            //else if (tmp == "=" || (tmp == "," && (parameter_opened || !braket_opened)) || tmp == ";" || (tmp == ")" && parameter_opened) || tmp == "in")
                            else if (tmp == "=" || tmp == "," || tmp == ";" || (tmp == ")" && parameter_opened) || tmp == "in")
                            {
                                //添加普通变量
                                //变量符合命名规则，并且（变量前为空格或制表符或逗号如：var a;var a;var a,b;，还有如果是函数参数,如：function(a,b,c)				
/** 这个情况尚不能处理
var a = 1;
var b = 2;
function xx(num, str,aaa) {
var c1,c2,c3;
var q3=xx(xx(c1,c2,c3),a,b);//此处的a会出错
}
**/
                                cur_brace.Add(new Variable_Common(word));
                            }
                        }
                        else
                        {
                            if (tmp == "=" && this.getNextNumNoBlankStr(i, 2) == "new")
                            {
                                //修改普通变量类型为 类对象变量
                                cur_brace.Modify(word, typeof(Variable_ObjectClass), this.getNextNumNoBlankStr(i, 2));
                            }
                            else if (tmp == "=" && this.getNextNumNoBlankStr(i, 2) == "function")
                            {
                                //添加普通变量类型为函数或类定义变量
                                cur_brace.Modify(word, typeof(Variable_Function));
                            }
                            else if (tmp == "=")
                            {
                                //
                                cur_brace.Check(word);
                            }
                            else
                            {
                                cur_brace.AddNonMemberNotToRandomName(word);
                            }

                        }
                    }
                    else if (this.isStartWith(this._ignore, i))
                    {
                        //处理忽略词
                        i = i + this._ignore.Length - 1;
                    }
                    else
                    {

                    }
                }
            }
            Brace.Root.SetAllNonMemberVarRandomName();
        }        
        private void Translate()
        {
            bool var_opened = false;
            bool var_pause = false;
            bool function_opened = false;
            bool parameter_opened = false;
            bool execute_opened = false;//处理eval后的字符串
            bool braket_opened = false;//()
            bool square_braket_opened = false;//[]
            bool call_opened = false;
			string call_string = "";
            string word = "";
            string tmp = "",tmp2="";
            string str = "";
            string pre="";

            bool quote_opened = false; //引用标记
            string quote_char = ""; //引用标记类型
            bool regexp_opened = false;//正则表达式
            bool regexp_end = false;//正则表达式

            StringBuilder sb = new StringBuilder();
            Brace cur_brace = Brace.Root;
            for (int i = 0; i < word_num; i++)
            {
                word = (string)this.list_word[i];
                if (quote_opened)
                {
                    if (Helper.Contains2(this._quotation, word))
                    {
                        //处理引号里的引号
                        if (quote_char == word)
                        {
                            //是相应的引号
                            sb.Append(word);
                            quote_opened = false;
                            quote_char = "";
                            str = sb.ToString();
                            //
                            if (execute_opened)
                            {
                                str = this.TranslatePiece(cur_brace, str);
                            }
                            if (this.IsMoveStr)
                            {
                                //防止诸如{"abc":"skdf","cdf":"skdf"}中的“abc”数据被移动
                                tmp = this.getPreNumNoBlankStr(i, 3);
                                if (this.getNextNoBlankStr(i) != ":" || (tmp != "{" && tmp != ","))
                                {
                                    #region
                                    //if (!this.IsMerge && this.FileNum > 1)
                                    //{
                                    //    //if (pre != " ") this.list_out.Add(" ");
                                    //    //if (!Helper.Contains(_wordEnd, pre)) this.list_out.Add(" ");
                                    //    if (this.isVar(pre)) this.list_out.Add(" ");
                                    //    if (this.dic_strings[this._fileNo].Contains(str))
                                    //    {
                                    //        this.list_out.Add(string.Format("{0}{1}[{2}]", this.arrayNameToMovestr, isMD5FileName ? DicFilename[this._fileNo] : this._fileNo.ToString(), this.dic_strings[this._fileNo].IndexOf(str)));
                                    //    }
                                    //    else
                                    //    {
                                    //        this.dic_strings[this._fileNo].Add(str);
                                    //        this.list_out.Add(string.Format("{0}{1}[{2}]", this.arrayNameToMovestr, isMD5FileName ? DicFilename[this._fileNo] : this._fileNo.ToString(), this.dic_strings[this._fileNo].Count - 1));
                                    //    }
                                    //}
                                    //else
                                    //{
                                    //    //if (pre != " ") this.list_out.Add(" ");
                                    //    //if (!Helper.Contains(_wordEnd, pre)) this.list_out.Add(" ");
                                    //    if (this.isVar(pre)) this.list_out.Add(" ");
                                    //    if (this.list_strings.Contains(str))
                                    //    {
                                    //        this.list_out.Add(this.arrayNameToMovestr + string.Format("[{0}]", this.list_strings.IndexOf(str)));
                                    //    }
                                    //    else
                                    //    {
                                    //        this.list_strings.Add(str);
                                    //        this.list_out.Add(this.arrayNameToMovestr + string.Format("[{0}]", this.list_strings.Count - 1));
                                    //    }
                                    //}
                                    //pre = "";
                                    #endregion
                                    if (this.isVar(pre)) this.list_out.Add(" ");
                                    this.list_out.Add(AddGetStrArrMember(str,false));
                                    pre = "";
                                }
                                else
                                {
                                    if (this.IsEncodeStr)
                                        this.list_out.Add(this.getEncodeStrings(str));
                                    else
                                        this.list_out.Add(str);
                                }
                            }
                            else
                            {
                                if (this.IsEncodeStr)
                                    this.list_out.Add(this.getEncodeStrings(str));
                                else
                                    this.list_out.Add(str);
                            }
                            sb.Length = 0;
                        }
                        else
                        {
                            sb.Append(word);
                        }
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        sb.Append(word);
                        if (i < word_num - 1)
                        {
                            sb.Append(this.list_word[i + 1]);
                            i = i + 1;
                            continue;
                        }
                    }
                    else
                    {
                        sb.Append(word);
                    }
                }
                else if (regexp_opened)
                {
                    //处理转义字符
                    if (word == "/")
                    {
                        this.list_out.Add(word);
                        regexp_opened = false;
                        regexp_end = true;
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        this.list_out.Add(word);
                        if (i < word_num - 1)
                        {
                            this.list_out.Add(this.list_word[i + 1]);
                            i = i + 1;
                            continue;
                        }
                    }
                    else
                    {
                        this.list_out.Add(word);
                    }
                }
                else
                {
                    if (call_opened)
                    {
                        #region 单独处理指针引用
                        if (call_string.Length == 0)
                        {
                            call_string += this.list_word[i];
                        }
                        else
                        {
                            if (!isVar(word))
                            {
                                tmp = this.getPreNumNoBlankStr(i, 3);
                                if (tmp == "window" && (word == "=" || this.getNextNoBlankStr(i) == "="))
                                {
                                    if (this.IsUseSquareBracket)
                                    {
                                        if (this.IsMoveStr)
                                            this.list_out.Add(string.Format("[{0}]", AddGetStrArrMember(Brace.Root.GetResultName(call_string, false),true)));
                                        else
                                            this.list_out.Add("[\"" + this.toHex(Brace.Root.GetResultName(call_string, false)) + "\"]");
                                    }
                                    else
                                        this.list_out.Add("." + Brace.Root.GetResultName(call_string, false));
                                }
                                else
                                {
                                    if (tmp == "this")
                                    {
                                        if (this.IsUseSquareBracket)
                                        {
                                            if (this.IsMoveStr)
                                                this.list_out.Add(string.Format("[{0}]", AddGetStrArrMember(cur_brace.GetResultName(call_string, true), true)));
                                            else
                                                this.list_out.Add("[\"" + this.toHex(cur_brace.GetResultName(call_string, true)) + "\"]");
                                        }
                                        else
                                            this.list_out.Add("." + cur_brace.GetResultName(call_string, true));
                                    }
                                    else if (tmp == "prototype")
                                    {
                                        tmp = this.getPreNumNoBlankStr(i, 5);
                                        Brace b = cur_brace.GetVBrace(tmp);
                                        if (b != null)
                                        {
                                            if (this.IsUseSquareBracket)
                                            {
                                                if (this.IsMoveStr)
                                                    this.list_out.Add(string.Format("[{0}]", AddGetStrArrMember(cur_brace.GetVBrace(tmp).GetResultName(call_string, true), true)));
                                                else
                                                    this.list_out.Add("[\"" + this.toHex(cur_brace.GetVBrace(tmp).GetResultName(call_string, true)) + "\"]");
                                            }
                                            else
                                                this.list_out.Add("." + cur_brace.GetVBrace(tmp).GetResultName(call_string, true));
                                        }
                                        else
                                        {
                                            this.list_out.Add("." + call_string);
                                        }
                                    }
                                    else
                                    {
                                        Variable v = cur_brace.GetVar(tmp, false);
                                        ////如果类定义在引用代码的后面则在此补类变量的所属类领域.VBrace
                                        if (v != null && v.VBrace == null && !string.IsNullOrEmpty(v.ClassName))
                                        {
                                            v.VBrace = cur_brace.GetVBrace(v.ClassName);
                                        }
                                        if (v != null && v.VBrace != null)
                                        {
                                            if (this.IsUseSquareBracket)
                                            {
                                                if (this.IsMoveStr)
                                                    this.list_out.Add(string.Format("[{0}]", AddGetStrArrMember(v.VBrace.GetResultName(call_string, true), true)));
                                                else
                                                    this.list_out.Add("[\"" + this.toHex(v.VBrace.GetResultName(call_string, true)) + "\"]");
                                            }
                                            else
                                                this.list_out.Add("." + v.VBrace.GetResultName(call_string, true));
                                        }
                                        else
                                        {
                                            if (this.IsUseSquareBracket)
                                            {
                                                if (this.IsMoveStr)
                                                    this.list_out.Add(string.Format("[{0}]", AddGetStrArrMember(cur_brace.GetResultName(call_string, true), true)));
                                                else
                                                    this.list_out.Add("[\"" + this.toHex(cur_brace.GetResultName(call_string, true)) + "\"]");
                                            }
                                            else
                                                this.list_out.Add("." + cur_brace.GetResultName(call_string, true));
                                        }
                                    }
                                }
                                if (word != this._call) call_opened = false;
                                call_string = "";
                                goto Label1;
                            }
                        }
                        #endregion
                        continue;
                    }

                Label1:
                    if (word == " " || word == "\t" || word == "\n" || word == "\r")
                    {
                        this.list_out.Add(word);
                        regexp_end = false;
                    }
                    else if (word == this._var)//var
                    {
                        var_opened = true;
                        var_pause = false;
                        this.list_out.Add(word);
                    }
                    else if (word == this._varPause)//=
                    {
                        var_pause = true;
                        this.list_out.Add(word);
                    }
                    else if (word == this._varContinue)//,
                    {
                        if (!((braket_opened && function_opened == false) || square_braket_opened))
                            var_pause = false;
                        this.list_out.Add(word);
                        regexp_end = false;
                    }
                    else if (word == "function")
                    {
                        function_opened = true;
                        var_pause = false;
                        this.list_out.Add(word);
                    }
                    else if (word == ";")
                    {
                        var_opened = false;
                        var_pause = false;
                        execute_opened = false;
                        this.list_out.Add(word);
                        regexp_end = false;
                    }
                    else if (word == "(")
                    {
                        braket_opened = true;
                        if (function_opened == true)
                        {
                            var_opened = true;
                            parameter_opened = true;
                            function_opened = false;
                            cur_brace = this.bm.Get(i);
                        }
                        this.list_out.Add(word);
                        //if (execute_opened)
                        //{
                        //    tmp = this.getNextNumNoBlankStr(i, 3);
                        //    if (this.getNextNumNoBlankStr(i, 1)=="ArrayStrings_MUI" && Helper.IsDigit(tmp))
                        //    {
                        //        this.list_strings[Convert.ToInt32(tmp)] = this.TranslatePiece(cur_brace,this.list_strings[Convert.ToInt32(tmp)] );
                        //    }
                        //}
                    }
                    else if (word == ")")
                    {
                        braket_opened = false;
                        execute_opened = false;
                        if (parameter_opened)
                        {
                            var_opened = false;
                            var_pause = false;
                        }
                        this.list_out.Add(word);
                    }
                    else if (word == "[")
                    {
                        square_braket_opened = true;
                        this.list_out.Add(word);
                    }
                    else if (word == "]")
                    {
                        square_braket_opened = false;
                        this.list_out.Add(word);
                    }
                    else if (word == this._beginBlock)
                    {
                        //?
                        if (function_opened)
                            var_opened = false;
                        if (parameter_opened)
                        {
                            //函数参数应包括到下一个层级
                            parameter_opened = false;
                        }
                        else
                        {
                            cur_brace = this.bm.Get(i);
                        }
                        this.list_out.Add(word);
                    }
                    else if (word == this._endBlock)
                    {
                        cur_brace = cur_brace.Parent;
                        this.list_out.Add(word);
                        // 在}后加;
                        if (isAddSemicolon(this.getNextNoBlankStr(i))) this.list_out.Add(";");
                    }
                    else if (word == this._call)
                    {
                        //处理引用调用
                        //判断引用(.)后面第一个是否为字母_$
                        if (!function_opened)
                        {
                            if (isVar(this.list_word[i + 1].ToString()))
                            {
                                call_opened = true;
                            }
                            else
                            {
                                this.list_out.Add(word);
                            }
                        }
                        else
                        {
                            this.list_out.Add(word);
                        }
                    }
                    else if (this.isKeyword(word))
                    {
                        //处理关键字
                        if (word == "in") var_opened = false;
                        this.list_out.Add(word);
                        //if (this.IsMoveStr && word == "case" && this.list_word[i+1]!=" ") this.list_out.Add(" ");
                    }
                    else if (this.isExecuteMothod(word))
                    {
                        //处理eval后的字符串
                        //if (this.IsUseSquareBracket && this.IsIncludeSystemObject)
                        //    this.list_out.Add("window[\"" + this.toHex(word) + "\"]");
                        //else
                            this.list_out.Add(word);
                        execute_opened = true;
                    }
                    else if (this.isWindowObject(word))
                    {
                        //window内置对象
                        if (this.IsUseSquareBracket && this.IsIncludeSystemObject)
                        {
                            if (this.IsMoveStr)
                                this.list_out.Add(string.Format("window[{0}]", AddGetStrArrMember(word, true)));
                            else
                                this.list_out.Add("window[\"" + this.toHex(word) + "\"]");
                        }
                        else
                            this.list_out.Add(word);
                    }
                    else if (this.isCommonObject(word))
                    {
                        //处理普通对象
                        this.list_out.Add(word);
                    }
                    else if (Helper.Contains2(this._quotation, word) && regexp_opened == false)
                    {
                        //处理引号（引号前不能为转义字符）
                        sb.Append(word);
                        quote_opened = true;
                        quote_char = word;
                        pre=this.list_word[i - 1];
                    }
                    else if (word == "/")
                    {
                        //正则表达式
                        this.list_out.Add(word);
                        tmp = this.getPreNoBlankStr(i);
                        if (regexp_opened == false && (tmp == ")" || tmp == "]" || Helper.IsVar(tmp) || Helper.IsNumber(tmp))) continue;
                        if (regexp_opened)
                            regexp_opened = false;
                        else
                            regexp_opened = true;
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        this.list_out.Add(word);
                        if (i < word_num - 1)
                        {
                            this.list_out.Add(this.list_word[i + 1]);
                            i = i + 1;
                            continue;
                        }
                    }
                    else if (isVar(word))
                    {
                        if (this.getNextNoBlankStr(i) == ":" && i > 1)
                        {
                            tmp = this.getPreNumNoBlankStr(i, 1);
                            if (tmp == "{" || tmp == ",")
                            {
                                this.list_out.Add(word);
                                continue;
                            }
                        }
                        if (regexp_end && this.getPreNoBlankStr(i) == "/" && new Regex("[gim]?[gim]?[gim]?").IsMatch(word))
                            this.list_out.Add(word);
                        else
                            this.list_out.Add(cur_brace.GetResultName(word, false));
                    }
                    else if (this.isStartWith(this._ignore, i))
                    {
                        //处理忽略词
                        i = i + this._ignore.Length - 1;
                        this.list_out.Add(this._ignore);
                        this._fileNo++;
                    }
                    else
                    {
                        if (this.IsIntToHex && Helper.IsDigit(word))
                        {
                            //将整数用"0x"十六进制形式表示
                            if (this.getNextNoBlankStr(i) != this._call && this.getPreNoBlankStr(i) != this._call && this.getPreNoBlankStr(i) != "$" && this.getPreNumNoBlankStr(i, 2) != "ArrayStrings_MUI" && this.getPreNumNoBlankStr(i, 2) != "ArrayRegexp_MUI")
                                this.list_out.Add(Helper.IntegerToHex(word));
                            else
                                this.list_out.Add(word);
                        }
                        else
                        {
                            this.list_out.Add(word);
                        }
                    }
                }
            }
        }
        private string TranslatePiece(Brace cur_brace, string piece)
        {
            //建立分隔后的字符串数组(分词)
            StringBuilder sb = new StringBuilder();
            List<string> list_piece = new List<string>();
            List<string> list_piece_out = new List<string>();
            for (int i = 0; i < piece.Length; i++)
            {
                if (this._wordDelimiters.IndexOf(piece[i]) == -1)
                {
                    //找不到关键字
                    sb.Append(piece[i]);
                }
                else
                {
                    if (sb.Length > 0) list_piece.Add(sb.ToString());
                    list_piece.Add(piece[i].ToString());
                    sb.Length = 0;
                }
            }
            int piece_num = list_piece.Count;
            bool call_opened = false;
            string call_string = "";
            string word = "";
            string tmp = "";
            for (int i = 0; i < piece_num; i++)
            {
                word = (string)list_piece[i];
                if (call_opened)
                {
                    //单独处理指针引用
                    if (call_string.Length == 0)
                    {
                        call_string += list_piece[i];
                    }
                    else
                    {
                        if (!isVar(word))
                        {
                            tmp = this.getPreNumNoBlankStr(list_piece,i, 3);
                            if (tmp == "window" && (word == "=" || this.getNextNoBlankStr(list_piece,i) == "="))
                            {
                                list_piece_out.Add("." + Brace.Root.GetResultName(call_string, false));
                            }
                            else
                            {
                                if (tmp != "this")
                                {
                                    Variable v = cur_brace.GetVar(tmp, false);
                                    ////如果类定义在引用代码的后面则在此补类变量的所属类领域.VBrace
                                    if (v != null && v.VBrace == null && !string.IsNullOrEmpty(v.ClassName))
                                    {
                                        v.VBrace = cur_brace.GetVBrace(v.ClassName);
                                    }
                                    if (v != null && v.VBrace!=null)
                                        list_piece_out.Add("." + v.VBrace.GetResultName(call_string, true));
                                    else
                                        list_piece_out.Add("." + cur_brace.GetResultName(call_string, true));
                                }
                                else
                                {
                                    list_piece_out.Add("." + cur_brace.GetResultName(call_string, true));
                                }
                            }
                            if (word != this._call) call_opened = false;
                            call_string = "";
                            goto Label0;
                        }
                    }
                    continue;
                }
            Label0:
                if (word == this._call)
                {
                    //处理引用调用
                    //判断引用(.)后面第一个是否为字母_$
                    if (isVar(list_piece[i + 1].ToString()))
                    {
                        call_opened = true;
                    }
                    else
                    {
                        list_piece_out.Add(word);
                    }
                }
                else if (this.isKeyword(word))
                {
                    list_piece_out.Add(word);
                }
                else if (this.isWindowObject(word))
                {
                    //window内置对象			
                    list_piece_out.Add(word);
                }
                else if (this.isCommonObject(word))
                {
                    //处理普通对象
                    list_piece_out.Add(word);
                }
                else if (this.isStartWith(this._ignore, i))
                {
                    //处理忽略词
                    i = i + this._ignore.Length - 1;
                }
                else if (isVar(word))
                {
                    list_piece_out.Add(cur_brace.GetResultName(word, false));
                }
                else
                {
                    list_piece_out.Add(word);
                }
            }
            return string.Join("", list_piece_out.ToArray()); ;
        }

        #region 函数
        // 是否在}后加;
        private bool isAddSemicolon(string str)
        {
            if (isVar(str))
            {
                if (Array.Exists<string>(ECMAScriptPacker.NotAddSemicolonKeywords, delegate(string s)
                {
                    return s == str;
                }))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            return false;

        }
        //是否以某字符串开头
        private bool isStartWith(string str, int index)
        {
            return this.isStartWith(this.list_word, str, index);
        }
        private bool isStartWith(List<string> list,string str, int index)
        {
            if (str == null || str == "") return false;
            int n = list.Count;
            //if(list.Count==1 && list[0].ToString().IndexOf(str)==0) return true;
            ArrayList cc = new ArrayList();
            for (int k = index; k < index + str.Length; k++)
            {
                if (k == n) break;
                cc.Add(list[k].ToString());
            }
            string c = String.Concat((String[])cc.ToArray(typeof(String)));
            if (this._caseSensitive)
            {
                if (str.Length >= list[index].ToString().Length && c.IndexOf(str) == 0)
                {
                    return true;
                }
            }
            else
            {
                if (str.Length >= list[index].ToString().Length && c.ToLower().IndexOf(str.ToLower()) == 0)
                {
                    return true;
                }
            }
            return false;
        }
        //获取下一个非空字符
        private string getNextNoBlankStr(int index)
        {
            return this.getNextNoBlankStr(this.list_word, index);
        }
        private string getNextNoBlankStr(List<string> list,int index)
        {
            for (int k = index + 1; k <= this.word_num; k++)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                    return list[k].ToString();
            }
            return string.Empty;
        }
        //获取下n个非空字符
        private string getNextNumNoBlankStr(int index, int num)
        {
            return this.getNextNumNoBlankStr(this.list_word, index, num);
        }
        private string getNextNumNoBlankStr(List<string> list,int index, int num)
        {
            for (int k = index + 1; k <= this.word_num; k++)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                {
                    num--;
                    if (num == 0) return list[k].ToString();
                }
            }
            return string.Empty;
        }
        //获取上一个非空字符
        private string getPreNoBlankStr(int index)
        {
            return this.getPreNoBlankStr(this.list_word, index);
        }
        private string getPreNoBlankStr(List<string> list,int index)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                    return list[k].ToString();
            }
            return string.Empty;
        }
        //获取上n个非空字符
        private string getPreNumNoBlankStr(int index, int num)
        {
            return this.getPreNumNoBlankStr(this.list_word, index, num);
        }
        private string getPreNumNoBlankStr(List<string> list,int index, int num)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                {
                    num--;
                    if (num == 0) return list[k].ToString();
                }
            }
            return string.Empty;
        }
        //判断紧接的是不是回车
        private int getNextReturnIndex(int index)
        {
            for (int k = index + 1; k < this.word_num; k++)
            {
                if (new Regex(@"[ \t]").IsMatch(this.list_word[k].ToString()) == false)
                {
                    if (this.list_word[k].ToString() == "\r" || this.list_word[k].ToString() == "\n")
                        return k;
                    else return -1;
                }
            }
            return -1;
        }
        //判断前面的是不是回车
        private int getPreReturnIndex(int index)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (new Regex(@"[ \t]").IsMatch(this.list_word[k].ToString()) == false)
                {
                    if (this.list_word[k].ToString() == "\r" || this.list_word[k].ToString() == "\n")
                        return k;
                    else return -1;
                }
            }
            return -1;
        }
        //获取上一个空空格位置
        private int getPreBlankIndex(int index)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (this.list_word[k].ToString() == "\r" || this.list_word[k].ToString() == "\n") continue;
                if (new Regex(@" ").IsMatch(this.list_word[k].ToString()) == false)
                    return -1;
                else
                    return k;
            }
            return -1;
        }
        //获取下一个空空格位置
        private int getNextBlankIndex(int index)
        {
            for (int k = index + 1; k < this.word_num; k++)
            {
                if (this.list_word[k].ToString() == "\r" || this.list_word[k].ToString() == "\n") continue;
                if (new Regex(@" ").IsMatch(this.list_word[k].ToString()) == false)
                    return -1;
                else
                    return k;
            }
            return -1;
        }
        // 是否符合变量命名字首规则
        private bool isVar(string val)
        {
            //if (val == "_" || val == "$") return false;
            return new Regex("^[a-zA-Z_\\$].*$").IsMatch(val);
        }       
        private bool isBlank(string val)
        {
            return new Regex(@"\s").IsMatch(val);
        }
        private bool isFunction(string val)
        {
            if (val.Length == 1) return false;
            if (this._wordDelimiters.IndexOf(val) != -1) return false;
            return new Regex("(^|,)" + val + "($|,)").IsMatch(this._function);
        }
        private bool isKeyword(string val)
        {
            if (val.Length == 1) return false;
            if (this._wordDelimiters.IndexOf(val) != -1) return false;
            return new Regex("(^|,)" + val + "($|,)").IsMatch(this._keywords);
        }
        private bool isWindowObject(string val)
        {
            if (val.Length == 1) return false;
            if (this._wordDelimiters.IndexOf(val) != -1) return false;
            return new Regex("(^|,)" + val + "($|,)").IsMatch(this._windowObject);
        }
        private bool isCommonObject(string val)
        {
            if (val.Length == 1) return false;
            if (this._wordDelimiters.IndexOf(val) != -1) return false;
            return new Regex("(^|,)" + val + "($|,)").IsMatch(this._commonObjects);
        }
        private bool isExecuteMothod(string val)
        {
            if (val.Length == 1) return false;
            if (this._wordDelimiters.IndexOf(val) != -1) return false;
            return new Regex("(^|,)" + val + "($|,)").IsMatch(this._executeMothod);
        }
        #endregion
    }
}
