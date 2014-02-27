using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Pitch
    {
        public const float PitchWidth = 1205;
        public const float PitchHeight = 705;
        public const float Border = 30;
        public const float Instep = 166;

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
        public Image<Gray, Byte> ThresholdImage
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

        public PointF TopLeft = PointF.Empty;
        public PointF TopRight = PointF.Empty;
        public PointF BottomLeft = PointF.Empty;
        public PointF BottomRight = PointF.Empty;

        public HomographyMatrix WarpMatrix { get { return m_WarpMat; } }
        private HomographyMatrix m_WarpMat;

        public void Update(Image<Bgr, byte> nowImage)
        {
            DerivePitchEdges(nowImage);

            TopLeft = GetTopLeft();
            TopRight = GetTopRight();
            BottomLeft = GetBottomLeft();
            BottomRight = GetBottomRight();

            PointF[] sourcePoints = { TopLeft, TopRight, BottomLeft, BottomRight };
            PointF[] destPoints = { 
                                      new PointF(Border + Instep, Border), 
                                      new PointF(PitchWidth  + (Border * 2) - Instep, Border) ,
                                      new PointF(Border + Instep, PitchHeight + (Border * 2)), 
                                      new PointF(PitchWidth  + (Border * 2) - Instep, PitchHeight + (Border * 2)) };

            m_WarpMat = CameraCalibration.GetPerspectiveTransform(sourcePoints, destPoints);
        }

        private Queue<LineSegment2D> m_TopBorderCandidateLines = new Queue<LineSegment2D>();
        private Queue<LineSegment2D> m_BottomBorderCandidates = new Queue<LineSegment2D>();

        private void DerivePitchEdges(Image<Bgr, Byte> image)
        {
            Image<Bgr, byte> imageMasked = image.Copy(ImageProcess.ThresholdHsv(image, 22, 89, 33, 240, 40, 240));

            Image<Gray, byte> imageCanny = imageMasked.Canny(170, 130);

            var lines = imageCanny.HoughLinesBinary(1, Math.PI / 360, 30, 200, 50)[0];

            //Image<Bgr, byte> retImage = imageCanny.Convert<Bgr, byte>();

            foreach (var line in lines)
            {
                float dirY = line.Direction.Y;

                if (line.Length > 500 && dirY < 0.3 && dirY > -0.3f)
                {
                    if (line.P1.Y < image.Height / 2 && line.P2.Y < image.Height / 2)
                    {
                        m_TopBorderCandidateLines.Enqueue(line);
                        if(m_TopBorderCandidateLines.Count > 20)
                            m_TopBorderCandidateLines.Dequeue();
                        //retImage.Draw(line, new Bgr(Color.Red), 2);
                    }
                    else if (line.P1.Y > image.Height / 2 && line.P2.Y > image.Height / 2)
                    {
                        m_BottomBorderCandidates.Enqueue(line);
                        if (m_BottomBorderCandidates.Count > 20)
                            m_BottomBorderCandidates.Dequeue();
                        //retImage.Draw(line, new Bgr(Color.YellowGreen), 2);
                    }
                }
            }
        }

        private PointF GetTopLeft()
        {
            if (m_TopBorderCandidateLines.Count == 0) return PointF.Empty;
            return new PointF((float)m_TopBorderCandidateLines.Average(x => x.P1.X), (float)m_TopBorderCandidateLines.Average(x => x.P1.Y));
        }
        private PointF GetTopRight()
        {
            if (m_TopBorderCandidateLines.Count == 0) return PointF.Empty;
            return new PointF((float)m_TopBorderCandidateLines.Average(x => x.P2.X), (float)m_TopBorderCandidateLines.Average(x => x.P2.Y));
        }
        private PointF GetBottomLeft()
        {
            if (m_BottomBorderCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)m_BottomBorderCandidates.Average(x => x.P1.X), (float)m_BottomBorderCandidates.Average(x => x.P1.Y));
        }
        private PointF GetBottomRight()
        {
            if (m_BottomBorderCandidates.Count == 0) return PointF.Empty;
            return new PointF((float)m_BottomBorderCandidates.Average(x => x.P2.X), (float)m_BottomBorderCandidates.Average(x => x.P2.Y));
        }
    }
}
