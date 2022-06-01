using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Specialized;
using System.Web;
using System.IO;
using System.Collections.Generic;

/*
	packer, version 2.0 (beta) (2005/02/01)
	Copyright 2004-2005, Dean Edwards
	Web: http://dean.edwards.name/

	This software is licensed under the CC-GNU LGPL
	Web: http://creativecommons.org/licenses/LGPL/2.1/
    
    Ported to C# by Jesse Hansen, twindagger2k@msn.com
*/

// http://dean.edwards.name/packer/

namespace MUI.Components.ECMAScriptDealer
{
	/// <summary>
	/// Packs a javascript file into a smaller area, removing unnecessary characters from the output.
	/// </summary>
    public class ECMAScriptPacker : IHttpHandler
    {
        /// <summary>
        /// The encoding level to use. See http://dean.edwards.name/packer/usage/ for more info.
        /// </summary>
        public enum PackerEncoding { None = 0, Numeric = 10, Mid = 36, Normal = 62, HighAscii = 95 };

        private PackerEncoding encoding = PackerEncoding.Normal;
        private bool fastDecode = true;
        private bool compressed = false;
        private bool specialChars = false;
        private bool enabled = true;

        public static string IGNORE = "$1";
		//string _wordDelimiters= ",.?!;:\\/<>(){}[]\"'=+-|*%@#$^&";//·Ö¸î×Ö·û
        //string[] _keywords = "break,case,catch,continue,debugger,default,delete,do,else,false,finally,for,function,Function,if,in,instanceof,new,null,return,switch,this,throw,true,try,typeof,var,void,while,with,window,function,Function,alert,escape,execScript,unescape,document,parseInt,parseFloat,ActiveXObject,Array,Boolean,Date,Enumerator,Error,Global,Math,Number,Object,RegExp,String,eval,execScript,var".Split(new char[]{','});//¹Ø¼ü×Ö
        public static string[] NotAddSemicolonKeywords = "else,catch,finally,while".Split(new char[] { ',' });//¹Ø¼ü×Ö
        /// <summary>
	    /// The encoding level for this instance
	    /// </summary>
        public PackerEncoding Encoding
        {
            get { return encoding; }
            set { encoding = value; }
        }

        /// <summary>
        /// Adds a subroutine to the output to speed up decoding
        /// </summary>
        public bool FastDecode
        {
            get { return fastDecode; }
            set { fastDecode = value; }
        }

        /// <summary>
        /// Replaces special characters
        /// </summary>
        public bool SpecialChars
        {
            get { return specialChars; }
            set { specialChars = value; }
        }
        public bool HaveCompressed
        {
            get { return compressed; }
            set { compressed = value; }
        }

        /// <summary>
        /// Packer enabled
        /// </summary>
        public bool Enabled
        {
            get { return enabled; }
            set { enabled = value; }
        }

