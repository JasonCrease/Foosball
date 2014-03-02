using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Pole
    {
        public int[] SliceYs = new int[2];
        public int SearchX { get; set; }
        private int m_PoleIndex;

        public Pole(int poleIndex)
        {
            m_PoleIndex = poleIndex;

            m_Pts = new Queue<System.Drawing.PointF>[7];
            for (int i = 0; i < 2; i++)
                m_Pts[i] = new Queue<PointF>();
        }

        private Queue<PointF>[] m_Pts;

        public void AddPoint(int index, PointF point)
        {
            if (m_Pts[index].Count > 10)
                m_Pts[index].Dequeue();
            m_Pts[index].Enqueue(point);
        }

        float m_XStart, m_XEnd;

        public LineSegment2DF CalcLine()
        {
            float avgX0 = 0f, avgX1 = 0f, avgY0 = 0f, avgY1 = 0f;

            avgX0 = m_Pts[0].Average(x => x.X);
            avgY0 = m_Pts[0].Average(x => x.Y);
            avgX1 = m_Pts[1].Average(x => x.X);
            avgY1 = m_Pts[1].Average(x => x.Y);

            if (avgX0 == avgX1)
            {
                m_XStart = avgX0;
                m_XEnd = avgX1;
            }
            else
            {
                float gradient = (avgX1 - avgX0) / (avgY1 - avgY0);
                m_XStart = avgX0 - (avgY0 * gradient);
                m_XEnd = m_XStart + (650 * gradient);
            }

            return new LineSegment2DF(new PointF(m_XStart, 0), new PointF(m_XEnd, 650));
        }

        public bool IsFound
        {
            get
            {
                return m_Pts[1].Count > 0 && m_Pts[0].Count > 0;
            }
        }

        internal void FindMen(Image<Bgr, byte> perspImage)
        {
            Image<Hsv, byte> perspImageHsv = perspImage.Convert<Hsv, byte>();

            int height = 660;
            Image<Bgr, byte> stripBgr = new Image<Bgr, byte>(1, height);
            Image<Hsv, byte> stripHsv = new Image<Hsv, byte>(1, height);
            Image<Gray, byte> thresholded;
            float gradient = (m_XStart - m_XEnd) / height;

            string str = "";

            for (int y = 0; y < height; y++)
            {
                int x = (int)((gradient * y) + m_XStart);

                stripBgr.Data[y, 0, 0] = perspImage.Data[y, x, 0];
                stripBgr.Data[y, 0, 1] = perspImage.Data[y, x, 1];
                stripBgr.Data[y, 0, 2] = perspImage.Data[y, x, 2];
                stripHsv.Data[y, 0, 0] = perspImageHsv.Data[y, x, 0];
                stripHsv.Data[y, 0, 1] = perspImageHsv.Data[y, x, 1];
                stripHsv.Data[y, 0, 2] = perspImageHsv.Data[y, x, 2];

                str += stripBgr.Data[y, 0, 0] + "," + stripBgr.Data[y, 0, 1] + "," + stripBgr.Data[y, 0, 2] + "," +
                        stripHsv.Data[y, 0, 0] + "," + stripHsv.Data[y, 0, 1] + "," + stripHsv.Data[y, 0, 2] + "\r\n";
            }

            thresholded = ImageProcess.ThresholdHsv(stripBgr, 0, 23, 45, 98, 100, 256);
        }
    }

}