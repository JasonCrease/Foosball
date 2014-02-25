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

        public PointF m_OldTLPoint = PointF.Empty;
        public PointF m_OldTRPoint = PointF.Empty;
        public PointF m_OldBLPoint = PointF.Empty;
        public PointF m_OldBRPoint = PointF.Empty;

        public PointF[] GetTableCorners(Image<Bgr, byte> nowImage)
        {

            int scale = 2;
            int offset = 4;
            DownImage = nowImage.Convert<Bgr, Byte>().PyrDown();
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


            if (m_OldTLPoint == Point.Empty)
            {
                m_OldBLPoint = bottomLeft;
                m_OldBRPoint = bottomRight;
                m_OldTLPoint = topLeft;
                m_OldTRPoint = topRight;
            }
            else
            {
                const float rate = 0.2f;
                bottomLeft.X = (bottomLeft.X * rate) + (m_OldBLPoint.X * (1 - rate));
                bottomLeft.Y = (bottomLeft.Y * rate) + (m_OldBLPoint.Y * (1 - rate));
                bottomRight.X = (bottomRight.X * rate) + (m_OldBRPoint.X * (1 - rate));
                bottomRight.Y = (bottomRight.Y * rate) + (m_OldBRPoint.Y * (1 - rate));
                topLeft.X = (topLeft.X * rate) + (m_OldTLPoint.X * (1 - rate));
                topLeft.Y = (topLeft.Y * rate) + (m_OldTLPoint.Y * (1 - rate));
                topRight.X = (topRight.X * rate) + (m_OldTRPoint.X * (1 - rate));
                topRight.Y = (topRight.Y * rate) + (m_OldTRPoint.Y * (1 - rate));

                m_OldBLPoint = bottomLeft;
                m_OldBRPoint = bottomRight;
                m_OldTLPoint = topLeft;
                m_OldTRPoint = topRight;
            }

            return new PointF[] { topLeft, topRight, bottomLeft, bottomRight };
        }
    }
}
