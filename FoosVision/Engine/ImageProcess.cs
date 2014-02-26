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
            Image<Hsv, Byte> hsvImg = image.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] channels = hsvImg.Split();
            channels[0] = channels[0].InRange(new Gray(hmin), new Gray(hmax));
            channels[1] = channels[1].InRange(new Gray(smin), new Gray(smax));
            channels[2] = channels[2].InRange(new Gray(vmin), new Gray(vmax));

            return channels[0].And(channels[1]).And(channels[2]);
        }

        public static PointF GetCentreOfMass(Image<Gray, byte> image)
        {
            image = image.Erode(3);

            long imageWidth = image.Width;
            long imageHeight = image.Height;
            long totalMassX = 0;
            long totalMassY = 0;
            long totalMass = 0;

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

            if (totalMass < 3)          // heuristically, this means too few pixels to come to a good conclusion
                return PointF.Empty;

            float comX = totalMassX / totalMass;
            float comY = totalMassY / totalMass;

            return new PointF(comX, comY);
        }
    }
}
