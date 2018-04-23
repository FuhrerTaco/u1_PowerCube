/**
 * Nolan Meehan
 * April 23, 2018
 * Draws a rectangle around the yellow Cube.
 */
 using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace u1_PowerCubeNolan
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Image im = new Image();
        public MainWindow()
        {
            InitializeComponent();
            canvas.Children.Add(im);

        }

        BitmapImage image; 
        private void btnImageSelect_Click(object sender, RoutedEventArgs e)
        {
            int iLast = 0;
            OpenFileDialog ofd = new OpenFileDialog();
            List<Rect> r = new List<Rect>();
            if (ofd.ShowDialog()==true)
            {
                image = new BitmapImage(new Uri(ofd.FileName, UriKind.Absolute));
                int stride = image.PixelWidth * 4;
                int size = image.PixelHeight * stride;
                byte[] imagePixels = new byte[size];
                image.CopyPixels(imagePixels, stride, 0);
                List<Point> l = new List<Point>();
                for (int i = 0; i < imagePixels.Length; i+=4)
                {
                    int red = imagePixels[i + 2];
                    int blue = imagePixels[i];
                    int green = imagePixels[i + 1];
                    float[] hsv = new float[3];
                    int min = Math.Min(red, blue);
                    min = Math.Min(min, green);
                    int max = Math.Max(red, blue);
                    max = Math.Max(max, green);
                    hsv[2] = max;
                    float delta = max - min;
                    if(delta<1)
                    {
                        hsv[0] = 0;
                        hsv[1] = 1;
                    }
                    if (max > 0) hsv[1] = delta / max;
                    else
                    {
                        hsv[0] = 0;
                    }
                    if (red >= max) hsv[0] = (green - blue) / delta;
                    else if (green >= max) hsv[0] = 2 + (blue - red) / delta;
                    else hsv[0] = 4 + (red - green) / delta;
                            hsv[0] *= 60;
                    if (hsv[0] < 0.0) hsv[0] += 360;

                    if (hsv[0] > 50&& hsv[0] < 70 && hsv[1] > 0.6 && hsv[2] > 150)
                    {
                        imagePixels[i + 2]= 255;
                        imagePixels[i]= 255;
                        imagePixels[i + 1]= 255;
                        if (l.Count == 0)
                        {
                            int x =(int) ((i / 4) % image.PixelWidth);
                            int y = (int) ((i / 4) / image.PixelHeight);
                            l.Add(new Point(x,y));
                            iLast = i;
                        }
                        else
                        {
                            if (Math.Sqrt(Math.Pow((i / 4) % image.PixelWidth - (iLast / 4) % image.PixelWidth, 2)  + Math.Pow((i / 4) / image.PixelHeight - (iLast / 4) / image.PixelHeight, 2)) <600)
                            {
                                l.Add(new Point((i / 4) % image.PixelWidth, (i / 4) / image.PixelHeight));
                                iLast = i;
                            }
                            else
                            {
                                r.Add(getRect(l));
                                l.Clear();
                            }
                        }
                    }
                    else
                    {
                        imagePixels[i + 2] = 0;
                        imagePixels[i] = 0;
                        imagePixels[i + 1] = 0;
                    }


                }
                r.Add(getRect(l));
                l.Clear();
                BitmapSource bs = BitmapSource.Create(image.PixelWidth, image.PixelHeight, image.DpiX, image.DpiY, PixelFormats.Bgr32, null, drawRect(r,imagePixels), stride);
                BitmapImage filteredImage = ByteArrayToImage(bs);
                //test.Source = filteredImage;
                im.Source = filteredImage;
                im.RenderTransform = new ScaleTransform(0.25, 0.25);
                FileStream sw = new FileStream("test.jpg",FileMode.Create);
                JpegBitmapEncoder encoder = new JpegBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bs));
                encoder.Save(sw);
                sw.Close();
            }
        }

        private BitmapImage ByteArrayToImage(BitmapSource bs)
        {
            JpegBitmapEncoder encoder = new JpegBitmapEncoder();
            MemoryStream memoryStream = new MemoryStream();
            BitmapImage bImg = new BitmapImage();

            encoder.Frames.Add(BitmapFrame.Create(bs));
            encoder.Save(memoryStream);

            memoryStream.Position = 0;
            bImg.BeginInit();
            bImg.StreamSource = new MemoryStream(memoryStream.ToArray());
            bImg.EndInit();

            memoryStream.Close();
            return bImg;
        }

        private Rect getRect(List<Point> l)
        {
            if (l.Count == 0) return new Rect(0,0,0,0);
            int xLow , xHigh, yLow,yHigh;
            yHigh = yLow =(int) l.ToArray()[0].Y;
            xHigh = xLow = (int)l.ToArray()[0].X;
            foreach (Point p in l)
            {
                if (p.X < xLow) xLow = (int)p.X;
                if (p.X > xHigh) xHigh = (int)p.X;
                if (p.Y < yLow) yLow = (int)p.Y;
                if (p.Y > yHigh) yHigh = (int) p.Y;
            }
            return new Rect(xLow, yLow, xHigh - xLow, yHigh - yLow);
        }

        byte[] drawRect(List<Rect> r, byte[] b)
        {
            for (int i = 0; i < b.Length; i += 4)
            {
                foreach(Rect re in r)
                {
                    if((re.Left <=i/4%image.PixelWidth&&re.Right>= i / 4 % image.PixelWidth && (re.Top==i/4 / image.PixelHeight || re.Bottom==i/4 / image.PixelHeight)) || ((i/4 / image.PixelHeight >= re.Top&&re.Bottom>=i/4/image.PixelHeight)&&(re.Left== i / 4 % image.PixelWidth || re.Right== i / 4 % image.PixelWidth)))
                    {
                        b[i] = 0;
                        b[i + 1] = 255;
                        b[i + 2] = 0;
                    }
                }
            }
                return b;
        }

    }
}
