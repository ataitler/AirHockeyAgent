using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public class PointParams
    {
        private List<Point> parameters;
        private DateTime timeStamp;

        public void AddParameters(Point P)
        {
            parameters.Add(P);
        }

        public DateTime T
        {
            get { return new DateTime(timeStamp.Ticks); }
            set { timeStamp = value; }
        }

        public PointParams()
        {
            parameters = new List<Point>();
        }

        public void EditParamByIndex(int index, Point newParam)
        {
            parameters[index - 1].X = newParam.X;
            parameters[index - 1].Y = newParam.Y;
        }
        
        public Point GetParamsByIndex(int index)
        {
            return parameters[index - 1];
        }

        public double[,] GetParamsAsArray()
        {
            double[,] arrayParameters = new double[2, parameters.Count];

            for (int i = 0; i < parameters.Count; i++)
            {
                arrayParameters[0, i] = parameters[i].X;
                arrayParameters[1, i] = parameters[i].Y;
            }

            return arrayParameters;
        }

        public void Clear()
        {
            parameters.Clear();
        }

        public override string ToString()
        {
            string obj = string.Empty;

            foreach (Point P in parameters)
            {
                obj = obj + " " + P.ToString();
            }
            return obj;
        }
    }
}
