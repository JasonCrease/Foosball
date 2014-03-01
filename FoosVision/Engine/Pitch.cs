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
        public Pole[] Poles = new Pole[8];
        public const float PitchWidth = 1205;
        public const float PitchHeight = PitchWidth / 2;
        public const float Border = 30;
        public const float Instep = 170;

        public float[] PoleRelPositions = { 61, 213, 380, 537, 694, 840, 1008, 1164 };

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
        public Image<Gray, Byte> ThresholdedImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> CannyImage
        {
            get;
            private set;
        }
        public Image<Bgr, Byte> MaskedImage
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
        public Image<Bgr, Byte> DebugImage
        {
            get;
            set;
        }
        public Image<Bgr, Byte> PerspImage
        {
            get;
            set;
        }
        public Image<Gray, Byte> ThresholdedPerspImage
        {
            get;
            private set;
        }
        public Image<Gray, Byte> ThresholdedCannyImage
        {
            get;
            private set;
        }

        public Pitch()
        {
            for (int i = 0; i < 8; i++ )
                Poles[i] = new Pole(i);
            Poles[0].SearchX = 61;
            Poles[1].SearchX = 213;
            Poles[2].SearchX = 380;
            Poles[3].SearchX = 537;
            Poles[4].SearchX = 694;
            Poles[5].SearchX = 840;
            Poles[6].SearchX = 1008;
            Poles[7].SearchX = 1164;;
        }

        public PointF TopLeft = PointF.Empty;
        public PointF TopRight = PointF.Empty;
        public PointF BottomLeft = PointF.Empty;
        public PointF BottomRight = PointF.Empty;

        public HomographyMatrix WarpMatrix { get { return m_WarpMat; } }
        private HomographyMatrix m_WarpMat;
        private HomographyMatrix m_WarpMatInv;

        public void Update(Image<Bgr, byte> nowImage)
        {
            DebugImage = nowImage;
            DerivePitchEdges(nowImage);

            TopLeft = GetTopLeft();
            TopRight = GetTopRight();
            BottomLeft = GetBottomLeft();
            BottomRight = GetBottomRight();

            PointF[] sourcePoints = { TopLeft, TopRight, BottomLeft, BottomRight };
            PointF[] destPoints = { 
                                      new PointF(Instep, Border), 
                                      new PointF(PitchWidth - Instep, Border) ,
                                      new PointF(Instep, PitchHeight + Border), 
                                      new PointF(PitchWidth - Instep, PitchHeight + Border) };

            m_WarpMat = CameraCalibration.GetPerspectiveTransform(sourcePoints, destPoints);
            m_WarpMatInv = CameraCalibration.GetPerspectiveTransform(destPoints, sourcePoints);

            PerspImage = nowImage.WarpPerspective(m_WarpMat, 1205, (int)(PitchHeight + Border * 2),
                Emgu.CV.CvEnum.INTER.CV_INTER_CUBIC,
                Emgu.CV.CvEnum.WARP.CV_WARP_FILL_OUTLIERS,
                new Bgr(200, 200, 200)).Convert<Bgr, byte>();
            ThresholdedPerspImage = ImageProcess.ThresholdHsv(PerspImage, 22, 89, 33, 240, 40, 250).
                ThresholdBinaryInv(new Gray(100), new Gray(255));

            DerivePolePositions();
        }

        private void DerivePolePositions()
        {
            // Use a convolution to sharpen it. The centre needs to be about 
            int kernelHeight = 1;
            int kernelWidth = 32;
            float kernelTotalWeight = 0;
            float kernelSize = kernelWidth * kernelHeight;
            float[,] kernelValues = new float[kernelHeight, kernelWidth];

            for (int i = 0; i < kernelHeight; i++)
                for (int j = 0; j < kernelWidth; j++)
                {
                    float weight = 0f;
                    int distFromCentre = Math.Abs(j - (kernelWidth / 2 - 1));

                    if (distFromCentre < 3)
                        weight = Math.Abs(distFromCentre) * 20;
                    else if (distFromCentre < 10)
                        weight = 0;
                    else
                        weight = -Math.Abs(distFromCentre);

                    kernelValues[i, j] = weight;
                    kernelTotalWeight += weight;
                }

            for (int i = 0; i < kernelHeight; i++)
                for (int j = 0; j < kernelWidth; j++)
                {
                    kernelValues[i, j] = kernelValues[i, j] / Math.Abs(kernelTotalWeight);
                }

            ConvolutionKernelF kernel = new ConvolutionKernelF(kernelValues);

            Image<Gray, float> convImage = ThresholdedPerspImage.Convolution(kernel);

            int[] sliceYs = new int[] { 50, 150, 220, 320, 410, 500, 600 };

            //DebugImage = ThresholdedPerspImage.Convert<Bgr, byte>();
            DebugImage = PerspImage;

            for (int poleNum = 0; poleNum < 8; poleNum++)
                for (int sliceNum = 0; sliceNum < 7; sliceNum++)
                    for (int mx = Poles[poleNum].SearchX - 20; mx < Poles[poleNum].SearchX + 20; mx++)
                    {
                        int y = sliceYs[sliceNum];
                        float intensity = convImage.Data[y, mx, 0];
                        if (intensity > 400)
                        {
                            //CircleF circ = new CircleF(new PointF(mx, y), 3);
                            //DebugImage.Draw(circ, new Bgr(0, intensity / 3, 150), 2);
                            Poles[poleNum].AddPoint(sliceNum, new PointF(mx, sliceYs[sliceNum]));
                        }
                    }

            for (int i = 0; i < 8; i++)
            {
                if (Poles[i].IsFound)
                {
                    Poles[i].FindMen(PerspImage);
                    var line = Poles[i].CalcLine();
                    DebugImage.Draw(line, new Bgr(20, 250, 50), 4);
                    
                }
            }
        }

        private Queue<LineSegment2D> m_TopBorderCandidateLines = new Queue<LineSegment2D>();
        private Queue<LineSegment2D> m_BottomBorderCandidates = new Queue<LineSegment2D>();

        private void DerivePitchEdges(Image<Bgr, Byte> image)
        {
            ThresholdedImage = ImageProcess.ThresholdHsv(image, 22, 89, 33, 240, 40, 250);
            MaskedImage = image.Copy(ThresholdedImage);

            CannyImage = MaskedImage.Canny(200, 100);

            var lines = CannyImage.HoughLinesBinary(1, Math.PI / 360, 100, 200, 50)[0];

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
                    }
                    else if (line.P1.Y > image.Height / 2 && line.P2.Y > image.Height / 2)
                    {
                        m_BottomBorderCandidates.Enqueue(line);
                        if (m_BottomBorderCandidates.Count > 20)
                            m_BottomBorderCandidates.Dequeue();
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
