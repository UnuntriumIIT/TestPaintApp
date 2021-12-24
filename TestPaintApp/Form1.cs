using System;
using System.Drawing;
using System.Windows.Forms;

namespace TestPaintApp
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            initGraphics();
            WindowState = FormWindowState.Maximized;
        }

        private bool IsClicked = false;
        private ArPoints arPoints = new ArPoints(2);
        private Bitmap image = new Bitmap(1, 1);
        private Graphics g;
        private Pen p = new Pen(Color.Black, 3f);

        private void initGraphics()
        {
            Rectangle rectangle = Screen.PrimaryScreen.Bounds;
            image = new Bitmap(rectangle.Width, rectangle.Height);
            g = Graphics.FromImage(image);
            p.StartCap = System.Drawing.Drawing2D.LineCap.Round;
            p.EndCap = System.Drawing.Drawing2D.LineCap.Round;
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Image = image;
        }

        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            IsClicked = true;
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            IsClicked = false;
            arPoints.Reset();
        }

        private void pictureBox1_MouseMove(object sender, MouseEventArgs e)
        {
            if (!IsClicked) { return; }
            arPoints.SetPoint(e.X, e.Y);
            if (arPoints.GetCount() >= 2)
            {
                g.DrawLines(p, arPoints.GetPoints());
                pictureBox1.Image = image;
                arPoints.SetPoint(e.X, e.Y);
            }
        }

        private void button4_Click(object sender, EventArgs e)
        {
            if (colorDialog1.ShowDialog() == DialogResult.OK) 
            {
                p.Color = colorDialog1.Color;
                ((Button)sender).BackColor = colorDialog1.Color;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            g.Clear(pictureBox1.BackColor);
            pictureBox1.Image = image;
        }

        private void trackBar1_ValueChanged(object sender, EventArgs e)
        {
            p.Width = trackBar1.Value;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            saveFileDialog1.Filter = "PNG(*.PNG)|*.png";
            if(saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if(pictureBox1.Image != null)
                {
                    pictureBox1.Image.Save(saveFileDialog1.FileName);
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            openFileDialog1.Filter = "PNG(*.PNG)|*.png";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                var inputFile = Image.FromFile(openFileDialog1.FileName);
                g.DrawImage(inputFile, new Point(0, 0));
                pictureBox1.Image = image;
            }
        }

        private void button6_Click(object sender, EventArgs e)
        {
            var temp = (Bitmap)image.Clone();
            var bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    /*
                     * The RGB values are converted to grayscale using the NTSC formula: 
                     * gray = 0.299 * Red + 0.587 * Green + 0.114 * Blue. 
                     * This formula closely represents the average person's relative perception 
                     * of the brightness of red, green, and blue light.
                     */
                    byte gray = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);

                    bmap.SetPixel(i, j, Color.FromArgb(gray, gray, gray));
                }
            }
            g.DrawImage(bmap, new Point(0, 0));
            pictureBox1.Image = image;
        }

        private void button5_Click(object sender, EventArgs e)
        {
            var temp = (Bitmap)image.Clone();
            var bmap = (Bitmap)temp.Clone();
            Color c;
            for (int i = 0; i < bmap.Width; i++)
            {
                for (int j = 0; j < bmap.Height; j++)
                {
                    c = bmap.GetPixel(i, j);
                    byte gray = (byte)(0.299 * c.R + 0.587 * c.G + 0.114 * c.B);
                    Color newColor = Color.FromArgb(gray, gray, gray);

                    byte r = (byte)(1 * newColor.R);
                    byte g = (byte)(0.95 * newColor.G);
                    byte b = (byte)(0.82 * newColor.B);

                    bmap.SetPixel(i, j, Color.FromArgb(r,g,b));
                }
            }
            g.DrawImage(bmap, new Point(0, 0));
            pictureBox1.Image = image;
        }

        private void button7_Click(object sender, EventArgs e)
        {
            var temp = (Bitmap)image.Clone();
            var bmap = (Bitmap)temp.Clone();
            var result = Rotation.RotateImage(bmap, 90f);
            g.DrawImage(result, new Point(0, 0));
            pictureBox1.Image = image;
        }
    }

    public static class Rotation
    {
        public static Bitmap RotateImage(Bitmap bmp, float angle)
        {
            float alpha = angle;

            //edit: negative angle +360
            while (alpha < 0) alpha += 360;

            float gamma = 90;
            float beta = 180 - angle - gamma;

            float c1 = bmp.Height;
            float a1 = (float)(c1 * Math.Sin(alpha * Math.PI / 180) / Math.Sin(gamma * Math.PI / 180));
            float b1 = (float)(c1 * Math.Sin(beta * Math.PI / 180) / Math.Sin(gamma * Math.PI / 180));

            float c2 = bmp.Width;
            float a2 = (float)(c2 * Math.Sin(alpha * Math.PI / 180) / Math.Sin(gamma * Math.PI / 180));
            float b2 = (float)(c2 * Math.Sin(beta * Math.PI / 180) / Math.Sin(gamma * Math.PI / 180));

            int width = Convert.ToInt32(b2 + a1);
            int height = Convert.ToInt32(b1 + a2);

            Bitmap rotatedImage = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(rotatedImage))
            {
                g.TranslateTransform(rotatedImage.Width / 2, rotatedImage.Height / 2); //set the rotation point as the center into the matrix
                g.RotateTransform(angle); //rotate
                g.TranslateTransform(-rotatedImage.Width / 2, -rotatedImage.Height / 2); //restore rotation point into the matrix
                g.DrawImage(bmp, new Point((width - bmp.Width) / 2, (height - bmp.Height) / 2)); //draw the image on the new bitmap
            }
            return rotatedImage;
        }
    }

    public class ArPoints
    {
        private int index = 0;
        private Point[] points;

        public ArPoints(int size)
        {
            if (size < 2) { throw new Exception("Error on creating ArPoints!"); }
            points = new Point[size];
        }

        public void SetPoint(int x, int y)
        {
            if (index >= points.Length) { index = 0; }
            points[index] = new Point(x, y);
            index++;
        }

        public void Reset()
        {
            index = 0;
        }

        public int GetCount()
        {
            return index;
        }

        public Point[] GetPoints()
        {
            return points;
        }
    }
}
