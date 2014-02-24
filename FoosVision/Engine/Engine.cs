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
            //TableImage = new Image<Bgr, byte>(TableImagePath).Resize(1280, 720, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, false);

            Image<Hsv, Byte> hsvImg = TableImage.Convert<Hsv, Byte>();
            Channels = hsvImg.Split();
            Channels[0] = Channels[0].InRange(new Gray(hmin), new Gray(hmax));
            Channels[1] = Channels[1].InRange(new Gray(smin), new Gray(smax));
            Channels[2] = Channels[2].InRange(new Gray(vmin), new Gray(vmax));

            var andImage = Channels[0].And(Channels[1]).And(Channels[2]);
            andImage = andImage.Erode(2);
            //andImage = andImage.Dilate(3);

            //TableImage = andImage.Convert<Bgr, byte>();
            //return;

            long totalMassX = 0;
            long totalMassY = 0;
            long totalMass = 1;

            for (int x = 0; x < 1280; x += 1)
                for (int y = 0; y < 720; y += 1)
                {
                    if (andImage.Data[y, x, 0] > 0)
                    {
                        totalMassX += x;
                        totalMassY += y;
                        totalMass++;
                    }
                }

            float comX = totalMassX / totalMass;
            float comY = totalMassY / totalMass;

            CircleF circle = new CircleF(new PointF(comX, comY), 10);
            TableImage.Draw(circle, new Bgr(70, 115, 255), 2);


            Channels = null;
            //TableImage = null;
        }
    }
}
