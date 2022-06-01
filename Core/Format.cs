using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MUI.Components.ECMAScriptDealer
{
    public struct FormatOptions
    {
        public bool IsRestoreEncodeStr;
        public int TabType;
    }
    public class Format
    {
		public Format()
		{
        }
        public Format(string code)
        {
            this.Code = code;
        }
        public string Code { get; set; }
        ECMAScriptPacker sp = new ECMAScriptPacker();
        bool bRestoreEncodeStr = false;
        int tabType = 0;
        public string Exec()
        {
            this.list_strings.Clear();
            this.list_regexp.Clear();
            string script = this.Code;

            script = sp.replaceTSFH(script);
            script = this.RemoveWhiteSpace(script);

            this.SplitScript(script + "\n");
            this.Translate();
            script = string.Join("", this.list_out.ToArray());

            script = this.sp.restoreTSFH(script);

            return script;
        }
        public void SetFormatOptions(FormatOptions opts)
        {
            this.bRestoreEncodeStr = opts.IsRestoreEncodeStr;
            this.tabType = opts.TabType;

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
            this.list_regexp.Clear();
            this.bm.Clear();
        }
        #endregion
        private string RemoveWhiteSpace(string input)
        {
            // remove white-space
            //input = new Regex("(\\b|\\$)\\s+(\\b|\\$)").Replace(input, "$1 $2");
            //input = new Regex("([+\\-])\\s+([+\\-])").Replace(input, "$1 $2");
            //input = new Regex("\\s+").Replace(input, "");
            return input;
        }
        string _wordDelimiters = "　 ,.?!;:\\/<>(){}[]\"'\r\n\t=+-|*%@#$^&";//分割字符	
        string _lineComment = "//";//单行注释字符		
        string _commentOn = "/*";//多行注释开始		
        string _commentOff = "*/";//多行注释结束块结束字符
        string _quotation = "\",'";//引用字符
        string _escape = "\\";//转义字符		

        int word_num = 0;
        List<string> list_word = new List<string>();//用于存储分隔后的字符串数组
        List<string> list_out = new List<string>();//用于存储输出结果的数组
        List<string> list_strings = new List<string>();//移动字符串时存储所有字符串的数组
        List<string> list_regexp = new List<string>();//临时存储所有正则表达式的数组
        BraceManager bm = new BraceManager();

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
                    sb.Length = 0;
                }
            }
            word_num = this.list_word.Count - 1;

        }
        //获得代码层级映像（即大括号层级），按分割字，分块显示
        private void Translate()
        {
            Brace cur_brace = new Brace();//根级的上级为null
            Brace.Root = cur_brace;
            this.bm.Add(0, cur_brace);

            bool braket_opened = false;
            bool slash_star_comment_opened = false; //多行注释标记
            bool slash_slash_comment_opened = false; //单行注释标记
            bool quote_opened = false; //引用标记
            string quote_char = ""; //引用标记类型
            bool regexp_opened = false;//正则表达式

            string word = "";
            string tmp = "";
            string str = "";
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < word_num; i++)
            {
                word = (string)this.list_word[i];
                if (slash_slash_comment_opened || slash_star_comment_opened)
                {
                    //在单行注释、多行注释里
                    if (slash_star_comment_opened && this.isStartWith(this._commentOff, i))
                    {
                        //处理多行注释结束
                        slash_star_comment_opened = false;
                        this.list_out.Add(this._commentOff);
                        i = i + this.getSkipLength(this._commentOff);
                    }
                    else if (slash_slash_comment_opened && this.getNextReturnIndex(i) != -1)
                    {
                        slash_slash_comment_opened = false;
                        this.list_out.Add(word);
                    }
                    else
                    {
                        this.list_out.Add(word);
                    }
                }
                else if (quote_opened)
                {
                    if (Helper.Contains2(this._quotation, word))
                    {
                        //处理引号里的引号
                        if (quote_char == word)
                        {
                            //是相应的引号
                            //this.list_out.Add(word);
                            //quote_opened = false;
                            //quote_char = "";

                            sb.Append(word);
                            quote_opened = false;
                            quote_char = "";
                            str = sb.ToString();
                            if (this.bRestoreEncodeStr)
                            {
                                str = Helper.RestoreEncodedStr(str);
                            }
                            this.list_out.Add(str);
                            sb.Length = 0;
                        }
                        else
                        {
                            //this.list_out.Add(word);
                            sb.Append(word);
                        }
                    }
                    else if (word == this._escape)
                    {
                        //处理转义字符
                        //this.list_out.Add(word);
                        //if (i < word_num - 1)
                        //{
                        //    this.list_out.Add(this.list_word[i + 1]);
                        //    i = i + 1;
                        //    continue;
                        //}

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
                        //this.list_out.Add(word);
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
                else if (word == "\n")
                {
                    i = this.getNextNoBlankIndex(i);
                    if (i == -1) return;
                    if (this.list_word[i] != "{")
                    {
                        this.list_out.Add(word);
                        this.addIndent(cur_brace, i);
                    }
                    if (this.isReturn(i)) { continue; }
                    i--;
                }
                else if (word == ";")
                {
                    this.list_out.Add(word);
                    i = this.getNextNoBlankIndex(i);
                    if (i == -1) return;
                    if (!this.isReturn(i))
                    {
                        if (!braket_opened && !this.isStartWith(this._lineComment, i) && !this.isStartWith(this._commentOn, i))
                        {
                            this.list_out.Add("\r\n");
                            this.addIndent(cur_brace, i);
                        }
                    }
                    i--;
                }
                else if (word == "{")
                {
                    this.list_out.Add(word);
                    cur_brace = new Brace(cur_brace);
                    this.bm.Add(i, cur_brace);
                    i = this.getNextNoBlankIndex(i);
                    if (i == -1) return;
                    if (!this.isReturn(i))
                    {
                        if (!this.isStartWith(this._lineComment, i) && !this.isStartWith(this._commentOn, i))
                        {
                            this.list_out.Add("\r\n");
                            this.addIndent(cur_brace, i);
                        }
                    }
                    i--;

                }
                else if (word == "}")
                {
                    tmp = this.getPreNoBlankStr(i);
                    if (tmp != ";" && tmp != "{" && tmp != "}" && !this.isReturn(tmp))
                    {
                        this.list_out.Add("\r\n");
                        this.addIndent(cur_brace, i);
                    }
                    if (cur_brace != null)
                        cur_brace = cur_brace.Parent;
                    this.list_out.Add(word);
                    i = this.getNextNoBlankIndex(i);
                    if (i == -1) return;
                    if (!this.isReturn(i))
                    {
                        if (this.list_word[i] != ";" && this.list_word[i] != ")" && this.list_word[i] != "(")
                        {
                            if (!this.isStartWith(this._lineComment, i) && !this.isStartWith(this._commentOn, i))
                            {
                                this.list_out.Add("\r\n");
                                this.addIndent(cur_brace, i);
                            }
                        }
                    }
                    i--;
                }
                else if (word == "(")
                {
                    this.list_out.Add(word);
                    braket_opened = true;
                }
                else if (word == ")")
                {
                    this.list_out.Add(word);
                    braket_opened = false;
                }
                else if (this.isStartWith(this._lineComment, i))
                {
                    //处理单行注释
                    slash_slash_comment_opened = true;
                    this.list_out.Add(_lineComment);
                    i = i + this.getSkipLength(this._lineComment);
                }
                else if (this.isStartWith(this._commentOn, i))
                {
                    //处理多行注释的开始
                    slash_star_comment_opened = true;
                    this.list_out.Add(this._commentOn);
                    i = i + this.getSkipLength(this._commentOn);
                }
                else if (Helper.Contains2(this._quotation, word) && regexp_opened == false)
                {
                    //处理引号（引号前不能为转义字符）
                    //this.list_out.Add(word);
                    quote_opened = true;
                    quote_char = word;
                    sb.Append(word);
                }
                else if (word == "/")
                {
                    //正则表达式
                    this.list_out.Add(word);
                    tmp = this.getPreNoBlankStr(i);
                    if (regexp_opened == false && (tmp == ")" || Helper.IsVar(tmp) || Helper.IsNumber(tmp))) continue;
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
                else
                {
                    this.list_out.Add(word);
                }
            }
        }
        //添加缩进
        private void addIndent(Brace brace,int index)
        {
            if (index < 0) return;
            if (brace == null) return;
            Brace cn = brace.Parent;
            if (this.list_word[index] == "}" && cn!=null) cn = cn.Parent;            
            while (cn != null)
            {
                this.list_out.Add("\t");
                cn = cn.Parent;
            }
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
            ArrayList cc = new ArrayList();
            for (int k = index; k < index + str.Length; k++)
            {
                if (k == n) break;
                cc.Add(list[k].ToString());
            }
            string c = String.Concat((String[])cc.ToArray(typeof(String)));
                if (str.Length >= list[index].ToString().Length && c.IndexOf(str) == 0)
                {
                    return true;
                }
            return false;
        }
        private int getSkipLength(string val)
        {
            int count = 0;
            for (int k = 0; k < val.Length; k++)
            {
                if (this._wordDelimiters.IndexOf(val[k]) >= 0)
                {
                    count++;
                }
            }
            if (count > 0)
            {
                count = count - 1;
            }
            return count;
        }

        //获取下一个非空字符(不包括回车）
        private string getNextNoBlankStr(int index)
        {
            return this.getNextNoBlankStr(this.list_word, index);
        }
        private string getNextNoBlankStr(List<string> list, int index)
        {
            for (int k = index + 1; k <= this.word_num; k++)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                    return list[k].ToString();
            }
            return string.Empty;
        }
        //获取下n个非空字符(不包括回车）
        private string getNextNumNoBlankStr(int index, int num)
        {
            return this.getNextNumNoBlankStr(this.list_word, index, num);
        }
        private string getNextNumNoBlankStr(List<string> list, int index, int num)
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
        //获取上一个非空字符(不包括回车）
        private string getPreNoBlankStr(int index)
        {
            return this.getPreNoBlankStr(this.list_word, index);
        }
        private string getPreNoBlankStr(List<string> list, int index)
        {
            for (int k = index - 1; k >= 0; k--)
            {
                if (new Regex(@"\s").IsMatch(list[k].ToString()) == false)
                    return list[k].ToString();
            }
            return string.Empty;
        }
        //获取上n个非空字符(不包括回车）
        private string getPreNumNoBlankStr(int index, int num)
        {
            return this.getPreNumNoBlankStr(this.list_word, index, num);
        }
        private string getPreNumNoBlankStr(List<string> list, int index, int num)
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
        //获取下一个非空位置
        private int getNextNoBlankIndex(int index)
        {
            for (int k = index + 1; k < this.word_num; k++)
            {
                if (new Regex(@"[ \t]").IsMatch(this.list_word[k])) continue;
                else
                    return k;
            }
            return -1;
        }
        //是否是回车
        private bool isReturn(int index)
        {
            return index >= 0 && (this.list_word[index] == "\n" || (this.list_word[index] == "\r"&& this.list_word[index+1] == "\n"));
        }
        //是否是空格
        private bool isReturn(string str)
        {
            return (str == "\r" || str == "\n");
        }
    }
}
