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
            m_Pitch = new Pitch();
            m_Ball = new Ball();
            Stats = new Stats();
        }

        public Stats Stats { get; private set; }
        private Pitch m_Pitch;
        private Ball m_Ball;

        public Ball Ball { get { return m_Ball; } }
        private int m_FrameNumber = 0;
        
        public Image<Bgr, byte> DebugImage { get; set; }

        public void ProcessNextFrame(Image<Bgr, byte> tableImage)
        {
            DebugImage = tableImage.Copy();

            // Updating 

            m_Ball.Update(tableImage);
            if (m_FrameNumber % 5 == 0)
                m_Pitch.Update(tableImage);
            m_Ball.SetBallRealPosition(m_Pitch.WarpMatrix);
            Stats.AddBall(m_Ball);

            // Drawing 

            if (m_Ball.Pos.X > 0 && m_Ball.Pos.Y > 0 && m_Ball.Pos.X < tableImage.Width && m_Ball.Pos.Y < tableImage.Height)
                DebugImage.Draw(new CircleF(m_Ball.Pos, 12), new Bgr(Color.MediumBlue), 5);

            DebugImage.Draw(new CircleF(m_Pitch.TopLeft, 12), new Bgr(0, 0, 200), 4);
            DebugImage.Draw(new CircleF(m_Pitch.TopRight, 12), new Bgr(0, 100, 200), 4);
            DebugImage.Draw(new CircleF(m_Pitch.BottomLeft, 12), new Bgr(100, 0, 200), 4);
            DebugImage.Draw(new CircleF(m_Pitch.BottomRight, 12), new Bgr(0, 200, 200), 4);

            //DebugImage = DebugImage.WarpPerspective(m_Pitch.WarpMatrix, 1205, 800,
            //    Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
            //    Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS, new Bgr(Color.Wheat));

            m_FrameNumber++;
        }

    }
}
