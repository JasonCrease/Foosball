using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    public class Ball
    {
        public Ball()
        {
            m_PreviousBallPositions = new List<PointF>();

            m_KalmanFilter = new Kalman(4, 2, 0);
            m_KalmanFilter.CorrectedState = new Matrix<float>(new float[] { 0f, 0f, 0f, 0f });
            m_KalmanFilter.TransitionMatrix = new Matrix<float>(new float[,] { { 1f, 0, 1, 0 }, { 0, 1f, 0, 1 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } });
            m_KalmanFilter.MeasurementNoiseCovariance = new Matrix<float>(new float[,] { { 0.001f, 0 }, { 0, 0.001f } });
            m_KalmanFilter.ProcessNoiseCovariance = new Matrix<float>(new float[,] { { 0.0001f, 0, 0, 0 }, { 0, 0.0001f, 0, 0 }, { 0, 0, 0.0001f, 0 }, { 0, 0, 0, 0.0001f } });
            m_KalmanFilter.ErrorCovariancePost = new Matrix<float>(new float[,] { { 1f, 0, 0, 0 }, { 0, 1f, 0, 0 }, { 0, 0, 0f, 0 }, { 0, 0, 0, 1f } });
            m_KalmanFilter.MeasurementMatrix = new Matrix<float>(new float[,] { { 1f, 0, 0, 0 }, { 0, 1f, 0, 0 } });
        }

        public PointF Pos;
        public PointF RelPos;
        public double Speed { get; private set; }

        internal void Update(Image<Bgr, byte> image)
        {
            Image<Gray, byte> threshImage;
            bool pointObserved = false;

            // Try looking in local window
            if (Pos != Point.Empty)
            {
                int width = 120;
                int height = 60;
                int x = Math.Max(1, (int)Pos.X - (width / 2));
                int y = Math.Max(1, (int)Pos.Y - (height / 2));
                if (x +width > image.Width) width = image.Width - x-1;
                if (y + height > image.Height ) height = image.Height - y-1;

                if (x < 0 || x + width > image.Width || y < 0 || y + height > image.Height
                    || width < 1 || height < 1)
                {
                    goto notGoodSearch;
                }

                Image<Bgr, byte> searchWindow = image.GetSubRect(new Rectangle(x, y, width, height));
                threshImage = ImageProcess.ThresholdHsv(searchWindow, 14, 37, 150, 256, 98, 256);
                PointF searchPos = ImageProcess.GetCentreOfMass(threshImage.Erode(1).Dilate(1).Erode(1), 4);

                if (searchPos != Point.Empty)
                {
                    pointObserved = true;
                    Pos = new PointF(x + searchPos.X, y + searchPos.Y);
                }
            }

            notGoodSearch:

            if (Pos == Point.Empty || pointObserved == false) // If window fails to find point, then search the whole image
            {
                threshImage = ImageProcess.ThresholdHsv(image, 17, 34, 150, 256, 98, 256);
                PointF searchPos = ImageProcess.GetCentreOfMass(threshImage.Erode(2).Dilate(1).Erode(1), 6);
                if (searchPos != Point.Empty)
                {
                    pointObserved = true;
                    Pos = searchPos;
                }
            }

            Matrix<float> prediction = m_KalmanFilter.Predict();
            Matrix<float> measured = new Matrix<float>(new float[,] { { Pos.X }, { Pos.Y } });

            if (pointObserved)
            {
                Matrix<float> estimated = m_KalmanFilter.Correct(measured);
                Pos = new PointF(estimated[0, 0], estimated[1, 0]);
            }
            else
            {
                Pos = new PointF(prediction[0, 0], prediction[1, 0]);
            }

            if (Pos.X < 1) Pos.X = 1;
            if (Pos.Y < 1) Pos.Y = 1;
            if (Pos.X > 1279) Pos.X = 1279;
            if (Pos.Y > 719) Pos.Y = 719;
        }


        internal void AddPosition(PointF ballPos)
        {
            if (ballPos != PointF.Empty)
            {
                m_BallPosCount++;
                m_PreviousBallPositions.Add(ballPos);
            }

            if (m_BallPosCount > 2)
                SetBallSpeed();
        }


        private void SetBallSpeed()
        {
            PointF prevPoint = m_PreviousBallPositions[m_BallPosCount - 2];
            PointF thisPoint = m_PreviousBallPositions[m_BallPosCount - 1];

            double answer = Math.Sqrt(Math.Pow(prevPoint.X - thisPoint.X, 2) + Math.Pow(prevPoint.Y - thisPoint.Y, 2)) * 0.029 * 2.2;

            Speed = ((answer * 0.4) + (Speed * 0.6));
        }

        private int m_BallPosCount;
        private List<PointF> m_PreviousBallPositions;
        private Kalman m_KalmanFilter;

        internal void SetBallRealPosition(HomographyMatrix warpMatrix)
        {
            PointF[] pointArray = new PointF[1];
            pointArray[0] = this.Pos;
            warpMatrix.ProjectPoints(pointArray);
            this.RelPos = pointArray[0];

            if (this.RelPos.X < -50) RelPos.X = -50;
            if (this.RelPos.X > Pitch.PitchWidth + 50) RelPos.X = Pitch.PitchWidth + 50;
            if (this.RelPos.Y < 0) RelPos.Y = 0;
            if (this.RelPos.Y > Pitch.PitchHeight + 50) RelPos.Y = Pitch.PitchHeight + 50;

            if (RelPosIsOnTable())
                AddPosition(RelPos);
        }
        private bool RelPosIsOnTable()
        {
            return (RelPos.X > 0 && RelPos.X < Pitch.PitchWidth && RelPos.Y > 0 && RelPos.Y < Pitch.PitchHeight);
        }


        public override string ToString()
        {
            return string.Format("Speed {0}mph at ({1}, {2})", Speed.ToString("#.#"), RelPos.X.ToString("###"), RelPos.Y.ToString("###"));
        }
    }

}
