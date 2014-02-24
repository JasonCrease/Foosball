using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    class Ball
    {
        internal static PointF GetPosition(Image<Bgr, byte> image)
        {
            Image<Gray, byte> threshImage = ImageProcess.ThresholdHsv(image, 17, 32, 130, 256, 118, 256);
            return ImageProcess.GetCentreOfMass(threshImage);
        }
    }
}
