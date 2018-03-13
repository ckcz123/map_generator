using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace map_generator
{
    public partial class Form2 : Form
    {
        private Bitmap bitmap = null, picture = null;
        private int selection;

        public Form2()
        {
            InitializeComponent();

            Bitmap tmpMap = null;
            // load image
            if (Directory.Exists("project\\images") && File.Exists("project\\images\\terrains.png"))
            {
                tmpMap = (Bitmap)Image.FromFile("project\\images\\terrains.png");
            }
            else if (Directory.Exists("..\\project\\images") && File.Exists("..\\project\\images\\terrains.png"))
            {
                tmpMap = (Bitmap)Image.FromFile("..\\project\\images\\terrains.png");
            }
            if (tmpMap == null)
            {
                MessageBox.Show("images目录不存在！", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Application.Exit();
                return;
            }
            bitmap = cloneBitmap(tmpMap);
            picture = cloneBitmap(tmpMap);
            tmpMap.Dispose();
            selection = 0;

            drawBorder();

        }

        private Bitmap cloneBitmap(Bitmap bitmap)
        {
            return bitmap.Clone(new Rectangle(0, 0, bitmap.Width, bitmap.Height), bitmap.PixelFormat);
        }

        private void drawBorder()
        {
            picture = cloneBitmap(bitmap);
            Graphics graphics=Graphics.FromImage(picture);
            Pen pen = new Pen(Color.Crimson, 3);
            pen.Alignment = PenAlignment.Inset;
            graphics.DrawRectangle(pen, 0, 32 * selection, 32, 32);
            pen.Dispose();
            graphics.Dispose();
            pictureBox1.Image = picture;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            selection = ((MouseEventArgs)e).Y / 32;
            drawBorder();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            Bitmap ground = bitmap.Clone(new Rectangle(0, 32 * selection, 32, 32), bitmap.PixelFormat);
            bitmap.Dispose();
            picture.Dispose();
            Hide();
            new Form1(ground).Show();
        }
    }
}
