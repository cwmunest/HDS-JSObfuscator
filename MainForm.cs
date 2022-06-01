using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Collections;
using System.Text.RegularExpressions;
using System.Web;
using System.Web.Services;
using MUI.Components.ECMAScriptDealer;
//2014-11-27 key:unestkey->cwmunest 83966983->26551105
namespace HDS.JSObfuscator
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }
        public static string Version = "2.14";//当前大版本号
        System.Text.Encoding encoding = System.Text.Encoding.UTF8;
        bool authorized = false;
        bool isValid = false;
        int maxFileSize = 10000;//非法文件大小限制
        int ProbationDay = 10;//试用期天数
        bool isOpen = false;
        bool isProhibit = false;
        string openFileName = "";
        string fileSplitString = "#@$%&*?#";
        //string wsURL="http://localhost:8080/WebService/hdjso.asmx";
        string wsURL = "http://www.moralsoft.com/WebService/hdjso.asmx";

        bool IsUsedForever = true;//是否永久使用
        string UserName = "";

        HDS.JSObfuscator.IFNotifier Notifier_Update = null;
        HDS.JSObfuscator.IFNotifier Notifier_Advertising = null;

        private void addIgnoreSentence(Confusion cf)
        {
            cf.IgnoreSentence.Add("qazwsxedcasd", "this._super()");
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            Notifier_Update = new IFNotifier(this, 0);
            Notifier_Advertising = new IFNotifier(this, 1);
            //ShowNotifier(null, null, null);

            int ret = CheckValid();
            if (ret == 1)
            {
                gb1.Visible = false;
                gb2.Visible = false;
                lblthankyouforuser.Text = "您已注册，谢谢使用！";
                lblthankyouforuser.Visible = true;
                this.isValid = true;
                this.authorized = true;
            }
            else if (ret == 0)
            {
                this.isValid = false;
                maxFileSize = 100000;//试用期100K
                //this.btnFormat.ForeColor = Color.Green;
                //this.btnFormatFile.ForeColor = Color.Green;
                //this.btnTrimCode.ForeColor = Color.Green;
                //this.btnTrim.ForeColor = Color.Green;
                //this.btnEncryptCode.ForeColor = Color.Green;
                //this.btnEncrypt.ForeColor = Color.Green;
                //this.btnCopy.ForeColor = Color.Green;
                //this.btnCopyFile.ForeColor = Color.Green;
            }
            else if (ret == -1)
            {
                this.isValid = false;
                //this.btnFormat.ForeColor = Color.Green;
                //this.btnFormatFile.ForeColor = Color.Green;
                //this.btnTrimCode.ForeColor = Color.Green;
                //this.btnTrim.ForeColor = Color.Green;
                //this.btnEncryptCode.ForeColor = Color.Green;
                //this.btnEncrypt.ForeColor = Color.Green;
                //this.btnCopy.ForeColor = Color.Green;
                //this.btnCopyFile.ForeColor = Color.Green;
            }
            else if (ret == -2)
            {
                this.isValid = false;
                //this.btnFormat.Enabled = false;
                //this.btnFormatFile.Enabled = false;
                //this.btnTrimCode.Enabled = false;
                //this.btnTrim.Enabled = false;
                //this.btnEncryptCode.Enabled = false;
                //this.btnEncrypt.Enabled = false;
                //this.btnCopy.Enabled = false;
                //this.btnCopyFile.Enabled = false;
                //this.btnConfuseCode.Enabled = false;
                //this.btnConfuseFiles.Enabled = false;
                //this.btnConfuseEncryptCode.Enabled = false;
                //this.btnConfuseEncrypt.Enabled = false;
                isProhibit = true;
            }
            this.cmbTabType.SelectedIndex = 0;
            this.cbbEncoding.SelectedIndex = 2;

        }

        private void OK()
        {
            //return;
            gb1.Visible = false;
            gb2.Visible = false;
            this.groupBox2.Visible = false;
            this.groupBox8.Visible = true;
            //lblthankyouforuser.Text = "免费使用，谢谢使用！";
            //lblthankyouforuser.Visible = true;
            this.isValid = true;
            this.isProhibit = false;
            this.authorized = true;
        }
        #region 检验合法性
        private bool CheckFileSize(string code)
        {
            return CheckFileSize(code, this.maxFileSize);
        }
        private bool CheckFileSize(string code,bool bcheck)
        {
            return CheckFileSize(code, this.maxFileSize,bcheck);
        }
        private bool CheckFileSize(string code, int len)
        {
            return CheckFileSize(code, len, true);
        }
        private bool CheckFileSize(string code, int len,bool bcheck)
        {
            if (bcheck && this.isValid == false)
            {
                if (code.Length > len)
                {
                    MessageBox.Show("超过" + (len / 1000).ToString() + "KB的文件请在注册后使用！", "提示");
                    return false;
                }
                return true;
            }
            else
                return true;
        }
        private int CheckValid()
        {
            //return 1;
            //string fileName = "key.txt";
            //if (File.Exists(fileName) == false)
            //    return false;
            //System.IO.StreamReader sr = new StreamReader(fileName, Encoding.Default);
            //string content = sr.ReadToEnd();
            //sr.Close();
            //ArrayList al = Helper.GetAllMACAddresses();
            //foreach (string mac in al)
            //{
            //    if (content == Helper.GetHash2(mac))
            //    {
            //        if (content == "3at94f26are17323d5e8ak9d2d5hj6k2")
            //        {
            //            this.txtSrcPath.Text = @"";
            //            this.txtDesPath.Text = @"";
            //        }
            //        return true;
            //    }
            //}
            //return false;
            try
            {
                int ret = -1;
                string regCode = "";
                string createdTime = "";
                string fileName = getFileName();
                string guid = "";
                if (File.Exists(fileName) == false)
                {
                    createdTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    string encCreatedTime = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "26551105");
                    string encProbation = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(ProbationDay.ToString(), "cwmunest");
                    FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                    guid = System.Guid.NewGuid().ToString();
                    writer.Write(encCreatedTime + "\n" + encProbation + "\n" + guid);
                    writer.Flush();
                    writer.Close();
                    File.SetAttributes(fileName, FileAttributes.Hidden | FileAttributes.System);
                    //File.SetAttributes(fileName, FileAttributes.Normal | FileAttributes.Hidden);
                    //return 0;
                    //return -1;
                    ret = 0;
                }
                else
                {
                    System.IO.StreamReader sr = new StreamReader(fileName, Encoding.Default);
                    string content = sr.ReadToEnd();
                    sr.Close();
                    string[] arr = content.Split(new char[] { '\n' });
                    //string createdTime = arr[0];
                    createdTime = MUI.Components.Encryption.EncryptDecrypt.DecryptDES(arr[0], "26551105");
                    regCode = MUI.Components.Encryption.EncryptDecrypt.DecryptDES(arr[1], "cwmunest");
                    if (arr.Length < 3)
                    {
                       // ret = -1;
                        guid = System.Guid.NewGuid().ToString();
                    }
                    else
                    {
                        guid = arr[2];
                    }

                    if (arr.Length < 2 || arr[1] == regCode || regCode == "Invalid")
                    {
                        //非法数据
                        ret = -1;
                    }
                    else if (regCode == "Prohibition")
                    {
                        ret = -2;
                    }
                    else if (MUI.Components.ECMAScriptDealer.Helper.CheckValue(regCode, 'T'))
                    {
                        //加密日期 == 组件创建日期
                        if (!MUI.Components.ECMAScriptDealer.Helper.CheckValue(createdTime, 'T'))
                        {
                            ret = -1;//非法日期值
                        }
                        if (regCode.Equals(createdTime))
                        {
                            ret = 1;
                        }

                    }
                    else if (MUI.Components.ECMAScriptDealer.Helper.IsDigit(regCode))
                    {
                        //判断是否在试用期内
                        if (!MUI.Components.ECMAScriptDealer.Helper.CheckValue(createdTime, 'T'))
                        {
                            ret = -1;//非法日期值
                        }
                        if (DateTime.Now.CompareTo(Convert.ToDateTime(createdTime).AddDays(Convert.ToInt32(regCode))) < 0)
                        {
                            ret = 0;//在试用期内0
                        }
                        else
                        {
                            ret = -1;
                        }
                    }
                    else
                    {
                        ret = -1;
                    }
                    //过期则置为非法
                    if (ret == -1 && regCode != "Invalid")
                    {
                        string encCode = MUI.Components.Encryption.EncryptDecrypt.EncryptDES("Invalid", "cwmunest");
                        File.SetAttributes(fileName, FileAttributes.Normal);
                        FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                        writer.Write(arr[0] + "\n" + encCode + "\n" + guid);
                        writer.Flush();
                        writer.Close();
                        File.SetAttributes(fileName, FileAttributes.Hidden | FileAttributes.System);
                        regCode = "Invalid";
                    }
                }
                //远程检验合法性
                if (ret == 0) regCode = "Probation";
                else if (ret == -1) regCode = "Invalid";
                else if (ret == -2) regCode = "Prohibition";
                //if (ret != 1) 
                    RomoteValidate(createdTime, regCode,guid);
                return ret;
            }
            catch
            {
                this.isValid = true;
                OK();
                return -2;
            }
        }
        private string getFileName()
        {
            string fileName = string.Empty;
            try
            {
                if (Directory.Exists(@"c:\windows\system32"))
                    fileName = @"c:\windows\system32\msosys32.tbl";
                else if (Directory.Exists(@"c:\winxp\system32"))
                    fileName = @"c:\winxp\system32\msosys32.tbl";
                else if (Directory.Exists(@"C:\Program Files\Common Files\System"))
                    fileName = @"C:\Program Files\Common Files\System\msosys32.tbl";
                else if (Directory.Exists(@"C:\Program Files\Common Files\Microsoft Shared"))
                    fileName = @"C:\Program Files\Common Files\Microsoft Shared\msosys32.tbl";
                else
                    fileName = @"c:\msosys32.tbl";
            }
            catch
            {
                fileName = @"C:\Program Files\Common Files\System\msosys32.tbl";
            }
            return fileName;
        }
        //远程检验合法性
        //cd-试用日期，dc-解码
        private void RomoteValidate(string createdTime, string code,string guid)
        {
            try
            {
                //创建 Web Service 访问类,实现与 Web Service 通讯。
                HDS.JSObfuscator.HDSWebService.HDJSORegister rs = new HDS.JSObfuscator.HDSWebService.HDJSORegister();
                //rs.Url = "http://localhost:8080/WebService/hdjso.asmx";
                rs.Url = wsURL;
                rs.Timeout = 60 * 1000;
                rs.CheckValid2Completed += new HDS.JSObfuscator.HDSWebService.CheckValid2CompletedEventHandler(delegate(object sender, HDS.JSObfuscator.HDSWebService.CheckValid2CompletedEventArgs e)
                {
                    try
                    {
                        string ret = e.Result;
                        string[] arrRet = ret.Split(new char[] { '|' });
                        string regCode = arrRet[0];
                        if (!string.IsNullOrEmpty(regCode))
                        {
                            if (regCode == "1" || regCode == "Valid" || regCode.ToUpper() == "OK")
                            {
                                this.isValid = true;
                            }
                            else
                            {
                                if (regCode == "Invalid2") regCode = "Invalid";
                                string fileName = getFileName();
                                if (File.Exists(fileName))
                                {
                                    createdTime = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "26551105");
                                    string encCode = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(regCode == "xxx" ? "Prohibition" : regCode, "cwmunest");
                                    File.SetAttributes(fileName, FileAttributes.Normal);
                                    FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                                    StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                                    writer.Write(createdTime + "\n" + encCode + "\n" + guid);
                                    writer.Flush();
                                    writer.Close();
                                    File.SetAttributes(fileName, FileAttributes.Hidden | FileAttributes.System);
                                }
                                if (regCode == "xxx" || regCode == "Prohibition") this.isProhibit = true;
                                if (regCode == "Invalid") this.isValid = false;
                            }
                        }
                        ShowNotifier(arrRet.Length > 1 ? arrRet[1] : null, arrRet.Length > 2 ? arrRet[2] : null, arrRet.Length > 3 ? arrRet[3] : null);
                    }
                    catch
                    {
                        this.isValid = true;
                        OK();
                    }
                });
                string ip = "nottoknow";
                if (string.IsNullOrEmpty(guid))
                {
                    guid = MUI.Components.ECMAScriptDealer.Helper.GetFirstOfMACAddress();
                    ip = MUI.Components.ECMAScriptDealer.Helper.GetLocalIP();
                }
                if (IsUsedForever)
                    rs.CheckValid2Async(UserName, createdTime, code, ip, null);
                else
                    rs.CheckValid2Async(guid, createdTime, code, "0", null);

            }
            catch// (Exception e1)
            {
                this.isValid = true;
                OK();
                //MessageBox.Show("网络连接失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }
        
        #endregion

        #region 获取选项设置值
        private void GetCurEncoding()
        {
            if (this.cbbEncoding.Text.ToLower() == "default")
                this.encoding = System.Text.Encoding.Default;
            else if (this.cbbEncoding.Text.ToLower() == "utf8")
                this.encoding = System.Text.Encoding.UTF8;
            else if (this.cbbEncoding.Text.ToLower() == "unicode")
                this.encoding = System.Text.Encoding.Unicode;
            else if (this.cbbEncoding.Text.ToLower() == "ansi")
                this.encoding = System.Text.Encoding.GetEncoding("ANSI");
            else if (this.cbbEncoding.Text.ToLower() == "utf7")
                this.encoding = System.Text.Encoding.UTF7;
            else if (this.cbbEncoding.Text.ToLower() == "ascii")
                this.encoding = System.Text.Encoding.ASCII;
            else
                this.encoding = System.Text.Encoding.GetEncoding(this.cbbEncoding.Text);
        }
        private ConfusionOptions ReadOptions()
        {
            ConfusionOptions opts = new ConfusionOptions();
            opts.IsMoveStr = cbMoveStr.Checked;
            opts.IsEncodeStr = cbEncodeStr.Checked;
            opts.IsEncodeOnlyChineseStr = this.chkOnlyCH.Checked;
            opts.IsIntToHex = cbIntToHex.Checked;
            opts.IsConfuseVariable = cbConfuseVariable.Checked;
            opts.IsConfuseFunctionName = cbConfuseFunctionName.Checked;
            opts.IsConfuseClassMember = cbConfuseClassMember.Checked;
            opts.NoVariable = txtNoVariable.Text;
            opts.NoClassMember = txtNoClassMember.Text;

            opts.IsUseSquareBracket = cbUseSquareBracket.Checked;
            opts.IsIncludeSystemObject = cbIncludeSystemObject.Checked;

            opts.IsConfuseRootGlobalVar = true;
            opts.IsConfusePre_ = this.cbConfusePre_.Checked;
            opts.IsConfusePreS = cbConfusePreS.Checked;
            opts.IsVariablePre = cbVariablePre.Checked;
            opts.VariablePre = txtVariablePre.Text;
            opts.Seed = txtSeed.Text;
            opts.VarLength = txtVarLength.Text.Trim().Equals(string.Empty) ? 0 : Convert.ToInt16(txtVarLength.Text.Trim());
            return opts;
        }

        private bool CheckSet()
        {
            if (cbVariablePre.Checked && this.txtVariablePre.Text.Trim() != "" && new Regex("^[a-zA-Z_\\$].*$").IsMatch(txtVariablePre.Text) == false)
            {
                MessageBox.Show("非法变量名前缀！", "提示");
                txtVariablePre.Focus();
                return false;
            }
            if (Helper.IsDigit(this.txtVarLength.Text) == false)
            {
                MessageBox.Show("非法变量长度！", "提示");
                txtVarLength.Focus();
                return false;
            }
            else if (!txtSeed.Text.Trim().Equals(string.Empty) && Convert.ToInt32(this.txtVarLength.Text) < 3)
            {
                MessageBox.Show("当使用混淆种子时变量位数至少3位！", "提示");
                txtVarLength.Focus();
                return false;
            }
            return true;
        }
        #endregion

        #region 处理脚本代码
        private void btnPaste_Click(object sender, EventArgs e)
        {
            txtInitCode.Text = (string)Clipboard.GetDataObject().GetData(typeof(string));
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
        }

        private void btnReadFile_Click(object sender, EventArgs e)
        {
            DialogResult r = ofdSource.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                Stream s = ofdSource.OpenFile();
                TextReader rd = new StreamReader(s, this.encoding);
                string content = rd.ReadToEnd();
                rd.Close();
                s.Close();
                Regex regex = new Regex("([^\r])(\n+)");
                txtInitCode.Text = regex.Replace(content, new MatchEvaluator(changeUnixLineEndings));
            }
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
        }
        private string changeUnixLineEndings(Match match)
        {
            return match.Value.Replace("\n", "\r\n");
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtResultCode.Text = txtInitCode.Text = string.Empty;
            btnCopy.Enabled = false;
            btnSaveFile.Enabled = false;
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }

        private void btnFormat_Click(object sender, EventArgs e)
        {
            //if (this.isProhibit) return;
            if (txtInitCode.Text.Trim() == "") return;
            //if (!this.isValid)
            //{
            //    if (this.CheckFileSize(txtInitCode.Text,false) == false) return;
            //}
            FormatOptions fo = new FormatOptions();
            fo.IsRestoreEncodeStr = this.chkRestoreEncodeStr.Checked;
            fo.TabType = this.cmbTabType.SelectedIndex;
            txtResultCode.Text = ScriptDealer.Format(txtInitCode.Text,fo);
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
            this.link2.Text = "结果代码：" + txtResultCode.Text.Length + "字";
            lblRatio.Text = "压缩比：" + Math.Round(txtResultCode.Text.Length * 1.00 / this.txtInitCode.Text.Length, 2);
            btnCopy.Enabled = true;
            btnSaveFile.Enabled = true;
        }

        private void btnTrimCode_Click(object sender, EventArgs e)
        {
            //if (this.isProhibit) return;
            if (txtInitCode.Text.Trim() == "") return;
            //if (!this.isValid)
            //{
            //    if (this.CheckFileSize(txtInitCode.Text, false) == false) return;
            //}
            if (txtInitCode.Text.IndexOf("}\r\n.") != -1)
                txtResultCode.Text = ScriptDealer.CompressionCss(txtInitCode.Text);
            else
                txtResultCode.Text = ScriptDealer.BasicCompression(txtInitCode.Text);
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
            this.link2.Text = "结果代码：" + txtResultCode.Text.Length + "字";
            lblRatio.Text = "压缩比：" + Math.Round(txtResultCode.Text.Length * 1.00 / this.txtInitCode.Text.Length, 2);
            btnCopy.Enabled = true;
            btnSaveFile.Enabled = true;
        }

        private void btnConfuseCode_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            if (txtInitCode.Text.Trim() == "") return;
            if (!this.isValid)
            {
                if (this.CheckFileSize(txtInitCode.Text) == false) return;
            }
            if (!this.CheckSet()) { tabControl1.SelectedIndex = 0; return; }
            txtResultCode.Text = ScriptDealer.Confuse(this.txtInitCode.Text, ReadOptions());
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
            this.link2.Text = "结果代码：" + txtResultCode.Text.Length + "字";
            lblRatio.Text = "压缩比：" + Math.Round(txtResultCode.Text.Length * 1.00 / this.txtInitCode.Text.Length, 2);
            btnCopy.Enabled = true;
            btnSaveFile.Enabled = true;
        }

        private void btnEncryptCode_Click(object sender, EventArgs e)
        {
            //if (this.isProhibit) return;
            if (txtInitCode.Text.Trim() == "") return;
            //if (!this.isValid)
            //{
            //    if (this.CheckFileSize(txtInitCode.Text,false) == false) return;
            //}
            txtResultCode.Text = ScriptDealer.Encrypt(txtInitCode.Text);
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
            this.link2.Text = "结果代码：" + txtResultCode.Text.Length + "字";
            lblRatio.Text = "压缩比：" + Math.Round(txtResultCode.Text.Length * 1.00 / this.txtInitCode.Text.Length, 2);
            btnCopy.Enabled = true;
            btnSaveFile.Enabled = true;
        }

        private void btnConfuseEncryptCode_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            if (txtInitCode.Text.Trim() == "") return;
            if (!this.isValid)
            {
                if (this.CheckFileSize(txtInitCode.Text) == false) return;
            }
            if (!this.CheckSet()) { tabControl1.SelectedIndex = 0; return; }
            txtResultCode.Text = ScriptDealer.ConfuseEncrypt(this.txtInitCode.Text, this.ReadOptions());
            this.link1.Text = "初始代码：" + txtInitCode.Text.Length + "字";
            this.link2.Text = "结果代码：" + txtResultCode.Text.Length + "字";
            lblRatio.Text = "压缩比：" + Math.Round(txtResultCode.Text.Length * 1.00 / this.txtInitCode.Text.Length, 2);
            btnCopy.Enabled = true;
            btnSaveFile.Enabled = true;
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (txtResultCode.Text.Trim() != "")
                Clipboard.SetDataObject(txtResultCode.Text, true);
        }

        private void btnSaveFile_Click(object sender, EventArgs e)
        {
            DialogResult r = sfdResult.ShowDialog(this);
            if (r == DialogResult.OK)
            {
                Stream s = sfdResult.OpenFile();
                TextWriter rd = new StreamWriter(s, this.encoding);
                rd.Write(txtResultCode.Text);
                rd.Close();
                s.Close();
            }
        }
        #endregion

        #region 批量文件处理
        private void cbbEncoding_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.GetCurEncoding();
        }

        private void btnOpenSrcPath_Click(object sender, EventArgs e)
        {
            DialogResult result = fbDialog.ShowDialog();
            if (result == DialogResult.OK)
                this.txtSrcPath.Text = fbDialog.SelectedPath;
        }

        private void btnOpenDesPath_Click(object sender, EventArgs e)
        {
            DialogResult result = fbDialog.ShowDialog();
            if (result == DialogResult.OK)
                this.txtDesPath.Text = fbDialog.SelectedPath;
        }

        private void btnReadFileNameList_Click(object sender, EventArgs e)
        {
            if (this.txtSrcPath.Text == "") return;
            this.clbFileName.Visible = true;
            GetAllFileName(this.txtSrcPath.Text, this.cbbFilterExt.Text);
        }

        private void GetAllFileName(string path, string allowExt)
        {
            this.clbFileName.Items.Clear();
            if (allowExt == "" || allowExt.IndexOf(".") == -1) return;
            FileInfo[] fList = new DirectoryInfo(path).GetFiles();
            foreach (FileInfo tempFile in fList)
            {
                if (string.IsNullOrEmpty(tempFile.Extension)) continue;
                Regex rg = new Regex(tempFile.Extension.ToLower() + "(;|$)");
                if ((allowExt == "*.*" || rg.IsMatch(allowExt))
                    //&& tempFile.FullName.IndexOf("Load.js")==-1 
                    //&& tempFile.FullName.IndexOf("AjaxPro")==-1 
                    )
                {
                    if (this.chkAll.Checked || (tempFile.LastWriteTime.ToString("yyyy-MM-dd").CompareTo(this.dtModifiedDateFrom.Value.ToString("yyyy-MM-dd")) >= 0 && tempFile.LastWriteTime.ToString("yyyy-MM-dd").CompareTo(this.dtModifiedDateTo.Value.ToString("yyyy-MM-dd")) <= 0))
                        this.clbFileName.Items.Add(tempFile.FullName);
                }
            }
            GetDirectoryFileName(new DirectoryInfo(path), allowExt);
        }
        private void GetDirectoryFileName(DirectoryInfo di, string allowExt)
        {
            DirectoryInfo[] dList = di.GetDirectories();
            foreach (DirectoryInfo tempDirectory in dList)
            {
                FileInfo[] fList = tempDirectory.GetFiles();
                foreach (FileInfo tempFile in fList)
                {
                    if (string.IsNullOrEmpty(tempFile.Extension)) continue;
                    Regex rg = new Regex(tempFile.Extension.ToLower() + "(;|$)");
                    if ((allowExt == "*.*" || rg.IsMatch(allowExt))
                        //&& tempFile.FullName.IndexOf("Load.js")==-1
                        //&& tempFile.FullName.IndexOf("AjaxPro")==-1  
                        )
                        if (this.chkAll.Checked || (tempFile.LastWriteTime.ToString("yyyy-MM-dd").CompareTo(this.dtModifiedDateFrom.Value.ToString("yyyy-MM-dd")) >= 0 && tempFile.LastWriteTime.ToString("yyyy-MM-dd").CompareTo(this.dtModifiedDateTo.Value.ToString("yyyy-MM-dd")) <= 0))
                            this.clbFileName.Items.Add(tempFile.FullName);
                }
                GetDirectoryFileName(tempDirectory, allowExt);
            }
        }

        private void btnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.clbFileName.Items.Count; i++)
            {
                this.clbFileName.SetItemChecked(i, true);
            }
        }

        private void btnUnSelectAll_Click(object sender, EventArgs e)
        {
            for (int i = 0; i < this.clbFileName.Items.Count; i++)
            {
                this.clbFileName.SetItemChecked(i, false);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            object o = this.clbFileName.SelectedItem;
            if (o == null)
            {
                MessageBox.Show("请先选中项目！"); return;
            }
            int sindex = this.clbFileName.SelectedIndex;
            int count = this.clbFileName.Items.Count;
            if (sindex == 0) return;
            bool cChecked = this.clbFileName.GetItemChecked(sindex);
            this.clbFileName.Items[sindex] = this.clbFileName.Items[sindex - 1];
            this.clbFileName.SetItemChecked(sindex, this.clbFileName.GetItemChecked(sindex - 1));
            this.clbFileName.Items[sindex - 1] = o;
            this.clbFileName.SetItemChecked(sindex - 1, cChecked);
            this.clbFileName.SelectedIndex = sindex - 1;
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            object o = this.clbFileName.SelectedItem;
            if (o == null)
            {
                MessageBox.Show("请先选中项目！"); return;
            }
            int sindex = this.clbFileName.SelectedIndex;
            int count = this.clbFileName.Items.Count;
            if (sindex == count - 1) return;
            bool cChecked = this.clbFileName.GetItemChecked(sindex);
            this.clbFileName.Items[sindex] = this.clbFileName.Items[sindex + 1];
            this.clbFileName.SetItemChecked(sindex, this.clbFileName.GetItemChecked(sindex + 1));
            this.clbFileName.Items[sindex + 1] = o;
            this.clbFileName.SetItemChecked(sindex + 1, cChecked);
            this.clbFileName.SelectedIndex = sindex + 1;
        }

        private void btnShift_Click(object sender, EventArgs e)
        {
            object o = this.clbFileName.SelectedItem;
            if (o == null)
            {
                MessageBox.Show("请先选中项目！"); return;
            }
            int i = this.clbFileName.SelectedIndex;
            this.clbFileName.Items.RemoveAt(this.clbFileName.SelectedIndex);
            if (i < this.clbFileName.Items.Count)
                this.clbFileName.SelectedIndex = i;
            else if (this.clbFileName.Items.Count > 0) this.clbFileName.SelectedIndex = i - 1;
        }

        private void btnCopyFile_Click(object sender, EventArgs e)
        {
            if (this.clbFileName.CheckedItems.Count == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                return;
            }
            if (MessageBox.Show("确定拷贝选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;
            try
            {
                if (this.chkMerge.Checked == false)
                {
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        string desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));

                        File.Copy(srcFilePath, desFilePath, true);
                    }
                }
                else
                {
                    string sInitCode = "";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        sInitCode += content + "\r\n";
                    }
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(sInitCode);
                    writer.Flush();
                    writer.Close();
                }
                MessageBox.Show("操作成功！", "提示");
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnFormatFile_Click(object sender, EventArgs e)
        {
            //if (this.isProhibit) return;
            int iCheckedCount = this.clbFileName.CheckedItems.Count;
            if (iCheckedCount == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                return;
            }
            if (MessageBox.Show("确定排版选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;
            try
            {
                if (this.chkMerge.Checked == false)
                {
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        string desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        //if (!this.isValid)
                        //{
                        //    if (this.CheckFileSize(content,false) == false) return;
                        //}
                        FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, this.encoding);
                        writer.Write(ScriptDealer.Format(content));
                        writer.Flush();
                        writer.Close();
                    }
                }
                else
                {
                    string sInitCode = "";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        sInitCode += content + "\r\n";
                    }
                        if (!this.isValid)
                        {
                            if (this.CheckFileSize(sInitCode,false) == false) return;
                        }
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(ScriptDealer.Format(sInitCode));
                    writer.Flush();
                    writer.Close();
                }
                MessageBox.Show("操作成功！", "提示");
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnTrim_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            int iCheckedCount = this.clbFileName.CheckedItems.Count;
            if (iCheckedCount == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                this.txtOutputFileName.Focus();
                return;
            }
            if (MessageBox.Show("确定压缩选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;
            try
            {
                if (this.chkMerge.Checked == false)
                {
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        string desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        //if (!this.isValid)
                        //{
                        //    if (this.CheckFileSize(content, false) == false) return;
                        //}
                        FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, this.encoding);
                        if (Path.GetExtension(srcFilePath).ToLower() == ".css")
                            writer.Write(ScriptDealer.CompressionCss(content));
                        else
                            writer.Write(ScriptDealer.BasicCompression(content));
                        writer.Flush();
                        writer.Close();
                    }
                }
                else
                {
                    string sInitCode = "";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        sInitCode += content;
                    }
                    if (!this.isValid)
                    {
                        if (this.CheckFileSize(sInitCode,false) == false) return;
                    }
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    if (Path.GetExtension(desFilePath).ToLower() == ".css")
                        writer.Write(ScriptDealer.CompressionCss(sInitCode));
                    else
                    writer.Write(ScriptDealer.BasicCompression(sInitCode));
                    writer.Flush();
                    writer.Close();
                }

                MessageBox.Show("操作成功！", "提示");
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnConfuseFiles_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            if (!this.CheckSet()) { tabControl1.SelectedIndex = 0; return; }
            int iCheckedCount = this.clbFileName.CheckedItems.Count;
            if (iCheckedCount == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                this.txtOutputFileName.Focus();
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                return;
            }
            if (MessageBox.Show("确定混淆选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;

            try
            {
                Confusion cf = new Confusion();
                cf.SetConfusionOptions(this.ReadOptions());
                cf.IgnoreString = fileSplitString;
                addIgnoreSentence(cf);
                string sInitCode = "", sResult = "";
                int j = 0;
                for (int i = 0; i < this.clbFileName.Items.Count; i++)
                {
                    if (this.clbFileName.GetItemChecked(i) == false) continue;
                    string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                    System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                    string content = sr.ReadToEnd();
                    sr.Close();
                    sInitCode += content;
                    if (this.chkMerge.Checked == false)
                    {
                        if (j < iCheckedCount-1) sInitCode += "\r\n" + cf.IgnoreString;
                        cf.DicFilename.Add(j+1,Helper.GetHash(Path.GetFileName(srcFilePath)));
                    }
                    j++;
                }
                if (this.chkMerge.Checked)
                {
                    cf.IsMerge = true;
                }
                else
                {
                    cf.IsMerge = false;
                    cf.FileNum = j;
                }

                if (!this.isValid)
                {
                    if (this.CheckFileSize(sInitCode) == false) return;
                }
                cf.Code = sInitCode;
                sResult = cf.Exec();
                if (this.chkMerge.Checked == false)
                {
                    //string[] arrRet = new Regex(cf.IgnoreString).Split(sResult);
                    string[] arrRet = sResult.Split(new string[1] { cf.IgnoreString }, StringSplitOptions.None);
                    int iCheckedNO = 0;
                    string srcFilePath = "";
                    string desFilePath = "";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        if (srcFilePath.IndexOf(this.txtSrcPath.Text) != -1)
                            desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        else
                            desFilePath = this.txtDesPath.Text + "\\" + Path.GetFileName(srcFilePath);
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                        FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, this.encoding);
                        //#if( DEBUG)
                        //                        writer.Write(ScriptDealer.Format(arrRet[iCheckedNO]));
                        //#else
                        //                        writer.Write(arrRet[iCheckedNO]);
                        //#endif
                        if (j > 1 && cf.IsMoveStr)
                        {
                            writer.Write(cf.GetFileMoveStringArrays(iCheckedNO + 1));
                        }
                        writer.Write(arrRet[iCheckedNO]);
                        writer.Flush();
                        writer.Close();
                        iCheckedNO++;
                    }
                }
                else
                {
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(sResult);
                    writer.Flush();
                    writer.Close();
                }
                MessageBox.Show("操作成功！", "提示");
                cf.Dispose();
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnEncrypt_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            int iCheckedCount = this.clbFileName.CheckedItems.Count;
            if (iCheckedCount == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                this.txtOutputFileName.Focus();
                return;
            }
            if (MessageBox.Show("确定加密选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;
            try
            {
                if (this.chkMerge.Checked == false)
                {
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        string desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        if (!this.isValid)
                        {
                            if (this.CheckFileSize(content,false) == false) return;
                        }
                        FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, this.encoding);
                        writer.Write(ScriptDealer.Encrypt(content));
                        writer.Flush();
                        writer.Close();
                    }
                }
                else
                {
                    string sInitCode = "";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                        string content = sr.ReadToEnd();
                        sr.Close();
                        sInitCode += content;
                    }
                    if (!this.isValid)
                    {
                        if (this.CheckFileSize(sInitCode,false) == false) return;
                    }
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(ScriptDealer.Encrypt(sInitCode));
                    writer.Flush();
                    writer.Close();
                }

                MessageBox.Show("操作成功！", "提示");
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnConfuseEncrypt_Click(object sender, EventArgs e)
        {
            if (this.isProhibit) return;
            if (!this.CheckSet()) { tabControl1.SelectedIndex = 0; return; }
            int iCheckedCount = this.clbFileName.CheckedItems.Count;
            if (iCheckedCount == 0)
            {
                MessageBox.Show("请选择文件！", "提示");
                return;
            }
            if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() == "")
            {
                MessageBox.Show("请输入合并文件名！", "提示");
                this.txtOutputFileName.Focus();
                return;
            }
            if (MessageBox.Show("确定混淆且加密选中文件吗?", "提示",
                MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                return;
            try
            {
                Confusion cf = new Confusion();
                cf.SetConfusionOptions(this.ReadOptions());
                cf.IgnoreString = fileSplitString;
                addIgnoreSentence(cf);
                string sInitCode = "", sResult = "";
                int j = 0;
                for (int i = 0; i < this.clbFileName.Items.Count; i++)
                {
                    if (this.clbFileName.GetItemChecked(i) == false) continue;
                    string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                    System.IO.StreamReader sr = new StreamReader(srcFilePath, this.encoding);
                    string content = sr.ReadToEnd();
                    sr.Close();
                    sInitCode += content;
                    if (this.chkMerge.Checked == false)
                    {
                        if (j < iCheckedCount - 1) sInitCode += "\r\n" + cf.IgnoreString;
                        cf.DicFilename.Add(j + 1, Helper.GetHash(Path.GetFileName(srcFilePath)));
                    }
                    j++;
                }
                if (this.chkMerge.Checked)
                {
                    cf.IsMerge = true;
                }
                else
                {
                    cf.IsMerge = false;
                    cf.FileNum = j;
                }

                if (!this.isValid)
                {
                    if (this.CheckFileSize(sInitCode) == false) return;
                }
                cf.Code = sInitCode;
                sResult = cf.Exec();
                ECMAScriptPacker p = new ECMAScriptPacker();
                p.HaveCompressed = true;
                if (this.chkMerge.Checked == false)
                {
                    //string[] arrRet = new Regex(cf.IgnoreString).Split(sResult);
                    string[] arrRet = sResult.Split(new string[1] { cf.IgnoreString }, StringSplitOptions.None);
                    int iCheckedNO = 0;
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        string srcFilePath = this.clbFileName.GetItemText(this.clbFileName.Items[i]);
                        string desFilePath = this.txtDesPath.Text + srcFilePath.Replace(this.txtSrcPath.Text, "");
                        if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                            Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                        FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                        StreamWriter writer = new StreamWriter(fs, this.encoding);
                        //if (j > 1 && cf.IsMoveStr)
                        //{
                        //    writer.Write(cf.GetFileMoveStringArrays(iCheckedNO + 1));
                        //}
                        //writer.Write(p.Pack(arrRet[iCheckedNO]));
                        writer.Write(p.Pack(cf.GetFileMoveStringArrays(iCheckedNO + 1)+arrRet[iCheckedNO]));
                        writer.Flush();
                        writer.Close();
                        iCheckedNO++;
                    }
                }
                else
                {
                    string desFilePath = this.txtDesPath.Text + "\\" + (Path.HasExtension(this.txtOutputFileName.Text) ? this.txtOutputFileName.Text : this.txtOutputFileName.Text + ".js");
                    if (!Directory.Exists(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\"))))
                        Directory.CreateDirectory(desFilePath.Substring(0, desFilePath.LastIndexOf(@"\")));
                    FileStream fs = File.Open(desFilePath, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(p.Pack(sResult));
                    writer.Flush();
                    writer.Close();
                }
                MessageBox.Show("操作成功！", "提示");
                cf.Dispose();
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void toolBar1_ButtonClick(object sender, ToolBarButtonClickEventArgs e)
        {
            if (e.Button.ImageIndex == 0)//打开
            {
                DialogResult r = this.ofdMojs.ShowDialog(this);
                if (r == DialogResult.OK)
                {
                    Stream s = this.ofdMojs.OpenFile();
                    TextReader rd = new StreamReader(s, this.encoding);
                    string SrcPath = rd.ReadLine().Replace("SrcPath=", "");
                    string DesPath = rd.ReadLine().Replace("DesPath=", "");
                    string ComFileName = rd.ReadLine().Replace("ComFileName=", "");
                    string FileList = rd.ReadLine().Replace("FileList=", "");
                    string ConfusionOptions = rd.ReadLine().Replace("ConfusionOptions=", "");
                    rd.Close();
                    s.Close();
                    this.txtSrcPath.Text = SrcPath;
                    this.txtDesPath.Text = DesPath;
                    if (ComFileName != "")
                    {
                        this.chkMerge.Checked = true;
                        this.txtOutputFileName.Text = ComFileName;
                    }
                    this.clbFileName.Items.Clear();
                    string[] arrFileName = FileList.Split(new char[] { ';' });
                    foreach (string fn in arrFileName)
                    {
                        if (fn == "") continue;
                        this.clbFileName.Items.Add(fn);
                    }
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        this.clbFileName.SetItemChecked(i, true);
                    }
                    string[] arrOpts = ConfusionOptions.Split(new char[] { ';' });
                    this.cbMoveStr.Checked = Convert.ToBoolean(arrOpts[0]);
                    this.cbEncodeStr.Checked = Convert.ToBoolean(arrOpts[1]);
                    this.cbIntToHex.Checked = Convert.ToBoolean(arrOpts[2]);
                    this.cbConfuseVariable.Checked = Convert.ToBoolean(arrOpts[3]);
                    this.cbConfuseFunctionName.Checked = Convert.ToBoolean(arrOpts[4]);
                    this.cbConfuseClassMember.Checked = Convert.ToBoolean(arrOpts[5]);
                    this.txtNoVariable.Text = arrOpts[6];
                    this.txtNoClassMember.Text = arrOpts[7];
                    if (arrOpts.Length > 8)
                        this.cbbEncoding.Text = arrOpts[8];
                    if (arrOpts.Length > 9)
                        this.chkOnlyCH.Checked = Convert.ToBoolean(arrOpts[9]);
                    if (arrOpts.Length > 10)
                        this.cbUseSquareBracket.Checked = Convert.ToBoolean(arrOpts[10]);
                    if (arrOpts.Length > 11)
                        this.cbIncludeSystemObject.Checked = Convert.ToBoolean(arrOpts[11]);

                    this.isOpen = true;
                    this.openFileName = this.ofdMojs.FileName;
                    this.chkAll.Checked = false;
                    this.dtModifiedDateFrom.Enabled = false;
                    this.dtModifiedDateTo.Enabled = false;
                }
            }
            else if (e.Button.ImageIndex == 1)//保存
            {
                if (this.isOpen == false)
                {
                    DialogResult r = this.sfdMojs.ShowDialog(this);
                    if (r == DialogResult.OK)
                    {
                        string sContent = "";
                        sContent += "SrcPath=" + this.txtSrcPath.Text.Trim() + "\r\n";
                        sContent += "DesPath=" + this.txtDesPath.Text.Trim() + "\r\n";
                        if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() != "")
                        {
                            sContent += "ComFileName=" + this.txtOutputFileName.Text.Trim() + "\r\n";
                        }
                        else
                        {
                            sContent += "ComFileName=\r\n";
                        }
                        sContent += "FileList=";
                        for (int i = 0; i < this.clbFileName.Items.Count; i++)
                        {
                            if (this.clbFileName.GetItemChecked(i) == false) continue;
                            sContent += this.clbFileName.GetItemText(this.clbFileName.Items[i]) + ";";
                        }
                        sContent += "\r\nConfusionOptions=";
                        sContent += string.Join(";", new string[] { this.cbMoveStr.Checked.ToString(), this.cbEncodeStr.Checked.ToString(), this.cbIntToHex.Checked.ToString(), this.cbConfuseVariable.Checked.ToString(), this.cbConfuseFunctionName.Checked.ToString(), this.cbConfuseClassMember.Checked.ToString(), this.txtNoVariable.Text.Trim(), this.txtNoClassMember.Text.Trim(), this.cbbEncoding.Text, this.chkOnlyCH.Checked.ToString(), this.cbUseSquareBracket.Checked.ToString(), this.cbIncludeSystemObject.Checked.ToString() });
                        
                        Stream s = sfdMojs.OpenFile();
                        TextWriter rd = new StreamWriter(s, this.encoding);
                        rd.Write(sContent);
                        rd.Close();
                        s.Close();
                        this.isOpen = true;
                        this.openFileName = this.ofdMojs.FileName;
                    }
                }
                else//打开保存
                {
                    string sContent = "";
                    sContent += "SrcPath=" + this.txtSrcPath.Text.Trim() + "\r\n";
                    sContent += "DesPath=" + this.txtDesPath.Text.Trim() + "\r\n";
                    if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() != "")
                    {
                        sContent += "ComFileName=" + this.txtOutputFileName.Text.Trim() + "\r\n";
                    }
                    else
                    {
                        sContent += "ComFileName=\r\n";
                    }
                    sContent += "FileList=";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        sContent += this.clbFileName.GetItemText(this.clbFileName.Items[i]) + ";";
                    }
                    sContent += "\r\nConfusionOptions=";
                    sContent += string.Join(";", new string[] { this.cbMoveStr.Checked.ToString(), this.cbEncodeStr.Checked.ToString(), this.cbIntToHex.Checked.ToString(), this.cbConfuseVariable.Checked.ToString(), this.cbConfuseFunctionName.Checked.ToString(), this.cbConfuseClassMember.Checked.ToString(), this.txtNoVariable.Text.Trim(), this.txtNoClassMember.Text.Trim(), this.cbbEncoding.Text, this.chkOnlyCH.Checked.ToString(), this.cbUseSquareBracket.Checked.ToString(), this.cbIncludeSystemObject.Checked.ToString() });
                        
                    FileStream fs = File.Open(this.openFileName, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, this.encoding);
                    writer.Write(sContent);
                    writer.Flush();
                    writer.Close();
                    fs.Close();
                }
            }
            else if (e.Button.ImageIndex == 2)//另存为
            {
                DialogResult r = this.sfdMojs.ShowDialog(this);
                if (r == DialogResult.OK)
                {
                    string sContent = "";
                    sContent += "SrcPath=" + this.txtSrcPath.Text.Trim() + "\r\n";
                    sContent += "DesPath=" + this.txtDesPath.Text.Trim() + "\r\n";
                    if (this.chkMerge.Checked && this.txtOutputFileName.Text.Trim() != "")
                    {
                        sContent += "ComFileName=" + this.txtOutputFileName.Text.Trim() + "\r\n";
                    }
                    else
                    {
                        sContent += "ComFileName=\r\n";
                    }
                    sContent += "FileList=";
                    for (int i = 0; i < this.clbFileName.Items.Count; i++)
                    {
                        if (this.clbFileName.GetItemChecked(i) == false) continue;
                        sContent += this.clbFileName.GetItemText(this.clbFileName.Items[i]) + ";";
                    }
                    sContent += "\r\nConfusionOptions=";
                    sContent += string.Join(";", new string[] { this.cbMoveStr.Checked.ToString(), this.cbEncodeStr.Checked.ToString(), this.cbIntToHex.Checked.ToString(), this.cbConfuseVariable.Checked.ToString(), this.cbConfuseFunctionName.Checked.ToString(), this.cbConfuseClassMember.Checked.ToString(), this.txtNoVariable.Text.Trim(), this.txtNoClassMember.Text.Trim(), this.cbbEncoding.Text, this.chkOnlyCH.Checked.ToString() });
                        
                    Stream s = sfdMojs.OpenFile();
                    TextWriter rd = new StreamWriter(s, this.encoding);
                    rd.Write(sContent);
                    rd.Close();
                    s.Close();
                    this.isOpen = true;
                    this.openFileName = this.ofdMojs.FileName;
                }
            }
            else if (e.Button.ImageIndex == 3)//打开文件
            {
                DialogResult r = this.ofdOpenFile.ShowDialog(this);
                if (r == DialogResult.OK)
                {
                    this.clbFileName.Items.Add(this.ofdOpenFile.FileName);
                }
            }
        }

        private void chkAll_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkAll.Checked)
            {
                this.dtModifiedDateFrom.Enabled = false;
                this.dtModifiedDateTo.Enabled = false;
            }
            else
            {
                this.dtModifiedDateFrom.Enabled = true;
                this.dtModifiedDateTo.Enabled = true;
            }
        }

        private void chkMerge_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkMerge.Checked)
            {
                this.lblFileName.Visible = true;
                this.txtOutputFileName.Visible = true;
            }
            else
            {
                this.lblFileName.Visible = false;
                this.txtOutputFileName.Visible = false;
            }
        }

        #endregion

        #region 软件注册
        private void btnRegister_Click(object sender, EventArgs e)
        {
            try
            {
                bool bSuccess = false;
                //string fileName = "key.txt";
                //string regCode = this.txtRegCode.Text;
                //ArrayList al = Helper.GetAllMACAddresses();
                //foreach (string mac in al)
                //{
                //    if (regCode == Helper.GetHash2(mac))
                //    {
                //        bSuccess = true;
                //        break;
                //    }
                //}
                string regCode = this.txtRegCode.Text;
                string fileName = getFileName();
                System.IO.StreamReader sr = new StreamReader(fileName, Encoding.Default);
                string content = sr.ReadToEnd();
                sr.Close();
                string[] arr = content.Split(new char[] { '\n' });
                //string createdTime = arr[0];
                string createdTime =  MUI.Components.Encryption.EncryptDecrypt.DecryptDES(arr[0], "26551105");
                string decCode = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "cwmunest");
                string guid = arr[2];
                if (decCode.Equals(regCode)) bSuccess = true;

                if (bSuccess)
                {
                    File.SetAttributes(fileName, FileAttributes.Normal);
                    FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                    //writer.Write(regCode);
                    //writer.Write(createdTime + "\n" + regCode);
                    writer.Write(arr[0] + "\n" + regCode + "\n" + guid);
                    writer.Flush();
                    writer.Close();
                    File.SetAttributes(fileName, FileAttributes.Hidden | FileAttributes.System);

                    this.isValid = true;
                    MessageBox.Show("注册成功，谢谢使用！", "提示");

                    gb1.Visible = false;
                    gb2.Visible = false;
                    lblthankyouforuser.Text = "您已注册，谢谢使用！";
                    lblthankyouforuser.Visible = true;

                    this.btnFormat.ForeColor = Color.Black;
                    this.btnFormatFile.ForeColor = Color.Black;
                    this.btnTrimCode.ForeColor = Color.Black;
                    this.btnTrim.ForeColor = Color.Black;
                    this.btnCopyFile.ForeColor = Color.Black;

                    RomoteValidate(createdTime, createdTime,guid);
                }
                else
                {
                    MessageBox.Show("注册码有误！", "提示");
                }
            }
            catch (Exception e1)
            {
                MessageBox.Show("操作失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }

        private void btnOnlineRegister_Click(object sender, EventArgs e)
        {
            string regCode = "";
            try
            {
                string fileName = getFileName();
                System.IO.StreamReader sr = new StreamReader(fileName, Encoding.Default);
                string content = sr.ReadToEnd();
                sr.Close();
                string[] arr = content.Split(new char[] { '\n' });
                // string createdTime = arr[0];
                string createdTime = MUI.Components.Encryption.EncryptDecrypt.DecryptDES(arr[0], "26551105");
                string guid = arr[2];
                if(string.IsNullOrEmpty(guid)){ 
                    guid=Helper.GetFirstOfMACAddress();
                }
                if (!IsUsedForever)
                {
                    //创建 Web Service 访问类,实现与 Web Service 通讯。
                    HDS.JSObfuscator.HDSWebService.HDJSORegister rs = new JSObfuscator.HDSWebService.HDJSORegister();
                    rs.Url = wsURL;
                    rs.Timeout = 60 * 1000;
                    regCode = rs.GetRegisterCode(txtUserName.Text, "_" + guid, createdTime);
                    if (regCode == "") return;
                }
                else
                {
                    try
                    {
                        //创建 Web Service 访问类,实现与 Web Service 通讯。
                        HDS.JSObfuscator.HDSWebService.HDJSORegister rs = new JSObfuscator.HDSWebService.HDJSORegister();
                        rs.Url = wsURL;
                        rs.Timeout = 60 * 1000;
                        string ret = rs.GetRegisterCode(txtUserName.Text, guid, createdTime);
                        if (ret == "") return;
                    }
                    catch { }
                    if (txtUserName.Text.Trim() == UserName)
                        regCode = MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "cwmunest");
                }
                if (regCode != null && regCode.Length > 0 && regCode.Equals(MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "cwmunest")))
                {
                    //string fileName = "key.txt";
                    //FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                    //StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                    //writer.Write(regCode);
                    //writer.Flush();
                    //writer.Close();

                    File.SetAttributes(fileName, FileAttributes.Normal);
                    FileStream fs = File.Open(fileName, FileMode.Create, FileAccess.Write);
                    StreamWriter writer = new StreamWriter(fs, Encoding.Default);
                    //writer.Write(regCode);
                    //MessageBox.Show(fileName+"\n"+arr[0] + "\n" + regCode + "\n" + guid);
                    writer.Write(arr[0] + "\n" + regCode + "\n" + guid);
                    writer.Flush();
                    writer.Close();
                    File.SetAttributes(fileName, FileAttributes.Hidden | FileAttributes.System);

                    this.isValid = true;
                    this.isProhibit = false;
                    MessageBox.Show("注册成功，谢谢使用！", "提示");

                    gb1.Visible = false;
                    gb2.Visible = false;
                    lblthankyouforuser.Text = "您已注册，谢谢使用！";
                    lblthankyouforuser.Visible = true;

                    this.btnFormat.ForeColor = Color.Black;
                    this.btnFormatFile.ForeColor = Color.Black;
                    this.btnTrimCode.ForeColor = Color.Black;
                    this.btnTrim.ForeColor = Color.Black;
                    this.btnCopyFile.ForeColor = Color.Black;
                }
                else
                {
                    MessageBox.Show("请与作者联系，经作者确认后方可注册！", "提示");
                }

            }
            catch (Exception e1)
            {
                MessageBox.Show("网络连接失败！" + "\r\n" + e1.ToString(), "提示");
            }
        }
        //获取输入注册码注册方式的密钥，其实是加密创建日期，用密钥:26551105
        public string GetRegisterKey()
        {
            string fileName = getFileName();
            System.IO.StreamReader sr = new StreamReader(fileName, Encoding.Default);
            string content = sr.ReadToEnd();
            sr.Close();
            string[] arr = content.Split(new char[] { '\n' });
            //string createdTime = arr[0];
            string createdTime =  MUI.Components.Encryption.EncryptDecrypt.DecryptDES(arr[0], "26551105");

            if (MUI.Components.ECMAScriptDealer.Helper.CheckValue(createdTime,'T'))
            {
                return MUI.Components.Encryption.EncryptDecrypt.EncryptDES(createdTime, "26551105");
            }
            return "";
        }

        private void linkInputCode_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            //MessageBox.Show("请提供本机网卡MAC地址！\r\n获取方法是在命令行中输入：ipconfig -all", "提示");
        }

        private void linkInputUserName_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MessageBox.Show("请事先与作者联系，以获取合法的用户名。推荐使用！", "提示");
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://www.moralsoft.com");
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:1079503892@qq.com");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("http://wpa.qq.com/msgrd?V=1&amp;Uin=1079503892&amp;Site=弘德软件&amp;Menu=yes");
        }


        #endregion

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
            this.Dispose();
        }
        private void linkLabel4_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            this.txtRegCode.Text = GetRegisterKey();
        }

        #region 浮动窗口提示
        private void ShowNotifier(string verno, string adv, string url)
        {
            //adv = "test"; url = "http://www.moralsoft.com";
            bool bUpdating = false, bAdvertising = false;
            if (!string.IsNullOrEmpty(verno) && Version.CompareTo(verno.Trim()) < 0)
            {
                bUpdating = true;
            }
            if (adv != null && !string.IsNullOrEmpty(adv.Trim()))
            {
                bAdvertising = true;
                if (url != null) { url = url.Trim(); }
                if (url == "") url = null;
            }
            if (bUpdating && bAdvertising)
            {
                Notifier_Update.NoticeSys_Event(OnUpdateClick, "有升级版本啦", "请点击下载最新版");
                if (adv.IndexOf("注册") == -1 || !this.isValid)
                    Notifier_Advertising.NoticeSys(url, "好消息", adv);
            }
            else
            {
                if (bUpdating)
                    Notifier_Update.NoticeSys_Event(OnUpdateClick, "有升级版本啦", "请点击下载最新版");
                else if (bAdvertising)
                {
                    if (adv.IndexOf("注册") == -1 || !this.authorized)
                    {
                        Notifier_Update.NoticeSys(url.IndexOf("http://")==-1?null:url, "好消息", adv);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(url))
                            Notifier_Update.NoticeSys(url.IndexOf("http://") == 0 ? url : null, "好消息", url);

                    }
                }

            }
            //this.Focus();
        }
        void OnUpdateClick(TaskbarNotifier tnf)
        {
            System.Diagnostics.Process.Start("http://www.moralsoft.com/download/" + "HDS JSObfuscator.rar");
            tnf.Hide();
        }
        //void OnAdvertisingClick(TaskbarNotifier tnf)
        //{
        //    tnf.Hide();
        //    //System.Diagnostics.Process.Start(Application.StartupPath + "\\" + "KFAutoUpdate.exe");
        //}
        #endregion
    }
}
