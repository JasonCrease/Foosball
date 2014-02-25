using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class TableFinder
    {
        public Image<Bgr, Byte> DownImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> GrayImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> ThresholdImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> CannyImage
        {
            get;
            private set;
        }
        public LineSegment2D[] Lines
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> LinesImage
        {
            get;
            private set;
        }

        public System.Drawing.PointF[] GetTableCorners(Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> TableImage)
        {
            DownImage = TableImage.Convert<Bgr, Byte>().PyrDown().PyrDown();
            GrayImage = TableImage.Convert<Gray, Byte>().PyrDown().PyrDown();

            //ThresholdImage = ImageProcess.ThresholdHsv(DownImage, 0, 23, 40, 143, 118, 256);    // For wood
            ThresholdImage = ImageProcess.ThresholdHsv(DownImage, 22, 89, 33, 135, 42, 201);  // For green
            ThresholdImage = ThresholdImage.Erode(8);

            //Image<Gray, Byte> FilteredImage = GrayImage.Copy(ThresholdImage);

            // Do canny filter
            CannyImage = ThresholdImage.Canny(100.0, 60.0);

            List<PointF> points = new List<PointF>();

            for (int x = 0; x < CannyImage.Width; x++)
                for (int y = 0; y < CannyImage.Height; y++)
                    if (CannyImage.Data[y, x, 0] > 100) points.Add(new PointF(x, y));

            var box = PointCollection.MinAreaRect(points.ToArray());

            box.center.X *= 4;
            box.center.Y *= 4;
            box.size.Height *= 4;
            box.size.Width *= 4;

            LinesImage = TableImage.Copy();
            LinesImage.Draw(box, new Bgr(Color.GreenYellow), 10);

            return null;

            //// Do Edge finder
            Lines = CannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
               Math.PI / 360, //Angle resolution measured in radians.
                50, //threshold
                30, //min Line width
                40 //gap between lines
                )[0]; //Get the lines from the first channel


            //LinesImage = FilteredImage.Convert<Bgr, byte>();
            foreach (LineSegment2D line in Lines)
                LinesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 2);

            return null;
        }
    }
}
