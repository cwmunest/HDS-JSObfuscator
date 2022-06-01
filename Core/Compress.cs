using System;
using System.Collections.Generic;
using System.Text;
using System.Collections;
using System.Text.RegularExpressions;

namespace MUI.Components.ECMAScriptDealer
{
    public class Compress
    {
        public Compress()
		{
        }
        public Compress(string code)
        {
            this.Code = code;
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
        }
        #endregion
        public string Code { get; set; }
        ECMAScriptPacker sp = new ECMAScriptPacker();

        public string Exec()
        {
            sp.AddSemicolon = false;
            string script = this.Code;
            script = this.sp.replaceTSFH(script);
            script = sp.Compression(script);

            this.SplitScript(script + "\n");
            this.Translate();
            script = string.Join("", this.list_out.ToArray());

            script = this.sp.restoreTSFH(script);
            return script;
        }
        public string ExecCompressCss()
        {
            string css = this.Code;
            css = sp.CompressionCss(css);
            return css;
        }
        string _wordDelimiters = "　 ,.?!;:\\/<>(){}[]\"'\r\n\t=+-|*%@#^&";//分割字符	
        //string[] _flowKeywords = "else,catch,finally,while".Split(new char[] { ',' });//关键字
        string _quotation = "\",'";//引用字符
        string _escape = "\\";//转义字符		

        int word_num = 0;
        List<string> list_word = new List<string>();//用于存储分隔后的字符串数组
        List<string> list_out = new List<string>();//用于存储输出结果的数组

        // 变量初始化
        private void SplitScript(string script)
        {
            //清除变量
            this.list_word.Clear();
            this.list_out.Clear();

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
            string word = "";
            string tmp = "";
            string str = "";
            bool quote_opened = false; //引用标记
            string quote_char = ""; //引用标记类型
            bool regexp_opened = false;//正则表达式

            StringBuilder sb = new StringBuilder();
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
                            this.list_out.Add(str);
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
                    if (Helper.Contains2(this._quotation, word) && regexp_opened == false)
                    {
                        //处理引号（引号前不能为转义字符）
                        sb.Append(word);
                        quote_opened = true;
                        quote_char = word;
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
                    else if (word == "}")
                    {
                        this.list_out.Add(word);
                        // 在}后加;
                        if (isAddSemicolon(this.getNextNoBlankStr(i))) this.list_out.Add(";");
                    }
                    else
                    {
                        this.list_out.Add(word);
                    }
                }
            }
        }

        #region 函数
        // 是否在}后加;
        private bool isAddSemicolon(string str)
        {
            if (Helper.IsVar(str))
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
        //获取下一个非空字符
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
        //获取上一个非空字符
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
        #endregion
    }
}
