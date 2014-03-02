using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Emgu.CV;
using Emgu.CV.Structure;
using System.Drawing;

namespace Engine
{
    class PoleSet
    {
        public Pole[] m_Poles = new Pole[8];

        public Image<Bgr, byte> DebugImage { get; set; }

        public PoleSet()
        {
            for (int i = 0; i < 8; i++)
                m_Poles[i] = new Pole(i);
            m_Poles[0].SearchX = 61;
            m_Poles[1].SearchX = 213;
            m_Poles[2].SearchX = 380;
            m_Poles[3].SearchX = 537;
            m_Poles[4].SearchX = 694;
            m_Poles[5].SearchX = 840;
            m_Poles[6].SearchX = 1008;
            m_Poles[7].SearchX = 1164;

            m_Poles[0].SliceYs[0] = 220;
            m_Poles[0].SliceYs[1] = 410;
            m_Poles[7].SliceYs[0] = 220;
            m_Poles[7].SliceYs[1] = 410;

            for (int poleNum = 1; poleNum < 7; poleNum++)
            {
                m_Poles[poleNum].SliceYs[0] = 150;
                m_Poles[poleNum].SliceYs[1] = 550;
            }

            BuildConvolution();
        }

        private void BuildConvolution()
        {
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

            m_KernelPole = new ConvolutionKernelF(kernelValues);
        }

        ConvolutionKernelF m_KernelPole;
        ConvolutionKernelF m_KernelMan;

        public void FindPoles(Image<Gray, byte> thresholdedPerspImage, Image<Bgr, byte> perspImage)
        {
            // Use a convolution to sharpen it such that the centre of the bar is a peak
            Image<Gray, float> convImage = thresholdedPerspImage.Convolution(m_KernelPole);

            //DebugImage = ThresholdedPerspImage.Convert<Bgr, byte>();
            DebugImage = perspImage;

            for (int poleNum = 0; poleNum < 8; poleNum++)
            {
                Pole pole = m_Poles[poleNum];
                for (int sliceNum = 0; sliceNum < 2; sliceNum++)
                    for (int mx = pole.SearchX - 20; mx < pole.SearchX + 20; mx++)
                    {
                        int y = pole.SliceYs[sliceNum];
                        float intensity = convImage.Data[y, mx, 0];
                        if (intensity > 400)
                        {
                            CircleF circ = new CircleF(new PointF(mx, y), 3);
                            DebugImage.Draw(circ, new Bgr(0, intensity / 3, 150), 2);
                            m_Poles[poleNum].AddPoint(sliceNum, new PointF(mx, pole.SliceYs[sliceNum]));
                        }
                    }
            }

            //return;

            for (int poleNum = 0; poleNum < 8; poleNum++)
            {
                Pole pole = m_Poles[poleNum];
                if (pole.IsFound)
                {
                    pole.FindMen(perspImage);
                    var line = pole.CalcLine();
                    DebugImage.Draw(line, new Bgr(20, 250, 50), 4);
                }
            }
        }
    }
}