        public ECMAScriptPacker()
        {
            Encoding = PackerEncoding.Normal;
            FastDecode = true;
            SpecialChars = false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="encoding">The encoding level for this instance</param>
        /// <param name="fastDecode">Adds a subroutine to the output to speed up decoding</param>
        /// <param name="specialChars">Replaces special characters</param>
        public ECMAScriptPacker(PackerEncoding encoding, bool fastDecode, bool specialChars)
        {
            Encoding = encoding;
            FastDecode = fastDecode;
            SpecialChars = specialChars;
        }

        /// <summary>
        /// Packs the script
        /// </summary>
        /// <param name="script">the script to pack</param>
        /// <returns>the packed script</returns>
        public string Pack(string script)
        {
            if (enabled)
            {
                if (!compressed)
                {
                    //script = basicCompression(script);
                    script = Compression(script);
                }
                if (SpecialChars)
                    script = encodeSpecialChars(script);
                if (Encoding != PackerEncoding.None)
                    script = encodeKeywords(script);
            }
            return script;
        }

        //zero encoding - just removal of whitespace and comments
        public string Compression(string script)
        {
            //return this.basicCompression(script);
            return this.basicCompression2(this.basicCompression1(script));
        }
        public string basicCompression(string script)
        {
            script += "\n";
            ParseMaster parser = new ParseMaster();
            // make safe
            parser.EscapeChar = '\\';
            // protect strings
            parser.Add("'[^'\\n\\r]*'", IGNORE);
            parser.Add("\"[^\"\\n\\r]*\"", IGNORE);
            // remove comments
            parser.Add("\\/\\/[^\\n\\r]*[\\n\\r]");
            parser.Add("\\/\\*[^*]*\\*+([^\\/][^*]*\\*+)*\\/");
            // protect regular expressions
            parser.Add("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?)", "$2");
            parser.Add("[^\\w\\$\\/'\"*)\\?:]\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?", IGNORE);
            // remove: ;;; doSomething();
            if (specialChars)
                parser.Add(";;[^\\n\\r]+[\\n\\r]");
            // remove redundant semi-colons
            parser.Add(";+\\s*([};])", "$2");
            // remove white-space
            parser.Add("(\\b|\\$)\\s+(\\b|\\$)", "$2 $3");
            parser.Add("([+\\-])\\s+([+\\-])", "$2 $3");
            parser.Add("\\s+");

            // done

            return parser.Exec(script);
        }
        //ÏÈÈ¥µô×¢ÊÍºÍ¶àÓà×Ö·û´®
        public string basicCompression1(string script)
        {
            script += "\n";
            ParseMaster parser = new ParseMaster();
            // make safe
            parser.EscapeChar = '\\';
            // protect strings
            parser.Add("'[^'\\n\\r]*'", IGNORE);
            parser.Add("\"[^\"\\n\\r]*\"", IGNORE);
            // remove comments
            parser.Add("\\/\\/[^\\n\\r]*[\\n\\r]");
            parser.Add("\\/\\*[^*]*\\*+([^\\/][^*]*\\*+)*\\/");
            // protect regular expressions
            parser.Add("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?)", "$2");
            parser.Add("[^\\w\\$\\/'\"*)\\?:]\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?", IGNORE);
            // remove: ;;; doSomething();
            if (specialChars)
                parser.Add(";;[^\\n\\r]+[\\n\\r]");
            // remove redundant semi-colons
            parser.Add(";+\\s*([};])", "$2");

            return parser.Exec(script);
        }
        //ÔÙÈ¥µô¿Õ¸ñ
        public string basicCompression2(string script)
        {
            script += "\n";
            ParseMaster parser = new ParseMaster();
            // make safe
            parser.EscapeChar = '\\';
            // protect strings
            parser.Add("'[^'\\n\\r]*'", IGNORE);
            parser.Add("\"[^\"\\n\\r]*\"", IGNORE);

            // protect regular expressions
            parser.Add("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?)", "$2");
            parser.Add("[^\\w\\$\\/'\"*)\\?:]\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/g?i?m?", IGNORE);
            
            // remove white-space
            parser.Add("(\\b|\\$)\\s+(\\b|\\$)", "$2 $3");
            parser.Add("([+\\-])\\s+([+\\-])", "$2 $3");
            parser.Add("\\s+");

            // done

            return parser.Exec(script);
        }

        public string CompressionCss(string css)
        {
            css += "\n";
            ParseMaster parser = new ParseMaster();
            // remove comments
            parser.Add("\\/\\*[^*]*\\*+([^\\/][^*]*\\*+)*\\/");
            // remove redundant semi-colons
            parser.Add(";+\\s*([};])", "$2");
            // remove white-space
            parser.Add("(\\b|\\$)\\s+(\\b|\\$)", "$2 $3");
            parser.Add("([+\\-])\\s+([+\\-])", "$2 $3");
            parser.Add(",\\s+", ",");
            parser.Add("[\\n\\r]+");
            // done

            return parser.Exec(css);
        }

        #region added by unest
        //Ìæ»»ÌØÊâ×Ö·û
        public string replaceTSFH(string input)
        {
            return input.Replace("{}", "ReGeXbObJ");
        }
        public string restoreTSFH(string input)
        {
            return input.Replace("ReGeXbObJ", "{}");
        }
        #region ÎÞÓÃ
        public List<string> ListStrings
        {
            get { return _list_strings; }
            set { _list_strings = value; }
        }
        public List<string> ListRegexp
        {
            get { return _list_regexp; }
            set { _list_regexp = value; }
        }
        public bool AddSemicolon
        {
            get { return _addSemicolon; }
            set { _addSemicolon = value; }
        }
        public bool IsRestore
        {
            get { return _isRestore; }
            set { _isRestore = value; }
        }
        public string ArrayStringsName
        {
            get { return _array_strings_name; }
            set { _array_strings_name = value; }
        }
        public string restoreStrings(string input)
        {
            return new Regex("ArrayStrings_MUI\\[(\\d+)\\]").Replace(input, new MatchEvaluator(this.me_restoreStrings));
        }
        public string restoreRegexp(string input)
        {
            return new Regex("ArrayRegexp_MUI\\[(\\d+)\\]").Replace(input, new MatchEvaluator(this.me_restoreRegexp));
        }
        List<string> _list_strings = new List<string>();//ÒÆ¶¯×Ö·û´®Ê±´æ´¢ËùÓÐ×Ö·û´®µÄÊý×é
        List<string> _list_regexp = new List<string>();//ÁÙÊ±´æ´¢ËùÓÐÕýÔò±í´ïÊ½µÄÊý×é
        bool _addSemicolon = false;
        bool _isRestore = true;
        string _array_strings_name = "ArrayStrings_MUI";
        public string addSemicolon(Match match)
        {
            string str = match.Groups[1].Value.ToString();
            if (Helper.IsVar(str))
            {
                if (!Array.Exists<string>(ECMAScriptPacker.NotAddSemicolonKeywords, delegate(string s)
                {
                    return str.StartsWith(s);
                }))
                {
                    return match.Groups[0].Value.ToString().Insert(1, ";");
                }
            }
            return match.Groups[0].Value.ToString();
        }
        //Ìæ»»ÕýÔòÄÚµÄ×ªÒå·ûºÅ
        public string replaceRegexXGFH(string input)
        {
            return input.Replace(@"\/", "ReGeXxG");
        }
        public string restoreRegexXGFH(string input)
        {
            return input.Replace("ReGeXxG", @"\/");
        }
        //Ìæ»»ÒýºÅÄÚµÄ×ªÒå·ûºÅ
        public string replaceStringZYFH(string input)
        {
            //return new Regex("\\\\\"").Replace(input, "ZyQuOtE");
            return input.Replace("\\\"", "ZyQuOtE").Replace(@"\'", "ReGeXdYh");//.Replace(@"http://", "ReGeXdHttp").Replace("&nbsp;", "ReGeXkg") ;
        }
        public string restoreStringZYFH(string input)
        {
            return new Regex("ZyQuOtE").Replace(input, "\\\"").Replace("ReGeXdYh", @"\'");//.Replace("ReGeXdHttp", @"http://").Replace("ReGeXkg", "&nbsp;") ;
        }
        public string replaceStrings(string input)
        {
            //input = new Regex("'[^'\\n\\r]*'").Replace(input, new MatchEvaluator(me_replaceStrings));
            //input = new Regex("\"[^\"\\n\\r]*\"").Replace(input, new MatchEvaluator(me_replaceStrings));
            //return input;
            return new Regex("('[^'\\n\\r]*')|(\"[^\"\\n\\r]*\")").Replace(input, new MatchEvaluator(me_replaceStrings));

        }
        private string me_replaceStrings(Match match)
        {
            this._list_strings.Add(match.Groups[0].Value);
            return string.Format("ArrayStrings_MUI[{0}]", this._list_strings.Count - 1);
        }
        public string replaceRegexp(string input)
        {
            input = new Regex("\\s+(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/[gim]?[gim]?[gim]?)").Replace(input, new MatchEvaluator(me_replaceRegexp1));
            input = new Regex("([^\\w\\$\\/'\"*)\\?:])(\\/[^\\/\\n\\r\\*][^\\/\\n\\r]*\\/[gim]?[gim]?[gim]?)").Replace(input, new MatchEvaluator(me_replaceRegexp2));
            return input;
        }
        private string me_replaceRegexp1(Match match)
        {
            this._list_regexp.Add(match.Groups[1].Value);
            return string.Format("ArrayRegexp_MUI[{0}]", this._list_regexp.Count - 1);
        }
        private string me_replaceRegexp2(Match match)
        {
            this._list_regexp.Add(match.Groups[2].Value);
            return string.Concat(match.Groups[1].Value, "ArrayRegexp_MUI[", this._list_regexp.Count - 1, "]");
        }
        private string me_restoreStrings(Match match)
        {
            return this._list_strings[Convert.ToInt32(match.Groups[1].Value)];
        }
        private string me_restoreRegexp(Match match)
        {
            return this._list_regexp[Convert.ToInt32(match.Groups[1].Value)];
        }
        #endregion
        #endregion

        WordList encodingLookup;
        private string encodeSpecialChars(string script)
        {
            ParseMaster parser = new ParseMaster();
            // replace: $name -> n, $$name -> na
            parser.Add("((\\$+)([a-zA-Z\\$_]+))(\\d*)", 
                new ParseMaster.MatchGroupEvaluator(encodeLocalVars));

            // replace: _name -> _0, double-underscore (__name) is ignored
            Regex regex = new Regex("\\b_[A-Za-z\\d]\\w*");
            
            // build the word list
            encodingLookup = analyze(script, regex, new EncodeMethod(encodePrivate));

            parser.Add("\\b_[A-Za-z\\d]\\w*", new ParseMaster.MatchGroupEvaluator(encodeWithLookup));
            
            script = parser.Exec(script);
            return script;
        }

        private string encodeKeywords(string script)
        {
		    // escape high-ascii values already in the script (i.e. in strings)
		    if (Encoding == PackerEncoding.HighAscii) script = escape95(script);
		    // create the parser
            ParseMaster parser = new ParseMaster();
            EncodeMethod encode = getEncoder(Encoding);

            // for high-ascii, don't encode single character low-ascii
            Regex regex = new Regex(
                    (Encoding == PackerEncoding.HighAscii) ? "\\w\\w+" : "\\w+"
                );
            // build the word list
            encodingLookup = analyze(script, regex, encode);


            // encode
            parser.Add((Encoding == PackerEncoding.HighAscii) ? "\\w\\w+" : "\\w+",
                new ParseMaster.MatchGroupEvaluator(encodeWithLookup));

            // if encoded, wrap the script in a decoding function
            return (script == string.Empty) ? "" : bootStrap(parser.Exec(script), encodingLookup);
        }

        private string bootStrap(string packed, WordList keywords)
        {
		    // packed: the packed script
		    packed = "'" + escape(packed) + "'";

		    // ascii: base for encoding
            int ascii = Math.Min(keywords.Sorted.Count, (int) Encoding);
            if (ascii == 0)
                ascii = 1;

		    // count: number of words contained in the script
		    int count = keywords.Sorted.Count;

		    // keywords: list of words contained in the script
            foreach (object key in keywords.Protected.Keys)
            {
                keywords.Sorted[(int) key] = "";
            }
		    // convert from a string to an array
            StringBuilder sbKeywords = new StringBuilder("'");
            foreach (string word in keywords.Sorted)
                sbKeywords.Append(word + "|");
            sbKeywords.Remove(sbKeywords.Length-1, 1);
            string keywordsout = sbKeywords.ToString() + "'.split('|')";

            string encode;
            string inline = "c";

            switch (Encoding)
            {
                case PackerEncoding.Mid:
                    encode = "function(c){return c.toString(36)}";
                    inline += ".toString(a)";
                    break;
                case PackerEncoding.Normal:
                    encode = "function(c){return(c<a?\"\":e(parseInt(c/a)))+" +
                        "((c=c%a)>35?String.fromCharCode(c+29):c.toString(36))}";
                    inline += ".toString(a)";
                    break;
                case PackerEncoding.HighAscii:
                    encode = "function(c){return(c<a?\"\":e(c/a))+" +
                        "String.fromCharCode(c%a+161)}";
                    inline += ".toString(a)";
                    break;
                default:
                    encode = "function(c){return c}";
                    break;
            }
            
            // decode: code snippet to speed up decoding
            string decode = "";
            if (fastDecode)
            {
                decode = "if(!''.replace(/^/,String)){while(c--)d[e(c)]=k[c]||e(c);k=[function(e){return d[e]}];e=function(){return'\\\\w+'};c=1;}";
                if (Encoding == PackerEncoding.HighAscii)
                    decode = decode.Replace("\\\\w", "[\\xa1-\\xff]");
                else if (Encoding == PackerEncoding.Numeric)
                    decode = decode.Replace("e(c)", inline);
                if (count == 0)
                    decode = decode.Replace("c=1", "c=0");
            }

            // boot function
            string unpack = "function(p,a,c,k,e,d){while(c--)if(k[c])p=p.replace(new RegExp('\\\\b'+e(c)+'\\\\b','g'),k[c]);return p;}";
            Regex r;
            if (fastDecode)
            {
                //insert the decoder
                r = new Regex("\\{");
                unpack = r.Replace(unpack, "{" + decode + ";", 1);
            }

            if (Encoding == PackerEncoding.HighAscii)
            {
                // get rid of the word-boundries for regexp matches
                r = new Regex("'\\\\\\\\b'\\s*\\+|\\+\\s*'\\\\\\\\b'");
                unpack = r.Replace(unpack, "");
            }
            if (Encoding == PackerEncoding.HighAscii || ascii > (int) PackerEncoding.Normal || fastDecode)
            {
                // insert the encode function
                r = new Regex("\\{");
                unpack = r.Replace(unpack, "{e=" + encode + ";", 1);
            }
            else
            {
                r = new Regex("e\\(c\\)");
                unpack = r.Replace(unpack, inline);
            }
            // no need to pack the boot function since i've already done it
            string _params = "" + packed + "," + ascii + "," + count + "," + keywordsout;
            if (fastDecode)
            {
                //insert placeholders for the decoder
                _params += ",0,{}";
            }
            // the whole thing
            return "eval(" + unpack + "(" + _params + "))\n";
        }

        private string escape(string input)
        {
            Regex r = new Regex("([\\\\'])");
            return r.Replace(input, "\\$1");
        }

        private EncodeMethod getEncoder(PackerEncoding encoding)
        {
            switch (encoding)
            {
                case PackerEncoding.Mid:
                    return new EncodeMethod(encode36);
                case PackerEncoding.Normal:
                    return new EncodeMethod(encode62);
                case PackerEncoding.HighAscii:
                    return new EncodeMethod(encode95);
                default:
                    return new EncodeMethod(encode10);
            }
        }

        private string encode10(int code)
        {
            return code.ToString();
        }

        //lookups seemed like the easiest way to do this since 
        // I don't know of an equivalent to .toString(36)
        private static string lookup36 = "0123456789abcdefghijklmnopqrstuvwxyz";

        private string encode36(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(36, i)) % 36;
                encoded = lookup36[digit] + encoded;
                code -= digit * (int) Math.Pow(36, i++);
            } while (code > 0);
            return encoded;
        }

