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

        public string TableImagePath { get; set; }
        public Image<Bgr, byte> TableImage { get; set; }
        public Image<Bgr, byte> TableImageMasked { get; set; }
        public Image<Gray, byte>[] Channels { get; set; }
        public Image<Gray, byte> ChannelsTogether { get; set; }

        public void Process(int hmin, int hmax, int smin, int smax, int vmin, int vmax)
        {
            Image<Gray, byte> threshImage = ImageProcess.ThresholdHsv(TableImage, hmin, hmax, smin, smax, vmin, vmax);
            PointF centrePoint = ImageProcess.GetCentreOfMass(threshImage);

            CircleF circle = new CircleF(centrePoint, 10);
            TableImage.Draw(circle, new Bgr(70, 115, 255), 2);
        }
    }
}
