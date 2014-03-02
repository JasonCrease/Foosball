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
        public int SearchX { get; set; }
        private int m_PoleIndex;

        public Pole(int poleIndex)
        {
            m_PoleIndex = poleIndex;

            m_Pts = new Queue<System.Drawing.PointF>[7];
            for (int i = 0; i < 7; i++)
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
            float avgX1, avgX3, avgY1, avgY3;

            if (m_PoleIndex == 0 || m_PoleIndex == 7)
            {
                avgX1 = m_Pts[2].Average(x => x.X);
                avgX3 = m_Pts[4].Average(x => x.X);
                avgY1 = m_Pts[2].Average(x => x.Y);
                avgY3 = m_Pts[4].Average(x => x.Y);
            }
            else
            {
                avgX1 = m_Pts[1].Average(x => x.X);
                avgX3 = m_Pts[5].Average(x => x.X);
                avgY1 = m_Pts[1].Average(x => x.Y);
                avgY3 = m_Pts[5].Average(x => x.Y);
            }

            if (avgX1 == avgX3)
            {
                m_XStart = avgX3;
                m_XEnd = avgX3;
            }
            else
            {
                float gradient = (avgX3 - avgX1) / (avgY3 - avgY1);
                m_XStart = avgX1 - (avgY1 * gradient);
                m_XEnd = m_XStart + (650 * gradient);
            }

            return new LineSegment2DF(new PointF(m_XStart, 0), new PointF(m_XEnd, 650));
        }

        public bool IsFound
        {
            get
            {
                return m_Pts[2].Count > 0 && m_Pts[4].Count > 0 && m_Pts[1].Count > 0 && m_Pts[5].Count > 0;
            }
        }

        internal void FindMen(Image<Bgr, byte> perspImage)
        {
            int height = 660;
            Image<Bgr, byte> stripBgr = new Image<Bgr, byte> (1, height);
            Image<Gray, byte> thresholded;
            float gradient = (m_XStart - m_XEnd) / height ;

            for (int y = 0; y < height; y++)
            {
                int x = (int)((gradient * y) + m_XStart);

                stripBgr.Data[y, 0, 0] = perspImage.Data[y, x, 0];
                stripBgr.Data[y, 0, 1] = perspImage.Data[y, x, 1];
                stripBgr.Data[y, 0, 2] = perspImage.Data[y, x, 2];
            }

            thresholded = ImageProcess.ThresholdHsv(stripBgr, 0, 23, 45, 98, 100, 256);
        }
    }

}