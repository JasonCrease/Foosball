using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class ImageProcess
    {
        public static Image<Gray, byte> ThresholdHsv(Image<Bgr, Byte> image, int hmin, int hmax, int smin, int smax, int vmin, int vmax)
        {
            //TableImage = new Image<Bgr, byte>(TableImagePath).Resize(1280, 720, Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC, false);

            Image<Hsv, Byte> hsvImg = image.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] Channels = hsvImg.Split();
            Channels[0] = Channels[0].InRange(new Gray(hmin), new Gray(hmax));
            Channels[1] = Channels[1].InRange(new Gray(smin), new Gray(smax));
            Channels[2] = Channels[2].InRange(new Gray(vmin), new Gray(vmax));

            return Channels[0].And(Channels[1]).And(Channels[2]);
        }

        public static PointF GetCentreOfMass(Image<Gray, byte> image)
        {
            image = image.Erode(2);

            long imageWidth = image.Width;
            long imageHeight = image.Height;
            long totalMassX = 0;
            long totalMassY = 0;
            long totalMass = 1;

            for (int x = 0; x < imageWidth; x += 1)
                for (int y = 0; y < imageHeight; y += 1)
                {
                    if (image.Data[y, x, 0] > 0)
                    {
                        totalMassX += x;
                        totalMassY += y;
                        totalMass++;
                    }
                }

            float comX = totalMassX / totalMass;
            float comY = totalMassY / totalMass;

            if (totalMass < 5)      // heuristically, this means too few pixels to come to a good conclusion
                return new PointF(0, 0);
            else
                return new PointF(comX, comY);
        }
    }
}
