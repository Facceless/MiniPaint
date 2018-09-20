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
using System.Drawing.Imaging;
using System.IO;
using System.Threading;

namespace MiniPaint
{
    public partial class Form1 : Form
    {
        public delegate void DelegateSetProgress(object sender, int value);
        public event DelegateSetProgress SetVal;
        public delegate void InvertDelegate();
        public event InvertDelegate Inv;
        Color paintcolor;
        bool choose = false;
        bool draw = false;
        int x=0, y=0, lx=0, ly =0;
        Items currentItem;
        DrawOperationType drawMode;
        Thread thread;
        public enum Items
        {
            Rectangle, Ellipse, Line, Text, Brush, Pencil, Eraser
        }

        public enum DrawOperationType
        {
            Draw, Fill
        }
        public Form1()
        {
            SetVal += setprogress;
            InitializeComponent();
            thread = new Thread(Run) { IsBackground = true };
        }

        void setprogress(object sender, int value)
        {
            progressBar1.Value = value;
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            try {
                if (draw)
                {
                    Graphics g = pictureBox1.CreateGraphics();
                    switch (currentItem)
                    {
                        case Items.Brush:
                            if (textBox1.Text == null) throw new Exception("Please input Brush");
                            g.FillEllipse(new SolidBrush(paintcolor), e.X, e.Y, Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox1.Text));
                            break;
                        case Items.Pencil:
                            g.FillEllipse(new SolidBrush(paintcolor), e.X, e.Y, 5, 5);
                            break;
                        case Items.Eraser:
                            g.FillEllipse(new SolidBrush(Color.White), e.X, e.Y, Convert.ToInt32(textBox1.Text), Convert.ToInt32(textBox1.Text));
                            break;
                    }
                    g.Dispose();
                }  
            }
            catch(Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            try
            {
                draw = false;
                lx = e.X;
                ly = e.Y;
                if (currentItem == Items.Line)
                {
                    Graphics g = pictureBox1.CreateGraphics();
                    g.DrawLine(new Pen(new SolidBrush(paintcolor), Convert.ToInt32(textBox1.Text)), new Point(x, y), new Point(lx, ly));
                    g.Dispose();
                }
                if (currentItem == Items.Ellipse)
                {

                    Graphics g = pictureBox1.CreateGraphics();
                    if (drawMode == DrawOperationType.Draw)
                    {
                        if (textBox1.Text == null) throw new Exception("Please input Brush");
                        g.DrawEllipse(new Pen(new SolidBrush(paintcolor), Convert.ToInt32(textBox1.Text)), new Rectangle(x - 1, y - 1, e.X - x + 2, e.Y - y + 2)); 
                    }
                    else if (drawMode == DrawOperationType.Fill)
                        g.FillEllipse(new SolidBrush(paintcolor), new Rectangle(x, y, e.X - x, e.Y - y));
                    g.Dispose();
                }
                if (currentItem == Items.Rectangle)
                {
                    Graphics g = pictureBox1.CreateGraphics();
                    if (drawMode == DrawOperationType.Draw)
                    {
                        if (textBox1.Text == null) throw new Exception("Please input Brush");
                        g.DrawRectangle(new Pen(new SolidBrush(paintcolor), Convert.ToInt32(textBox1.Text)), new Rectangle(x, y, e.X - x, e.Y - y));
                    }
                    else if (drawMode == DrawOperationType.Fill)
                        g.FillRectangle(new SolidBrush(paintcolor), new Rectangle(x, y, e.X - x, e.Y - y));
                    g.Dispose();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            draw = true;
            x = e.X;
            y = e.Y;
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void pictureBox5_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please choose Color and input Brush");
            currentItem = Items.Line;
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            currentItem = Items.Rectangle;
            MessageBox.Show("Please choose Color, input Brush and Type");
        }

        private void pictureBox6_Click(object sender, EventArgs e)
        {
            currentItem = Items.Ellipse;
            MessageBox.Show("Please choose Color, Brush and Type");
        }

        private void pictureBox7_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please choose Color and input Brush");
            currentItem = Items.Brush;
        }

        private void pictureBox8_Click(object sender, EventArgs e)
        {
            currentItem = Items.Pencil;
        }

        private void pictureBox9_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Please input Brush");
            currentItem = Items.Eraser;
        }

        private void pictureBox10_Click(object sender, EventArgs e)
        {
            pictureBox1.Refresh();
            pictureBox1.Image = null;
        }

        private void pictureBox11_Click(object sender, EventArgs e)
        {
            OpenFileDialog o = new OpenFileDialog();
            o.Filter = "Png files|*.png|jpeg files|*.jpg|bitmap|*.bmp";
            if (o.ShowDialog() == DialogResult.OK)
            {
                pictureBox1.Image = (Image)Image.FromFile(o.FileName).Clone();
            }
        }

