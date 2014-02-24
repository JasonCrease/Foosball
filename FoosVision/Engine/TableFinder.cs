using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class TableFinder
    {
        public Image<Bgr, Byte> DownImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> GrayImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> CannyImage
        {
            get;
            private set;
        }
        public LineSegment2D[] Lines
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> LinesImage
        {
            get;
            private set;
        }

        public System.Drawing.PointF[] GetTableCorners(Emgu.CV.Image<Emgu.CV.Structure.Bgr, byte> TableImage)
        {
            DownImage = TableImage.Convert<Bgr, Byte>().PyrDown().PyrDown();

            //GrayImage = ImageProcess.ThresholdHsv(DownImage, 22, 89, 33, 135, 42, 201);
            GrayImage = ImageProcess.ThresholdHsv(DownImage, 0, 23, 40, 143, 118, 256);

            GrayImage = GrayImage.Erode(1);

            // Do canny filter
            CannyImage = GrayImage.Canny(70.0, 30.0);

            // Do Edge finder
            Lines = CannyImage.HoughLinesBinary(
                1, //Distance resolution in pixel-related units
                Math.PI / 360.0, //Angle resolution measured in radians.
                20, //threshold
                50, //min Line width
                10 //gap between lines
                )[0]; //Get the lines from the first channel

            LinesImage = DownImage.CopyBlank();
            foreach (LineSegment2D line in Lines)
                LinesImage.Draw(line, new Bgr(System.Drawing.Color.Red), 1);

            return null;
        }
    }
}
