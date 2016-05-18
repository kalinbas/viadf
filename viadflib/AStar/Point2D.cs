using System;

namespace viadflib.AStar
{
    [Serializable]
    public class Point2D
    {
        double[] _Coordinates = new double[2];


        public Point2D(double[] Coordinates)
        {
            X = Coordinates[0]; Y = Coordinates[1];
        }

        public Point2D(double CoordinateX, double CoordinateY)
        {
            X = CoordinateX; Y = CoordinateY;
        }

        public double this[int CoordinateIndex]
        {
            get { return _Coordinates[CoordinateIndex]; }
            set { _Coordinates[CoordinateIndex] = value; }
        }

        public double X { set { _Coordinates[0] = value; } get { return _Coordinates[0]; } }

        public double Y { set { _Coordinates[1] = value; } get { return _Coordinates[1]; } }

        public static double DistanceBetween(Point2D P1, Point2D P2)
        {
            return Math.Sqrt((P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y));
        }

        public static double SquaredDistanceBetween(Point2D P1, Point2D P2)
        {
            return (P1.X - P2.X) * (P1.X - P2.X) + (P1.Y - P2.Y) * (P1.Y - P2.Y);
        }
    
        public override bool Equals(object Point)
        {
            Point2D P = (Point2D)Point;
            if (P == null) throw new ArgumentException("Object must be of type " + GetType());
            bool res = true;
            for (int i = 0; i < 2; i++) res &= P[i].Equals(this[i]);
            return res;
        }
      
        public override int GetHashCode()
        {
            double HashCode = 0;
            for (int i = 0; i < 2; i++) HashCode += this[i];
            return (int)HashCode;
        }
    
        public override string ToString()
        {
            string Deb = "{";
            string Sep = ";";
            string Fin = "}";
            string Resultat = Deb;
            int Dimension = 2;
            for (int i = 0; i < Dimension; i++)
                Resultat += _Coordinates[i].ToString() + (i != Dimension - 1 ? Sep : Fin);
            return Resultat;
        }
    }
}
