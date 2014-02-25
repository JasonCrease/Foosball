using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Ball
    {
        public Ball()
        {
            m_PreviousBallPositions = new List<PointF>();
        }

        public PointF Pos { get; private set; }
        public double Speed { get; private set; }

        internal void Update(Image<Bgr, byte> image)
        {
            Image<Gray, byte> threshImage = ImageProcess.ThresholdHsv(image, 17, 32, 130, 256, 118, 256);
            Pos = ImageProcess.GetCentreOfMass(threshImage);
            AddPosition(Pos);
        }

        internal void AddPosition(PointF ballPos)
        {
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

            Speed = Math.Min(answer, 100f);
        }

        private int m_BallPosCount;
        private List<PointF> m_PreviousBallPositions;
    }
}
