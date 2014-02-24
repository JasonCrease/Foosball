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
        }

        public Image<Bgr, byte> TableImage { get; set; }

        public void Process()
        {
            //PointF[] tableCorners = Table.FindTable(TableImage);
            PointF ballPos = Ball.GetPosition(TableImage);

            CircleF circle = new CircleF(ballPos, 10);
            TableImage.Draw(circle, new Bgr(70, 115, 255), 2);
        }
    }
}
