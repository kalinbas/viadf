using System;

namespace viadflib.TravelTime.KDTree
{
    public class Vector
    {
        private double[] _vectorArray;
        private int _dimension;

        public Vector(params double[] values)
        {
            _vectorArray = values;
            _dimension = values.Length;
        }

        public Vector(int dim)
        {
            _vectorArray = new double[dim];
            _dimension = dim;
            Zero();
        }

        public void Zero()
        {
            for (int i = 0; i < _dimension; i++)
            {
                _vectorArray[i] = 0;
            }
        }

        public double Abs()
        {
            double abs = 0;
            for (int i = 0; i < _dimension; i++)
            {
                abs += _vectorArray[i] * _vectorArray[i];
            }
            return Math.Sqrt(abs);
        }

        public double Dot(Vector vector)
        {
            if (vector._dimension != _dimension)
            {
                return 0;
            }
            double dotProd = 0;
            for (int i = 0; i < _dimension; i++)
            {
                dotProd += _vectorArray[i] * vector._vectorArray[i];
            }
            return dotProd;
        }

        public Vector Scale(double factor)
        {
            double[] values = new double[_dimension];

            for (int i = 0; i < _dimension; i++)
            {
                values[i] = _vectorArray[i] * factor;
            }
            return new Vector(values);
        }

        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < _dimension; i++)
            {
                if (i != 0)
                {
                    str += ", ";
                }
                str += _vectorArray[i];
            }
            return "(" + str + ")";
        }


        public static Vector operator +(Vector left, Vector right)
        {
            if (left._dimension != right._dimension)
            {
                return left;
            }
            double[] values = new double[left._dimension];

            for (int i = 0; i < left._dimension; i++)
            {
                values[i] = left._vectorArray[i] + right._vectorArray[i];
            }
            return (new Vector(values));
        }

        public static Vector operator -(Vector left, Vector right)
        {
            if (left._dimension != right._dimension)
            {
                return left;
            }
            double[] values = new double[left._dimension];

            for (int i = 0; i < left._dimension; i++)
            {
                values[i] = left._vectorArray[i] - right._vectorArray[i];
            }
            return (new Vector(values));
        }

        public double this[int i]
        {
            get
            {
                return _vectorArray[i];
            }
        }
    }
}
