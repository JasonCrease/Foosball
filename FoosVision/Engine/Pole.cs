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

            return new LineSegment2DF(new PointF(avgX1, avgY1), new PointF(avgX3, avgY3));
        }

        public bool IsFound
        {
            get
            {
                return m_Pts[2].Count > 0 && m_Pts[4].Count > 0 && m_Pts[1].Count > 0 && m_Pts[5].Count > 0;
            }
        }
    }

}