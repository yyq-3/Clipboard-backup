using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace 剪切板自动备份
{
    public partial class Form1 : Form
    {
        private readonly string _path = @"e:/x/" + DateTime.Now.ToString("yyyy-MM-dd");
        
        public Form1()
        {
            InitializeComponent();
            AddClipboardFormatListener(Handle);
            CheckFileExists();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            notifyIcon1.Visible = false;
            Visible = true;
            ShowInTaskbar = true;
        }

        private const int WM_SYSCOMMAND = 0x112;
        // const int SC_CLOSE = 0xF060;
        private const int SC_MINIMIZE = 0xF020;
        private const int WM_DRAWCLIPBOARD = 0x031D;
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_SYSCOMMAND)
            {
                if (m.WParam.ToInt32() == SC_MINIMIZE)
                {
                    ShowInTaskbar = false;
                    Visible = false;
                    notifyIcon1.Visible = true;
                    return;
                }
            }

            if (m.Msg == WM_DRAWCLIPBOARD)
            {
                //显示剪贴板中的文本信息
                if (Clipboard.ContainsText())
                {
                    toolStripStatusLabel1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "==>捕捉到文本";
                    SendNote(Clipboard.GetText());
                }

                //显示剪贴板中的图片信息
                if (Clipboard.ContainsImage())
                {
                    pictureBox1.Image = Clipboard.GetImage();
                    pictureBox1.Update();
                    pictureBox1.Image.Save(_path + "/" + DateTime.Now.ToString("HHmmss") + ".jpg");
                    toolStripStatusLabel1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "==>捕捉到截图";
                }
            }

            base.WndProc(ref m);
        }

        #region MyRegion

        //API declarations...
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern IntPtr AddClipboardFormatListener(IntPtr hwnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern bool RemoveClipboardFormatListener(IntPtr hWnd);

        #endregion

        // 窗口关闭前移除对剪切板监视
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            RemoveClipboardFormatListener(Handle);
        }

        // 查看文件夹是否存在
        private void CheckFileExists()
        {
            if (!Directory.Exists(_path))
            {
                Directory.CreateDirectory(_path);
            }
        }

        private void SendNote(string content)
        {
            var bw = new BinaryWriter(new FileStream(_path + "/bak.yyq", FileMode.Append));
            var br = new BinaryReader(new FileStream(_path + "/bak.yyq", FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
            
            bw.Write("\n" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + "--->" + content);
            toolStripStatusLabel1.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            bw.Close();

            var readString = br.ReadString();
            toolStripStatusLabel1.Text = readString;
            br.Close();
        }
    }
}