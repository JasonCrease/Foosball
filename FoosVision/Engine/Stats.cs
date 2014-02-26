﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace Engine
{
    public class Stats
    {
        Ball b;
        List<BallDesc> ballDescs = new List<BallDesc>();

        public Stats()
        {

        }

        public void AddBall(Ball b)
        {
            ballDescs.Add(new BallDesc(){ RelPos = b.RelPos, Speed = b.Speed });
        }

        public string GetPossessionSummary()
        {
            float[] boundaries = { 0, 250, 425, 600, 775, 950, 1205 };
            float redPossession = 0;
            float whitePossession = 0;

            foreach (BallDesc ballDesc in ballDescs)
            {
                for (int i = 0; i < boundaries.Length - 1; i++)
                {
                    if (ballDesc.RelPos.X > boundaries[i] && ballDesc.RelPos.X < boundaries[i + 1])
                    {
                        if (i % 2 == 0)
                            whitePossession++;
                        else
                            redPossession++;
                    }
                }
            }

            float totalPossession = redPossession + whitePossession;
            float redPossesionPerc = redPossession * 100f / totalPossession;
            float whitePossesionPerc = whitePossession * 100f / totalPossession;

            return string.Format("Red: {0}% White: {1}%", redPossesionPerc.ToString("##"), whitePossesionPerc.ToString("##"));
        }

        public double GetHighSpeed()
        {
            return ballDescs.Max(x => x.Speed);
        }
    }

    public class BallDesc
    {
        public PointF RelPos { get; set; }
        public double Speed { get; set; }
    }
}
