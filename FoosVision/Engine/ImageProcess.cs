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
        public static Image<Gray, byte> ThresholdHsv(Image<Bgr, Byte> image,  int hmin, int hmax, int smin, int smax, int vmin, int vmax)
        {
            Image<Hsv, Byte> hsvImg = image.Convert<Hsv, Byte>();
            Image<Gray, Byte>[] channels = hsvImg.Split();
            channels[0] = channels[0].InRange(new Gray(hmin), new Gray(hmax));
            channels[1] = channels[1].InRange(new Gray(smin), new Gray(smax));
            channels[2] = channels[2].InRange(new Gray(vmin), new Gray(vmax));

            return channels[0].And(channels[1]).And(channels[2]);
        }

        static List<LineSegment2D> TopEdgeCandidates = new List<LineSegment2D>();
        static List<LineSegment2D> BottomEdgeCandidates = new List<LineSegment2D>();

        public static Image<Bgr, byte> PitchEdges(Image<Bgr, Byte> image)
        {
            Image<Bgr, byte> imageMasked = image.Copy(ImageProcess.ThresholdHsv(image, 22, 89, 33, 240, 40, 240));

            Image<Gray, byte> imageCanny = imageMasked.Canny(170, 130);

            var lines = imageCanny.HoughLinesBinary(1, Math.PI / 360, 30, 200, 50)[0];

            //Image<Bgr, byte> retImage = imageCanny.Convert<Bgr, byte>();

            foreach (var line in lines)
            {
                float dirY = line.Direction.Y;

                if (line.Length > 500 && dirY < 0.3 && dirY  > -0.3f)
                {
                    if (line.P1.Y < image.Height / 2 && line.P2.Y < image.Height / 2)
                    {
                        TopEdgeCandidates.Add(line);
                        //retImage.Draw(line, new Bgr(Color.Red), 2);
                    }
                    else if (line.P1.Y > image.Height / 2 && line.P2.Y > image.Height / 2)
                    {
                        BottomEdgeCandidates.Add(line);
                        //retImage.Draw(line, new Bgr(Color.YellowGreen), 2);
                    }
                }
            }

            return null;
        }

        public static PointF GetTopLeft()
        {
            if (TopEdgeCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)TopEdgeCandidates.Average(x => x.P1.X), (float)TopEdgeCandidates.Average(x => x.P1.Y));
        }
        public static PointF GetTopRight()
        {
            if (TopEdgeCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)TopEdgeCandidates.Average(x => x.P2.X), (float)TopEdgeCandidates.Average(x => x.P2.Y));
        }
        public static PointF GetBottomLeft()
        {
            if (BottomEdgeCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)BottomEdgeCandidates.Average(x => x.P1.X), (float)BottomEdgeCandidates.Average(x => x.P1.Y));
        }
        public static PointF GetBottomRight()
        {
            if (BottomEdgeCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)BottomEdgeCandidates.Average(x => x.P2.X), (float)BottomEdgeCandidates.Average(x => x.P2.Y));
        }

        public static PointF GetCentreOfMass(Image<Gray, byte> image, int boundary)
        {
            int imageWidth = image.Width;
            int imageHeight = image.Height;
            int totalMassX = 0;
            int totalMassY = 0;
            int totalMass = 0;

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

            if (totalMass < boundary)          // heuristically, this means too few pixels to come to a good conclusion
                return PointF.Empty;

            float comX = totalMassX / totalMass;
            float comY = totalMassY / totalMass;

            return new PointF(comX, comY);
        }
    }
}