        private static string lookup62 = lookup36 + "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

        private string encode62(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(62, i)) % 62;
                encoded = lookup62[digit] + encoded;
                code -= digit * (int) Math.Pow(62, i++);
            } while (code > 0);
            return encoded;
        }

        private static string lookup95 = "¡¢£¤¥¦§¨©ª«¬­®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

        private string encode95(int code)
        {
            string encoded = "";
            int i = 0;
            do
            {
                int digit = (code / (int) Math.Pow(95, i)) % 95;
                encoded = lookup95[digit] + encoded;
                code -= digit * (int) Math.Pow(95, i++);
            } while (code > 0);
            return encoded;
        }

        private string escape95(string input)
        {
            Regex r = new Regex("[\xa1-\xff]");
            return r.Replace(input, new MatchEvaluator(escape95Eval));
        }

        private string escape95Eval(Match match)
        {
            return "\\x" + ((int) match.Value[0]).ToString("x"); //return hexadecimal value
        }

        private string encodeLocalVars(Match match, int offset)
        {
            int length = match.Groups[offset + 2].Length;
            int start = length - Math.Max(length - match.Groups[offset + 3].Length, 0);
            return match.Groups[offset + 1].Value.Substring(start, length) + 
                match.Groups[offset + 4].Value;
        }

        private string encodeWithLookup(Match match, int offset)
        {
            return (string) encodingLookup.Encoded[match.Groups[offset].Value];
        }

        private delegate string EncodeMethod(int code);

        private string encodePrivate(int code)
        {
            return "_" + code;
        }

        private WordList analyze(string input, Regex regex, EncodeMethod encodeMethod)
        {
            // analyse
            // retreive all words in the script
            MatchCollection all = regex.Matches(input);
            WordList rtrn;
            rtrn.Sorted = new StringCollection(); // list of words sorted by frequency
            rtrn.Protected = new HybridDictionary(); // dictionary of word->encoding
            rtrn.Encoded = new HybridDictionary(); // instances of "protected" words
            if (all.Count > 0)
            {
                StringCollection unsorted = new StringCollection(); // same list, not sorted
                HybridDictionary Protected = new HybridDictionary(); // "protected" words (dictionary of word->"word")
                HybridDictionary values = new HybridDictionary(); // dictionary of charCode->encoding (eg. 256->ff)
                HybridDictionary count = new HybridDictionary(); // word->count
                int i = all.Count, j = 0;
                string word;
                // count the occurrences - used for sorting later
                do
                {
                    word = "$" + all[--i].Value;
                    if (count[word] == null)
                    {
                        count[word] = 0;
                        unsorted.Add(word);
                        // make a dictionary of all of the protected words in this script
                        //  these are words that might be mistaken for encoding
                        Protected["$" + (values[j] = encodeMethod(j))] = j++;
                    }
                    // increment the word counter
                    count[word] = (int) count[word] + 1;
                } while (i > 0);
                /* prepare to sort the word list, first we must protect
                    words that are also used as codes. we assign them a code
                    equivalent to the word itself.
                   e.g. if "do" falls within our encoding range
                        then we store keywords["do"] = "do";
                   this avoids problems when decoding */
                i = unsorted.Count;
                string[] sortedarr = new string[unsorted.Count];
                do
                {
                    word = unsorted[--i];
                    if (Protected[word] != null)
                    {
                        sortedarr[(int) Protected[word]] = word.Substring(1);
                        rtrn.Protected[(int) Protected[word]] = true;
                        count[word] = 0;
                    }
                } while (i > 0);
                string[] unsortedarr = new string[unsorted.Count];
                unsorted.CopyTo(unsortedarr, 0);
                // sort the words by frequency
                Array.Sort(unsortedarr, (IComparer) new CountComparer(count));
                j = 0;
                /*because there are "protected" words in the list
                  we must add the sorted words around them */
                do 
                {
				    if (sortedarr[i] == null) 
                        sortedarr[i] = unsortedarr[j++].Substring(1);
				    rtrn.Encoded[sortedarr[i]] = values[i];
			    } while (++i < unsortedarr.Length);
                rtrn.Sorted.AddRange(sortedarr);
            }
            return rtrn;
        }

        private struct WordList
        {
            public StringCollection Sorted;
            public HybridDictionary Encoded;
            public HybridDictionary Protected;
        }

        private class CountComparer : IComparer
        {
            HybridDictionary count;

            public CountComparer(HybridDictionary count)
            {
                this.count = count;
            }

            #region IComparer Members

            public int Compare(object x, object y)
            {
                return (int) count[y] - (int) count[x];
            }

            #endregion
        }
        #region IHttpHandler Members

        public void ProcessRequest(HttpContext context)
        {
            // try and read settings from config file
            if (System.Configuration.ConfigurationSettings.GetConfig("ecmascriptpacker") != null)
            {
                NameValueCollection cfg = 
                    (NameValueCollection)
                    System.Configuration.ConfigurationSettings.GetConfig("ecmascriptpacker");
                if (cfg["Encoding"] != null)
                {
                    switch(cfg["Encoding"].ToLower())
                    {
                        case "none":
                            Encoding = PackerEncoding.None;
                            break;
                        case "numeric":
                            Encoding = PackerEncoding.Numeric;
                            break;
                        case "mid":
                            Encoding = PackerEncoding.Mid;
                            break;
                        case "normal":
                            Encoding = PackerEncoding.Normal;
                            break;
                        case "highascii":
                        case "high":
                            Encoding = PackerEncoding.HighAscii;
                            break;
                    }
                }
                if (cfg["FastDecode"] != null)
                {
                    if (cfg["FastDecode"].ToLower() == "true")
                        FastDecode = true;
                    else
                        FastDecode = false;
                }
                if (cfg["SpecialChars"] != null)
                {
                    if (cfg["SpecialChars"].ToLower() == "true")
                        SpecialChars = true;
                    else
                        SpecialChars = false;
                }
                if (cfg["Enabled"] != null)
                {
                    if (cfg["Enabled"].ToLower() == "true")
                        Enabled = true;
                    else
                        Enabled = false;
                }
            }
            // try and read settings from URL
            if (context.Request.QueryString["Encoding"] != null)
            {
                switch(context.Request.QueryString["Encoding"].ToLower())
                {
                    case "none":
                        Encoding = PackerEncoding.None;
                        break;
                    case "numeric":
                        Encoding = PackerEncoding.Numeric;
                        break;
                    case "mid":
                        Encoding = PackerEncoding.Mid;
                        break;
                    case "normal":
                        Encoding = PackerEncoding.Normal;
                        break;
                    case "highascii":
                    case "high":
                        Encoding = PackerEncoding.HighAscii;
                        break;
                }
            }
            if (context.Request.QueryString["FastDecode"] != null)
            {
                if (context.Request.QueryString["FastDecode"].ToLower() == "true")
                    FastDecode = true;
                else
                    FastDecode = false;
            }
            if (context.Request.QueryString["SpecialChars"] != null)
            {
                if (context.Request.QueryString["SpecialChars"].ToLower() == "true")
                    SpecialChars = true;
                else
                    SpecialChars = false;
            }
            if (context.Request.QueryString["Enabled"] != null)
            {
                if (context.Request.QueryString["Enabled"].ToLower() == "true")
                    Enabled = true;
                else
                    Enabled = false;
            }
            //handle the request
            TextReader r = new StreamReader(context.Request.PhysicalPath);
            string jscontent = r.ReadToEnd();
            r.Close();
            context.Response.ContentType = "text/javascript";
            context.Response.Output.Write(Pack(jscontent));
        }

        public bool IsReusable
        {
            get 
            { 
                if (System.Configuration.ConfigurationSettings.GetConfig("ecmascriptpacker") != null)
                {
                    NameValueCollection cfg = 
                        (NameValueCollection)
                        System.Configuration.ConfigurationSettings.GetConfig("ecmascriptpacker");
                    if (cfg["IsReusable"] != null)
                        if (cfg["IsReusable"].ToLower() == "true")
                            return true;
                }
                return false;
            }
        }

        #endregion
    }
}
