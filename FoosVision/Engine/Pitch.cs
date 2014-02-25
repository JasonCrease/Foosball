using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Pitch
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

        public PointF TopLeft;
        public PointF TopRight;
        public PointF BottomLeft;
        public PointF BottomRight;

        private PointF m_OldTLPoint = PointF.Empty;
        private PointF m_OldTRPoint = PointF.Empty;
        private PointF m_OldBLPoint = PointF.Empty;
        private PointF m_OldBRPoint = PointF.Empty;

        public void Update(Image<Bgr, byte> nowImage)
        {
            FindPitchCorners(nowImage);

            if (m_OldTLPoint == Point.Empty)
            {
                m_OldBLPoint = BottomLeft;
                m_OldBRPoint = BottomRight;
                m_OldTLPoint = TopLeft;
                m_OldTRPoint = TopRight;
            }
            else
            {
                const float rate = 0.2f;
                BottomLeft.X = (BottomLeft.X * rate) + (m_OldBLPoint.X * (1 - rate));
                BottomLeft.Y = (BottomLeft.Y * rate) + (m_OldBLPoint.Y * (1 - rate));
                BottomRight.X = (BottomRight.X * rate) + (m_OldBRPoint.X * (1 - rate));
                BottomRight.Y = (BottomRight.Y * rate) + (m_OldBRPoint.Y * (1 - rate));
                TopLeft.X = (TopLeft.X * rate) + (m_OldTLPoint.X * (1 - rate));
                TopLeft.Y = (TopLeft.Y * rate) + (m_OldTLPoint.Y * (1 - rate));
                TopRight.X = (TopRight.X * rate) + (m_OldTRPoint.X * (1 - rate));
                TopRight.Y = (TopRight.Y * rate) + (m_OldTRPoint.Y * (1 - rate));

                m_OldBLPoint = BottomLeft;
                m_OldBRPoint = BottomRight;
                m_OldTLPoint = TopLeft;
                m_OldTRPoint = TopRight;
            }
        }

        private void FindPitchCorners(Image<Bgr, byte> nowImage)
        {

            int scale = 2;
            int offset = 4;
            DownImage = nowImage.Convert<Bgr, Byte>().PyrDown();
            ThresholdImage = ImageProcess.ThresholdHsv(DownImage, 22, 89, 33, 135, 20, 201);  // For green
            ThresholdImage = ThresholdImage.Erode(1);
            ThresholdImage = ThresholdImage.Dilate(1);

            TopLeft = PointF.Empty;
            TopRight = PointF.Empty;
            BottomLeft = PointF.Empty;
            BottomRight = PointF.Empty;

            int widthOver2 = ThresholdImage.Width / 2;
            int heightOver2 = ThresholdImage.Height / 2;

            for (int startY = 0; startY < ThresholdImage.Height; startY++)
                for (int x = 0; x < startY; x++)
                {
                    int y = startY - x;
                    if (ThresholdImage.Data[y, x, 0] > 0)
                    {
                        TopLeft = new PointF((x + offset) * scale, (y - offset) * scale);
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
                        TopRight = new PointF((x2 - offset) * scale, (y - offset) * scale);
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
                        BottomLeft = new PointF((x + offset) * scale, (y + offset) * scale);
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
                        BottomRight = new PointF((x2 - offset) * scale, (y + offset) * scale);
                        goto next4;
                    }
                }
        next4:

            ;
        }
    }
}
