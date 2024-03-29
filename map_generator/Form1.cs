﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.IO;
using System.Text.RegularExpressions;
using Jint;
using Jint.Native;

namespace map_generator
{
    public partial class Form1 : Form
    {

        Dictionary<int, Bitmap> dict;
        Dictionary<int, HSV[,]> colorDict;

        public Form1(Bitmap ground)
        {
            InitializeComponent();
            loadResources(ground);

            List<string> list=new List<string>();
            for (int i = 0; i < 13; i++)
                list.Add("0 0 0 0 0 0 0 0 0 0 0 0 0");
            textBox1.Text = string.Join("\r\n", list);
            button2_Click(null,null);
        }

        private void loadResources(Bitmap ground)
        {
            dict = new Dictionary<int, Bitmap>();

            string directory = ".\\";
            if (!Directory.Exists(directory + "project"))
                directory = "..\\";

            if (!Directory.Exists(directory + "project"))
            {
                MessageBox.Show("project目录不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }

            // 读取JS
            string js = "";
            js += File.ReadAllText(directory + "project\\icons.js")+";";
            js += File.ReadAllText(directory + "project\\maps.js")+";";
            js += @"
                var icons=icons_4665ee12_3a1f_44a4_bea3_0fccba634dc1;
                var maps=maps_90f36752_8815_4be8_b32b_d7fad1d0542e;

                var cls = [], indexes = [];

                var point = 0;
                for(var i=1; i<1000; i++){
                    if (maps[i]) {
                        var id = maps[i].id;
                        var clss = maps[i].cls;
                        if(clss=='autotile'){
                            continue;
                        }
                        cls[i] = clss;
                        indexes[i] = icons[clss][id];
                    }
                  }
            ";

            var engine = new Engine().Execute(js);
            var cls = engine.GetValue("cls").AsArray();
            var indexes = engine.GetValue("indexes").AsArray();

            Dictionary<string, Bitmap> dictionary = new Dictionary<string, Bitmap>();
            dict.Add(0, ground);
            for (int i = 1; i < 1000; i++)
            {
                string filename = cls.Get(Convert.ToString(i)).ToString();
                if (!"undefined".Equals(filename))
                {
                    if (!dictionary.ContainsKey(filename))
                    {
                        Bitmap bitmap = loadBitmap(directory + "\\project\\materials\\" + filename + ".png");
                        dictionary.Add(filename, bitmap);
                    }
                    Bitmap image = dictionary[filename];
                    try
                    {
                        var height = 32;
                        if (filename.Contains("48")) height = 48;
                        dict.Add(i,
                            clipImage(image, 0, Convert.ToInt32(indexes.Get(Convert.ToString(i)).ToString()), ground, height));
                    }
                    catch (Exception)
                    {
                    }
                }
            }

            colorDict = new Dictionary<int, HSV[,]>();
            foreach (int id in dict.Keys)
            {
                Bitmap bitmap = dict[id];
                HSV[,] colors = new HSV[32, 32];
                for (int i = 0; i < 32; i++)
                {
                    for (int j = 0; j < 32; j++)
                    {
                        colors[i, j] = new HSV(bitmap.GetPixel(i, j));
                    }
                }
                colorDict.Add(id, colors); 
            }

            SendMessage(textBox1.Handle, EM_SETTABSTOPS, 1, new int[] { 4 * 4 });

        }

        private const int EM_SETTABSTOPS = 0x00CB;

        [DllImport("User32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr h, int msg, int wParam, int[] lParam);

        private Bitmap loadBitmap(string path)
        {
                FileStream fs = File.OpenRead(path); //OpenRead
                int filelength = 0;
                filelength = (int) fs.Length; //获得文件长度 
                Byte[] image = new Byte[filelength]; //建立一个字节数组 
                fs.Read(image, 0, filelength); //按字节流读取 
                Image result = Image.FromStream(fs);
                fs.Close();
                Bitmap bit = new Bitmap(result);
                return bit;
        }

        private Bitmap clipImage(Bitmap source, int offsetX, int offsetY, Bitmap road = null, int height=32)
        {
            offsetX *= 32; offsetY *= height;
            Bitmap bitmap = new Bitmap(32, 32);
            Graphics graphic = Graphics.FromImage(bitmap);
            if (road != null)
                graphic.DrawImage(road, 0, 0, new Rectangle(0, 0, 32, 32), GraphicsUnit.Pixel);
            graphic.DrawImage(source, 0, 0, new Rectangle(offsetX, offsetY, 32, 32), GraphicsUnit.Pixel);
            return bitmap;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            Bitmap bitmap = new Bitmap(416, 416);
            Graphics graphic = Graphics.FromImage(bitmap);

            string text = textBox1.Text;
            text = new Regex("\\t+").Replace(text, "  ");
            text = text.Replace("[", "").Replace("]", "").Replace(",", "");
            text = new Regex(" +").Replace(text, "\t");
            textBox1.Text = text;

            string[] lines = text.Split('\n');
            for (int i = 0; i < lines.Length; i++)
            {
                string[] words = lines[i].Trim().Split('\t');
                for (int j = 0; j < words.Length; j++)
                {
                    try
                    {
                        int id = Convert.ToInt32(words[j]);
                        graphic.DrawImage(dict[id], new Rectangle(32 * j, 32 * i, 32, 32), new Rectangle(0, 0, 32, 32), GraphicsUnit.Pixel);
                    }
                    catch (Exception) { }
                }
            }
            pictureBox1.Image = bitmap;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Image image = null;

            if (Clipboard.ContainsImage())
            {
                image = Clipboard.GetImage();
            }
            else
            {
                OpenFileDialog fileDialog = new OpenFileDialog();
                fileDialog.Filter = "图片文件|*.bmp;*.jpg;*.jpeg;*.gif;*.png";
                fileDialog.FilterIndex = 1;
                if (fileDialog.ShowDialog() == DialogResult.OK)
                {
                    image = Image.FromFile(fileDialog.FileName);
                }
            }

            if (image == null) return;
            Bitmap bitmap = new Bitmap(image.Width, image.Height);
            Graphics graphic = Graphics.FromImage(bitmap);
            Core.drawImage(graphic, image);
            textBox1.Text = Core.calculate(bitmap, colorDict);
            button2_Click(null,null);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            try
            {
                string text = textBox1.Text;
                string[] strings = text.Split(new char[] {'\n'});

                StringBuilder builder=new StringBuilder();
                var nextline = false;
                foreach (string s in strings)
                {
                    if (s.Trim().Length==0) continue;
                    string[] ss = s.Trim().Split(new char[] { '\t' });
                    if (nextline) builder.Append(',').Append('\n');
                    nextline = true;

                    var start = true;
                    foreach (string s1 in ss)
                    {
                        if (start) builder.Append('[');
                        else builder.Append(',');
                        start = false;
                        if (s1.Length < 3)
                        {
                            builder.Append(' ', 3 - s1.Length);
                        }
                        builder.Append(s1);
                    }
                    builder.Append(']');
                }
                Clipboard.SetText(builder.ToString());
                MessageBox.Show("地图的JS格式已复制到剪切板！", "复制成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("请全选并手动复制。", "复制失败！");
            }

        }

        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            Application.Exit();
        }


    }
}
