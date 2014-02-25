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
            int scale = 2;
            int offset = 4;
            DownImage = TableImage.Convert<Bgr, Byte>().PyrDown();
            //GrayImage = TableImage.Convert<Gray, Byte>().PyrDown().PyrDown();

            //ThresholdImage = ImageProcess.ThresholdHsv(DownImage, 0, 23, 40, 143, 118, 256);    // For wood
            ThresholdImage = ImageProcess.ThresholdHsv(DownImage, 22, 89, 33, 135, 42, 201);  // For green
            ThresholdImage = ThresholdImage.Erode(1);
            ThresholdImage = ThresholdImage.Dilate(1);

            PointF topLeft = PointF.Empty;
            PointF topRight = PointF.Empty;
            PointF bottomLeft = PointF.Empty;
            PointF bottomRight = PointF.Empty;

            int widthOver2 = ThresholdImage.Width / 2;
            int heightOver2 = ThresholdImage.Height / 2;

            for (int startY = 0; startY < ThresholdImage.Height; startY++)
                for (int x = 0; x < startY; x++)
                {
                    int y = startY - x;
                    if (ThresholdImage.Data[y, x, 0] > 0)
                    {
                        topLeft = new PointF((x + offset) * scale, (y - offset) * scale);
                        goto next1;
                    }
                }
            next1:

            for (int startY = 0; startY < ThresholdImage.Height; startY++)
                for (int x = 0; x < startY; x++)
                {
                    int y = startY - x;
                    int x2 = ThresholdImage.Width - x - 1;
                    if (ThresholdImage.Data[y, x2, 0] > 0)
                    {
                        topRight = new PointF((x2 - offset) * scale, (y - offset) * scale);
                        goto next2;
                    }
                }
            next2:

            for (int startY = 0; startY < ThresholdImage.Height; startY++)
                for (int x = 0; x < startY; x++)
                {
                    int y = ThresholdImage.Height - (startY - x) - 1;
                    if (ThresholdImage.Data[y, x, 0] > 0)
                    {
                        bottomLeft = new PointF((x + offset) * scale, (y + offset) * scale);
                        goto next3;
                    }
                }
            next3:

            for (int startY = 0; startY < ThresholdImage.Height; startY++)
                for (int x = 0; x < startY; x++)
                {
                    int y = ThresholdImage.Height - (startY - x) - 1;
                    int x2 = ThresholdImage.Width - x - 1;
                    if (ThresholdImage.Data[y, x2, 0] > 0)
                    {
                        bottomRight = new PointF((x2 - offset) * scale, (y + offset) * scale);
                        goto next4;
                    }
                }
            next4:

            ThresholdImage.Draw(new CircleF(topLeft, 10), new Gray(190), 4);
            ThresholdImage.Draw(new CircleF(topRight, 11), new Gray(190), 6);
            ThresholdImage.Draw(new CircleF(bottomLeft, 12), new Gray(190), 8);
            ThresholdImage.Draw(new CircleF(bottomRight, 13), new Gray(190), 10);

            return new PointF[] { topLeft, topRight, bottomLeft, bottomRight };
        }
    }
}
