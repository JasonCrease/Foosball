using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Engine
    {
        public Engine()
        {
            m_PreviousBallPositions = new List<PointF>();
        }

        private int m_BallPosCount;
        public double BallSpeed { get; private set; }
        public Image<Bgr, byte> TableImage { get; set; }
        private List<PointF> m_PreviousBallPositions;

        public void Process()
        {
            //PointF[] tableCorners = Table.FindTable(TableImage);
            PointF ballPos = Ball.GetPosition(TableImage);

            CircleF circle = new CircleF(ballPos, 10);
            TableImage.Draw(circle, new Bgr(70, 115, 255), 2);

            if (ballPos != PointF.Empty)
            {
                m_BallPosCount++;
                m_PreviousBallPositions.Add(ballPos);
            }

            if (m_BallPosCount > 2)
                SetBallSpeed();
        }

        private void SetBallSpeed()
        {
            PointF prevPoint = m_PreviousBallPositions[m_BallPosCount - 2];
            PointF thisPoint = m_PreviousBallPositions[m_BallPosCount - 1];

            double answer = Math.Sqrt(Math.Pow(prevPoint.X - thisPoint.X, 2) + Math.Pow(prevPoint.Y - thisPoint.Y, 2));

            BallSpeed = Math.Min(answer, 100f);
        }
    }
}