        public void Invert()
        {
            Bitmap bitmap = new Bitmap(pictureBox1.Width, pictureBox1.Height);
            Graphics g = Graphics.FromImage(bitmap);
            Rectangle rectangle = pictureBox1.RectangleToScreen(pictureBox1.ClientRectangle);
            g.CopyFromScreen(rectangle.Location, Point.Empty, pictureBox1.Size);
            g.Dispose();
            progressBar1.Maximum = pictureBox1.Width;
            for (int x = 0; x < pictureBox1.Width; x++)
            {
                for (int y = 0; y < pictureBox1.Height; y++)
                {
                    Color tempcolor = bitmap.GetPixel(x, y);
                    tempcolor = Color.FromArgb(tempcolor.A, 0xFF - tempcolor.R, 0xFF - tempcolor.G, 0xFF - tempcolor.B);
                    bitmap.SetPixel(x, y, tempcolor);
                }
                Thread.Sleep(10);
                progressBar1.Value = x;
            }
            Graphics gr = pictureBox1.CreateGraphics();
            gr.DrawImage(bitmap, 0, 0, pictureBox1.Width, pictureBox1.Height);
            gr.Dispose();
            thread = new Thread(Run) { IsBackground = true };
        }

        private void pictureBox12_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap bmp = new Bitmap(pictureBox1.Width, pictureBox1.Height);
                Graphics g = Graphics.FromImage(bmp);
                Rectangle rectangle = pictureBox1.RectangleToScreen(pictureBox1.ClientRectangle);
                g.CopyFromScreen(rectangle.Location, Point.Empty, pictureBox1.Size);
                g.Dispose();
                SaveFileDialog s = new SaveFileDialog();
                s.Filter = "Png files|*.png|jpeg files|*.jpg|bitmap|*.bmp";
                if (s.ShowDialog() == DialogResult.OK)
                {
                    if (File.Exists(s.FileName))
                    {
                        File.Delete(s.FileName);
                    }
                    if (s.FileName.Contains(".jpg"))
                    {
                        bmp.Save(s.FileName, ImageFormat.Jpeg);
                    }
                    else if (s.FileName.Contains(".png"))
                    {
                        bmp.Save(s.FileName, ImageFormat.Png);
                    }
                    else if (s.FileName.Contains(".bmp"))
                    {
                        bmp.Save(s.FileName, ImageFormat.Bmp);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void red_Scroll(object sender, EventArgs e)
        {
            paintcolor = Color.FromArgb(alpha.Value, red.Value, green.Value, blue.Value);
            pictureBox3.BackColor = paintcolor;
        }

        private void blue_Scroll(object sender, EventArgs e)
        {
            paintcolor = Color.FromArgb(alpha.Value, red.Value, green.Value, blue.Value);
            pictureBox3.BackColor = paintcolor;
        }

        private void green_Scroll(object sender, EventArgs e)
        {
            paintcolor = Color.FromArgb(alpha.Value, red.Value, green.Value, blue.Value);
            pictureBox3.BackColor = paintcolor;
        }

        private void alpha_Scroll(object sender, EventArgs e)
        {
            paintcolor = Color.FromArgb(alpha.Value, red.Value, green.Value, blue.Value);
            pictureBox3.BackColor = paintcolor;
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
           
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox1.SelectedIndex==1)
            {
                drawMode = DrawOperationType.Fill;
            }
            else if (comboBox1.SelectedIndex == 0)
            {
                drawMode = DrawOperationType.Draw;
            }
        }
        public void Run()
        {
            try
            {
                var action = new Action(Invert);
                if (pictureBox1.InvokeRequired)
                    pictureBox1.Invoke(action);
                else Invert();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
        private void pictureBox13_Click(object sender, EventArgs e)
        {
            thread.Start();
        }

        private void toolTip1_Popup(object sender, PopupEventArgs e)
        {

        }

        private void pictureBox2_MouseUp(object sender, MouseEventArgs e)
        {
            choose = false;
        }

        private void pictureBox2_MouseDown(object sender, MouseEventArgs e)
        {
            choose = true;
        }

        private void pictureBox2_MouseMove(object sender, MouseEventArgs e)
        {
            try
            {
                if (choose)
                {
                    Bitmap bmp = (Bitmap)pictureBox2.Image.Clone();
                    paintcolor = bmp.GetPixel(e.X, e.Y);
                    red.Value = paintcolor.R;
                    blue.Value = paintcolor.B;
                    green.Value = paintcolor.G;
                    alpha.Value = paintcolor.A;
                    pictureBox3.BackColor = paintcolor;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
