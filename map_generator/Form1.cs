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
            Bitmap items = loadBitmap("images/items.png");
            Bitmap terrains = loadBitmap("images/terrains.png");
            Bitmap enemys = loadBitmap("images/enemys.png");
            Bitmap animates = loadBitmap("images/animates.png");
            Bitmap ground = clipImage(terrains,0,0);

            // read text
            string[] maps = File.ReadAllLines("images/meaning.txt");

            Dictionary<string, Bitmap> dictionary = new Dictionary<string, Bitmap>();
            foreach (string map in maps)
            {
                string x = map.Trim();
                if (x.StartsWith("#") || x.Length==0) continue;



            }


            // 0-20 地形
            dict.Add(0, ground);
            dict.Add(1, clipImage(terrains, 0, 1, ground)); // 黄墙
            dict.Add(2, clipImage(terrains, 0, 2, ground)); // 白墙
            dict.Add(3, clipImage(terrains, 0, 3, ground)); // 蓝墙
            dict.Add(4, clipImage(terrains, 0, 4, ground)); // 星空
            dict.Add(5, clipImage(terrains, 0, 5, ground)); // 岩浆
            dict.Add(6, clipImage(terrains, 0, 15, ground)); // 蓝商店左
            dict.Add(7, clipImage(terrains, 0, 16, ground)); // 蓝商店右
            dict.Add(8, clipImage(terrains, 0, 17, ground)); // 粉商店左
            dict.Add(9, clipImage(terrains, 0, 18, ground)); // 粉商店右
            dict.Add(10, clipImage(animates, 0, 23, ground)); // 血网

            // 21-80 消耗品
            dict.Add(21, clipImage(items, 0, 0, ground));
            dict.Add(22, clipImage(items, 0, 1, ground));
            dict.Add(23, clipImage(items, 0, 2, ground));
            dict.Add(24, clipImage(items, 0, 3, ground));
            dict.Add(25, clipImage(items, 0, 4, ground));
            dict.Add(26, clipImage(items, 0, 6, ground)); // 钥匙
            dict.Add(27, clipImage(items, 0, 16, ground));
            dict.Add(28, clipImage(items, 0, 17, ground));
            dict.Add(29, clipImage(items, 0, 18, ground));
            dict.Add(30, clipImage(items, 0, 19, ground));
            dict.Add(31, clipImage(items, 0, 20, ground));
            dict.Add(32, clipImage(items, 0, 21, ground));
            dict.Add(33, clipImage(items, 0, 22, ground));
            dict.Add(34, clipImage(items, 0, 23, ground)); // 宝石、血瓶
            dict.Add(35, clipImage(items, 0, 50, ground));
            dict.Add(36, clipImage(items, 0, 55, ground));
            dict.Add(37, clipImage(items, 0, 51, ground));
            dict.Add(38, clipImage(items, 0, 56, ground));
            dict.Add(39, clipImage(items, 0, 52, ground));
            dict.Add(40, clipImage(items, 0, 57, ground));
            dict.Add(41, clipImage(items, 0, 53, ground));
            dict.Add(42, clipImage(items, 0, 58, ground));
            dict.Add(43, clipImage(items, 0, 54, ground));
            dict.Add(44, clipImage(items, 0, 59, ground)); // 剑盾
            dict.Add(45, clipImage(items, 0, 9, ground)); // 怪物手册
            dict.Add(46, clipImage(items, 0, 12, ground)); // 楼传器
            dict.Add(47, clipImage(items, 0, 45, ground)); // 破墙
            dict.Add(48, clipImage(items, 0, 44, ground)); // 破冰
            dict.Add(49, clipImage(items, 0, 43, ground)); // 炸弹
            dict.Add(50, clipImage(items, 0, 13, ground)); // 中心对称
            dict.Add(51, clipImage(items, 0, 15, ground)); // 上楼器
            dict.Add(52, clipImage(items, 0, 14, ground)); // 下楼器
            dict.Add(53, clipImage(items, 0, 11, ground)); // 幸运金币
            dict.Add(54, clipImage(items, 0, 41, ground)); // 冰冻徽章
            dict.Add(55, clipImage(items, 0, 40, ground)); // 十字架
            dict.Add(56, clipImage(items, 0, 29, ground)); // 圣水
            dict.Add(57, clipImage(items, 0, 8, ground)); // 地震卷轴

            // 81-100 门、楼梯、传送门
            dict.Add(81, clipImage(terrains, 0, 9, ground));
            dict.Add(82, clipImage(terrains, 0, 10, ground));
            dict.Add(83, clipImage(terrains, 0, 11, ground));
            dict.Add(84, clipImage(terrains, 0, 12, ground));
            dict.Add(85, clipImage(terrains, 0, 13, ground));
            dict.Add(86, clipImage(terrains, 0, 14, ground)); // 门
            dict.Add(87, clipImage(terrains, 0, 7, ground)); // 上楼
            dict.Add(88, clipImage(terrains, 0, 6, ground)); // 下楼
            dict.Add(89, clipImage(animates, 0, 21, ground));

            // 100+ 怪物
            var height = enemys.Height / 32;

            for (int i = 0; i < height; i++)
            {
                dict.Add(101 + i, clipImage(enemys, 0, i, ground));
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
            filelength = (int)fs.Length; //获得文件长度 
            Byte[] image = new Byte[filelength]; //建立一个字节数组 
            fs.Read(image, 0, filelength); //按字节流读取 
            System.Drawing.Image result = System.Drawing.Image.FromStream(fs);
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
                    builder.Append(']').Append('\n');
                }
                Clipboard.SetText(builder.ToString());
                MessageBox.Show("地图的JSON格式已复制到剪切板！", "复制成功！");
            }
            catch (Exception)
            {
                MessageBox.Show("请全选并手动复制。", "复制失败！");
            }

        }


    }
}
