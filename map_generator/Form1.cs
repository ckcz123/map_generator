using System;
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

namespace map_generator
{
    public partial class Form1 : Form
    {

        Dictionary<int, Bitmap> dict;
        Dictionary<int, HSV[,]> colorDict;

        public Form1()
        {
            InitializeComponent();
            loadResources();
        }

        private void loadResources()
        {
            dict = new Dictionary<int, Bitmap>();
            // Bitmap ground = loadBitmap("images/ground.png");
            Bitmap ground = null;

            string directory = ".\\";
            if (!Directory.Exists(directory + "images"))
                directory = "..\\";

            // read text
            string[] maps = File.ReadAllLines(directory + "images\\meaning.txt");

            Dictionary<string, Bitmap> dictionary = new Dictionary<string, Bitmap>();
            foreach (string map in maps)
            {
                string x = map.Trim();
                if (x.StartsWith("#") || x.Length==0) continue;
                int index = x.IndexOf('#');
                if (index >= 0)
                    x = x.Substring(0, index);
                x = x.Trim();
                string[] ss = x.Split(new char[] {','});
                if (ss.Length < 3) continue;
                try
                {
                    int id = Convert.ToInt32(ss[0].Trim());
                    string filename = ss[1].Trim();
                    index = Convert.ToInt32(ss[2].Trim());
                    if (!dictionary.ContainsKey(filename))
                    {
                        Bitmap bitmap = loadBitmap(directory + "images\\" + filename+".png");
                        dictionary.Add(filename, bitmap);
                    }
                    Bitmap image = dictionary[filename];
                    dict.Add(id, clipImage(image, 0, index, ground));
                    if (id == 0) // 路面
                    {
                        ground = image;
                    }
                }
                catch (Exception)
                {
                }
            }

            // 100+ 怪物
            Bitmap enemys = loadBitmap(directory + "images\\enemys.png");
            var height = enemys.Height / 32;

            for (int i = 0; i < height; i++)
            {
                dict.Add(201 + i, clipImage(enemys, 0, i, ground));
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

        private Bitmap clipImage(Bitmap source, int offsetX, int offsetY, Bitmap road = null)
        {
            offsetX *= 32; offsetY *= 32;
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

            string[] lines = textBox1.Text.Split('\n');
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
            graphic.DrawImage(image, 0, 0);
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
                foreach (string s in strings)
                {
                    if (s.Trim().Length==0) continue;
                    builder.Append('[');
                    string[] ss = s.Trim().Split(new char[] { '\t' });


                    int l = 5;
                    foreach (string s1 in ss)
                    {
                        if (l<5)
                        {
                            builder.Append(',');
                            builder.Append(' ', 5 - l);
                        }
                        builder.Append(s1);
                        l = s1.Length;
                    }
                    builder.Append(']').Append(',').Append('\n');
                }
                Clipboard.SetText(builder.ToString());
                MessageBox.Show("地图的JS格式已复制到剪切板！", "复制成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("请全选并手动复制。", "复制失败！");
            }

        }


    }
}
