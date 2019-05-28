using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class Point
    {
        double _x, _y;
        int precision = 5;
        double eps = 0.01;

        public double X
        {
            get
            { return _x; }
            set
            { _x = Math.Round(value, precision); }
        }

        public double Y
        {
            get
            { return _y; }
            set
            { _y = Math.Round(value, precision); }
        }

        public Point() { }

        public Point(double x, double y)
        {
            X = Math.Round(x, precision);
            Y = Math.Round(y, precision);
        }

        public Point(Point p)
        {
            X = p.X;
            Y = p.Y;
        }

        public void Set(double x, double y)
        {
            X = Math.Round(x, precision);
            Y = Math.Round(y, precision);
        }

        public double[] GetAsArray()
        {
            return new double[2] { X, Y };
        }

        public double Norm()
        {
            return Math.Sqrt(Math.Pow(X, 2) + Math.Pow(Y, 2));
        }

        public Point Normalize()
        {
            Point normal;
            double N = this.Norm();
            
            if (N>0)
                normal = new Point(this.X/N, this.Y/N);
            else
                normal = new Point(this);

            return normal;
        }

        public double Dist(Point d)
        {
            return Math.Sqrt(Math.Pow(X - d.X, 2) + Math.Pow(Y - d.Y, 2));
        }

        public bool Equals(Point P)
        {
            if ((Math.Abs(this.X - P.X) > eps) || (Math.Abs(this.Y - P.Y) > eps))
                return false;
            return true;
        }

        public double Angle()
        {
            return Math.Atan2(this.X, this.Y);
        }

        public override string ToString()
        {
            return "(" + X.ToString() + "," + Y.ToString() + ")";
        }

        public static Point operator+(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }

        public static Point operator-(Point p1, Point p2)
        {
            return new Point(p1.X - p2.X, p1.Y - p2.Y);
        }
    }
}
