﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace CoreSampleAnnotation.AnnotationPlane.Columns
{
    public static class Drawing
    {
        public static IEnumerable<Point> GetPolygon(double width, double height, ISideCurveGenerator sideCurve)
        {
            List<Point> result = new List<Point>();
            result.Add(new Point(0.0, 0.0));
            result.Add(new Point(width, 0.0));

            IEnumerable<Point> sidePoints = sideCurve.GenerateSide(height).
                Select(p => new Point(p.Y + width, p.X)); // transposing, so that (0.0;0.0);(length;0.0) projected to (width;0.0);(width;length) 

            result.AddRange(sidePoints);

            result.Add(new Point(width, height));
            result.Add(new Point(0.0, height));
            return result;
        }
    }

    public interface ISideCurveGenerator
    {
        /// <summary>
        /// Generates a set of points along horizontal (Y = 0) axis from X=0 up to X=length
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        IEnumerable<Point> GenerateSide(double length);
    }

    public class StraightSideCurveGenerator : ISideCurveGenerator
    {
        public IEnumerable<Point> GenerateSide(double length)
        {
            return new Point[] { new Point(0.0, 0.0), new Point(length, 0.0) };
        }
    }

    /// <summary>
    /// Generates one period of the signal
    /// </summary>
    public interface IOscillationGenerator
    {
        IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY);
    }

    /// <summary>
    /// Generates a side curve by repeating integer repeating periods provided by oscillationGenerator
    /// </summary>
    public class OscillatingSignalCurveGenerator : ISideCurveGenerator
    {
        private double xPeriod, signalMaxY;
        IOscillationGenerator generator;

        /// <param name="xPeriod">The period of the pattern allong X axis</param>
        /// <param name="signalMaxY">the avsolute hight of the pattern along Y axis</param>
        /// <param name="generator">Which signal to generate</param>
        public OscillatingSignalCurveGenerator(double xPeriod, double signalMaxY, IOscillationGenerator generator)
        {
            if (signalMaxY < 0)
                throw new ArgumentException("signalMaxY must be non-negative");
            this.xPeriod = xPeriod;
            this.signalMaxY = signalMaxY;
            this.generator = generator;
        }

        public IEnumerable<Point> GenerateSide(double length)
        {
            int periods = (int)Math.Floor(length / xPeriod);
            double straitEndsLength = length - periods * xPeriod;

            List<Point> result = new List<Point>();
            //starting straight line
            result.Add(new Point(0.0, 0.0));
            result.Add(new Point(straitEndsLength * 0.5, 0.0));

            //drawing curves
            double miniStep = xPeriod * 0.5;
            for (int i = 0; i < periods; i++)
            {
                double xOffset = straitEndsLength * 0.5 + i * xPeriod;
                IEnumerable<Point> periodPoints = generator.GeneratePeriod(xPeriod, signalMaxY).Select(p => new Point(p.X + xOffset, p.Y));

                result.AddRange(periodPoints);
            }

            //drawing trailing strait line
            result.Add(new Point(length, 0.0));
            return result;
        }
    }

    public class StepOscillationGenerator : IOscillationGenerator
    {
        public IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY)
        {
            double halfPeriod = xPeriod * 0.5;
            List<Point> result = new List<Point>();
            result.Add(new Point(0.0, signalMaxY));
            result.Add(new Point(0.0 + halfPeriod, signalMaxY));
            result.Add(new Point(0.0 + halfPeriod, -signalMaxY));
            result.Add(new Point(0.0 + xPeriod, -signalMaxY));
            result.Add(new Point(0.0 + xPeriod, 0.0));
            return result;
        }
    }

    public class SinOscillationGenerator : IOscillationGenerator
    {
        private int points;
        /// <param name="approxPointsCount">How many points to use for sin approximation</param>
        public SinOscillationGenerator(int approxPointsCount)
        {
            if (approxPointsCount < 2)
                throw new ArgumentException("approxPointsCount must be 2 or higher");
            this.points = approxPointsCount;
        }

        public IEnumerable<Point> GeneratePeriod(double xPeriod, double signalMaxY)
        {
            double step = Math.PI * 2.0 / (points - 1);
            Point[] result = new Point[points];

            double xScale = xPeriod / (Math.PI * 2.0);

            for (int i = 0; i < points; i++)
            {
                double arg = step * (i + 1);
                result[i] = new Point(arg * xScale, Math.Sin(arg)* signalMaxY);
            }
            return result;
        }
    }    
}
