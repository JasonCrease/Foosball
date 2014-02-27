﻿using System;
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

        public PointF TopLeft;
        public PointF TopRight;
        public PointF BottomLeft;
        public PointF BottomRight;

        private PointF m_OldTLPoint = PointF.Empty;
        private PointF m_OldTRPoint = PointF.Empty;
        private PointF m_OldBLPoint = PointF.Empty;
        private PointF m_OldBRPoint = PointF.Empty;

        public HomographyMatrix WarpMatrix { get { return m_WarpMat; } }
        private HomographyMatrix m_WarpMat;

        public void Update(Image<Bgr, byte> nowImage)
        {
            //if (m_OldTLPoint == Point.Empty)    
            //{
            //    m_OldBLPoint = BottomLeft;
            //    m_OldBRPoint = BottomRight;
            //    m_OldTLPoint = TopLeft;
            //    m_OldTRPoint = TopRight;
            //}
            //else
            //{
            //    const float rate = 0.2f;
            //    BottomLeft.X = (BottomLeft.X * rate) + (m_OldBLPoint.X * (1 - rate));
            //    BottomLeft.Y = (BottomLeft.Y * rate) + (m_OldBLPoint.Y * (1 - rate));
            //    BottomRight.X = (BottomRight.X * rate) + (m_OldBRPoint.X * (1 - rate));
            //    BottomRight.Y = (BottomRight.Y * rate) + (m_OldBRPoint.Y * (1 - rate));
            //    TopLeft.X = (TopLeft.X * rate) + (m_OldTLPoint.X * (1 - rate));
            //    TopLeft.Y = (TopLeft.Y * rate) + (m_OldTLPoint.Y * (1 - rate));
            //    TopRight.X = (TopRight.X * rate) + (m_OldTRPoint.X * (1 - rate));
            //    TopRight.Y = (TopRight.Y * rate) + (m_OldTRPoint.Y * (1 - rate));

            //    m_OldBLPoint = BottomLeft;
            //    m_OldBRPoint = BottomRight;
            //    m_OldTLPoint = TopLeft;
            //    m_OldTRPoint = TopRight;
            //}

            float border = 30;
            float instep = 166;

            ImageProcess.PitchEdges(nowImage);

            TopLeft = ImageProcess.GetTopLeft();
            TopRight = ImageProcess.GetTopRight();
            BottomLeft = ImageProcess.GetBottomLeft();
            BottomRight = ImageProcess.GetBottomRight();

            PointF[] sourcePoints = { TopLeft, TopRight, BottomLeft, BottomRight };
            PointF[] destPoints = { 
                                      new PointF(border + instep, border), 
                                      new PointF(PitchWidth  + (border * 2) - instep, border) ,
                                      new PointF(border + instep, PitchHeight + (border * 2)), 
                                      new PointF(PitchWidth  + (border * 2) - instep, PitchHeight + (border * 2)) };

            m_WarpMat = CameraCalibration.GetPerspectiveTransform(sourcePoints, destPoints);
        }
    }
}
