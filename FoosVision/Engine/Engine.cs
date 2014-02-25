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
        }

        private Pitch m_Pitch;
        private Ball m_Ball;

        public Ball Ball { get { return m_Ball; } }
        private int m_FrameNumber = 0;

        public Image<Bgr, byte> DebugImage { get; set; }

        public void ProcessNextFrame(Image<Bgr, byte> tableImage)
        {
            m_FrameNumber++;
            DebugImage = tableImage.Copy();

            m_Ball.Update(tableImage);
            DebugImage.Draw(new CircleF(m_Ball.Pos, 10), new Bgr(70, 115, 255), 2);

             m_Pitch.Update(tableImage);

             DebugImage.Draw(new CircleF(m_Pitch.TopLeft, 12), new Bgr(0, 0, 200), 4);
             DebugImage.Draw(new CircleF(m_Pitch.TopRight, 12), new Bgr(0, 100, 200), 4);
             DebugImage.Draw(new CircleF(m_Pitch.BottomLeft, 12), new Bgr(100, 0, 200), 4);
             DebugImage.Draw(new CircleF(m_Pitch.BottomRight, 12), new Bgr(0, 200, 200), 4);
        }
    }
}
