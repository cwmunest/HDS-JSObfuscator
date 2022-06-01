using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;

namespace HDS.JSObfuscator
{
    public enum NoticeMsgType
    {
        System
    }
    public class IFNotifier
    {
        //[DllImport("winmm.dll")]
        //private static extern long sndPlaySound(string lpszSoundName, long uFlags);
        //public void PlaySound(NoticeMsgType type)
        //{
        //    string path = string.Format("{0}\\data\\sound\\{1}.wav", Application.StartupPath, type.ToString());
        //    sndPlaySound(path, 1);
        //}

         TaskbarNotifier[] _notifiers=new TaskbarNotifier[1];
         public TaskbarNotifier Current=null;
         Bitmap[] sks = new Bitmap[1];

        public IFNotifier(Form fm,int idx)
        {
            //try
            //{
            sks[0] = global::HDS.JSObfuscator.Properties.Resources.skin_sys;//new Bitmap(GetType(),  "Resources.notifier_skin.skin_sys.bmp");
                //sks[1] = global::WebChat.Properties.Resources.skin_msg;// new Bitmap(GetType(), "Resources.notifier_skin.skin_msg.bmp");
                //sks[2] = global::WebChat.Properties.Resources.skin_req; //new Bitmap(GetType(), "Resources.notifier_skin.skin_req.bmp");
          
            TaskbarNotifier tbn;

            tbn = _notifiers[0] = new TaskbarNotifier(fm);
            tbn.Index = idx;
            tbn.SetBackgroundBitmap(sks[0], Color.FromArgb(255, 0, 255));
            tbn.SetCloseBitmap(global::HDS.JSObfuscator.Properties.Resources.skin_close, Color.FromArgb(255, 0, 255), new Point(220, 8));
            tbn.TitleRectangle = new Rectangle(10, 9, 250, 20);
            tbn.ContentRectangle = new Rectangle(8, 20, 220, 90);
            tbn.CloseClickable = true;//关闭是否响应单击
            tbn.ContentClickable = true;//内容是否响应单击
            tbn.onclick = new OnBarClick(OnSysClick);

            //tbn = _notifiers[1] = new TaskbarNotifier();
            //tbn.Index = 0;
            //tbn.SetBackgroundBitmap(sks[1], Color.FromArgb(255, 0, 255));
            //tbn.SetCloseBitmap(global::WebChat.Properties.Resources.skin_close, Color.FromArgb(255, 0, 255), new Point(220, 8));
            //tbn.TitleRectangle = new Rectangle(10, 9, 250, 20);
            //tbn.ContentRectangle = new Rectangle(8, 20, 220, 90);
            //tbn.CloseClickable = true;//关闭是否响应单击
            //tbn.ContentClickable = true;//内容是否响应单击
            //tbn.onclick = new OnBarClick(OnMsgClick);

            //tbn = _notifiers[2] = new TaskbarNotifier();
            //tbn.Index = 0;
            //tbn.SetBackgroundBitmap(sks[2], Color.FromArgb(255, 0, 255));
            //tbn.SetCloseBitmap(global::WebChat.Properties.Resources.skin_close, Color.FromArgb(255, 0, 255), new Point(220, 8));
            //tbn.TitleRectangle = new Rectangle(10, 9, 250, 20);
            //tbn.ContentRectangle = new Rectangle(8, 20, 220, 90);
            //tbn.CloseClickable = true;//关闭是否响应单击
            //tbn.ContentClickable = true;//内容是否响应单击
            //tbn.onclick = new OnBarClick(OnReqClick);
            //}
            //catch (Exception ex)
            //{
            //    string s = ex.ToString();
            //}
        }
        public TaskbarNotifier Alloc(string key)
        {
            switch (key)
            {
                case "sys":
                    return this._notifiers[0];
                //case "msg":
                //    return this._notifiers[1];
                //case "req":
                //    return this._notifiers[2];
            }
            return null;
        }
        void  OnSysClick(TaskbarNotifier tnf)
        {
            tnf.Hide();

            if (tnf.Arg != null && tnf.Arg.StartsWith("http://"))
            {
                try
                {
                    System.Diagnostics.Process.Start(tnf.Arg);
                }
                catch //(Exception ex)
                {
                    //Log.Add(ex.ToString());
                }
            }
        }

        //系统消息
        public void NoticeSys(string content)
        {
            NoticeSys(null, null, content);
        }
        public void NoticeSys(string title,string content)
        {
            NoticeSys(null, title, content);
        }
        public void NoticeSys(string url, string title, string body)
        {
            TaskbarNotifier tnf = this.Alloc("sys");
            if (tnf == null)
                return;
            string content = body;// DataCheck.NoHTML(body);
            tnf.Arg = url;
            tnf.Show(title ?? "", content, 80, 10000, 500);
            Current = tnf;
        }
        public void NoticeSys_Event(OnBarClick cli, string title, string body)
        {
            TaskbarNotifier tnf = this.Alloc("sys");
            if (tnf == null)
                return;
            string content = body;//DataCheck.NoHTML(body);
            tnf.onclick = cli;
            tnf.Show(title ?? "", content, 80, 10000, 500);
            Current = tnf;
        }
        ////来消息
        //public void NoticeMsg(MsgObject obj, string title, string content)
        //{
        //    TaskbarNotifier tnf = Alloc("msg");
        //    if (tnf == null)
        //        return;
        //    //content = DataCheck.NoHTML(content);
        //    tnf.Type = obj.Type.ToString().ToLower();
        //    tnf.Key = obj.Key;
        //    tnf.Show(title, content, 80, 10000, 500);
        //    Current = tnf;
        //}
        ////请求了聊天
        //public void NoticeReq(MsgObject obj, string title, string content)
        //{
        //    TaskbarNotifier tnf = Alloc("req");
        //    if (tnf == null)
        //        return;
        //    //content = DataCheck.NoHTML(content);
        //    tnf.Type = obj.Type.ToString().ToLower();
        //    tnf.Key = obj.Key;
        //    tnf.Show(title, content, 80, 10000, 500);
        //    Current = tnf;
        //}

    }
}
