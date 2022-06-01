using System;
using System.Collections.Generic;
using System.Text;

namespace MUI.Components.ECMAScriptDealer
{
    public class ScriptDealer
    {
        //排版
        public static string Format(string script)
        {
            Format fmt = new Format(script);
            script = fmt.Exec();
            fmt.Dispose();
            return script;
        }
        public static string Format(string script, FormatOptions opts)
        {
            Format fmt = new Format(script);
            fmt.SetFormatOptions(opts);
            script = fmt.Exec();
            fmt.Dispose();
            return script;
        }
        //普通压缩
        public static string BasicCompression(string script)
        {
            //ECMAScriptPacker sp = new ECMAScriptPacker();
            //sp.AddSemicolon = true;
            Compress cp = new Compress(script);
            script = cp.Exec();
            cp.Dispose();
            return script;
        }
        //普通压缩CSS文件
        public static string CompressionCss(string css)
        {
            //ECMAScriptPacker sp = new ECMAScriptPacker();
            //sp.AddSemicolon = true;
            Compress cp = new Compress(css);
            css = cp.ExecCompressCss();
            cp.Dispose();
            return css;
        }
        //混淆
        public static string Confuse(string script,ConfusionOptions opts)
        {
            Confusion cf = new Confusion(script, opts);
            //cf.IgnoreSentence.Add("qazwsxedcasd", "this._super()");
            script=cf.Exec();
            cf.Dispose();
            return script;
        }
        //加密
        public static string Encrypt(string script)
        {
            Compress cp = new Compress(script);
            script = cp.Exec();
            cp.Dispose();
            ECMAScriptPacker sp = new ECMAScriptPacker();
            //sp.AddSemicolon = true;
            sp.HaveCompressed = true;
            return sp.Pack(script);
        }
        //混淆加密
        public static string ConfuseEncrypt(string script, ConfusionOptions opts)
        {
            Confusion cf = new Confusion(script, opts);
            script = cf.Exec(true);
            cf.Dispose();
            return script;
        }
    }
}
