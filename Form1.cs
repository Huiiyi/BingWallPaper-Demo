using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.IO;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Threading;

namespace WallPaperSoft
{
    public partial class Form1 : Form
    {
        long totalBytes = 0;
        long totalDownloadedByte = 0;
        string path = null;
        public delegate void download_dg();
        public Form1()
        {
            InitializeComponent();
        }

        private void webBrowser1_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            this.webBrowser1.Document.Body.Style = "zoom:0.25";
        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (File.Exists(Application.StartupPath + "\\" + "SavePath.ini"))
            {
                StreamReader sr = new StreamReader(Application.StartupPath + "\\" + "SavePath.ini", Encoding.Default);
                path = sr.ReadLine();
                sr.Close();
            }
            else
            {
                folderBrowserDialog1.ShowDialog();
                path = folderBrowserDialog1.SelectedPath + "\\";
                File.WriteAllText(Application.StartupPath + "\\" + "SavePath.ini", folderBrowserDialog1.SelectedPath + "\\");
            }
            //创建线程进行下载
            download_dg dg = new download_dg(DownLoadFile);
            dg.BeginInvoke(null, null);
            //Thread newThread = new Thread(DownLoadFile);
            ////newThread.SetApartmentState(ApartmentState.MTA); 
            //newThread.SetApartmentState(ApartmentState.STA);//设置这个参数，指示应用程序的COM线程模型 是 单线程单元
            //newThread.Start();
        }
        private void DownLoadFile()
        {
            string url = webBrowser1.Url.AbsoluteUri;
            //正则表达式提取文件名
            string[] str = url.Split('/');
            string file = str.Last();
            //如果没有设置过路径则设置储存文件路径，否则直接读取
            //if (File.Exists(Application.StartupPath + "\\" + "SavePath.ini"))
            //{
            //    StreamReader sr = new StreamReader(Application.StartupPath + "\\" + "SavePath.ini", Encoding.Default);
            //    path = sr.ReadLine();
            //    sr.Close();
            //}
            //else
            //{
            //    folderBrowserDialog1.ShowDialog();
            //    path = folderBrowserDialog1.SelectedPath + "\\";
            //    File.WriteAllText(Application.StartupPath + "\\" + "SavePath.ini", folderBrowserDialog1.SelectedPath + "\\");
            //}
            string filename = path + file;
            //指定url 下载文件
            HttpWebRequest request = (HttpWebRequest)HttpWebRequest.Create(url);
            Stream stream = request.GetResponse().GetResponseStream();
            //获得文件大小
            totalBytes = request.GetResponse().ContentLength;
            //创建写入流
            FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write);
            byte[] bytes = new byte[1024 * 10];
            int readCount = 0;
            while (true)
            {
                readCount = stream.Read(bytes, 0, bytes.Length);
                totalDownloadedByte = readCount + totalDownloadedByte;
                if (readCount <= 0)
                    break;
                fs.Write(bytes, 0, readCount);
                fs.Flush();
            }
            fs.Close();
            stream.Close();
            SetWallPaper(filename);
        }
        //更换壁纸
        [DllImport("user32.dll", EntryPoint = "SystemParametersInfo")]
        public static extern int SystemParametersInfo(int uAction, int uParam, string lpvParam, int fuWinIni);
        private void SetWallPaper(string fn)
        {
            //使用Split以"."分割路径，取不带后缀的文件路径
            string[] f = fn.Split('.');
            string file = f.First();
            //换壁纸函数需要bmp格式
            Image image = Image.FromFile(fn);
            image.Save(file+".bmp", System.Drawing.Imaging.ImageFormat.Bmp);
            SystemParametersInfo(20, 0, file + ".bmp", 0x2);
            Thread.Sleep(2000);
            File.Delete(file + ".bmp");
            totalDownloadedByte = 0;
        }

        //更新进度条进度
        private void timer1_Tick(object sender, EventArgs e)
        {
            if (progressBar1 != null && totalBytes != 0)
            {
                progressBar1.Maximum = (int)(totalBytes);
            }
            progressBar1.Value = (int)(totalDownloadedByte);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.ShowDialog();
            //获取当前程序运行目录创建文件 并写入用户选择的路径保存
            File.WriteAllText(Application.StartupPath + "\\" + "SavePath.ini", folderBrowserDialog1.SelectedPath + "\\");
        }
    }
}
