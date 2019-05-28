using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AHEntities
{
    public enum QueueType
    {
        Velocity,
        Position
    }

    public class TrajectoryQueue
    {
        private int degreesOfFreedom;
        private List<double[]> positionQueue;
        private List<double[]> velocityQueue;
        double[] lastPosition;

        public int DOF
        {
            get { return degreesOfFreedom; }
        }

        public TrajectoryQueue(int DegreesOfFreedom, double[] initPosition)
        {
            lock (this)
            {
                degreesOfFreedom = DegreesOfFreedom;
                positionQueue = new List<double[]>();
                velocityQueue = new List<double[]>();
                // if DegreesOfFreedom == 2
                lastPosition = initPosition;
            }
        }

        public void InitPosition(double[] initPosition)
        {
            lock (this)
            {
                lastPosition = initPosition;
            }
        }

        public double[] GetNext(QueueType type)
        {
            double[] current;
            if (type == QueueType.Position)
            {
                lock (this)
                {
                    if (positionQueue.Count > 0)
                    {
                        current = new double[degreesOfFreedom];
                        positionQueue[0].CopyTo(current, 0);
                        positionQueue[0].CopyTo(lastPosition, 0);
                        positionQueue.RemoveAt(0);
                    }
                    else
                    {
                        current = lastPosition;
                    }
                }
            }
            else
            {
                lock (this)
                {
                    if (velocityQueue.Count > 0)
                    {
                        current = new double[degreesOfFreedom];
                        velocityQueue[0].CopyTo(current, 0);
                        velocityQueue.RemoveAt(0);
                    }
                    else
                    {
                        current = new double[] { 0, 0 };
                    }
                }
            }
            return current;
        }

        public List<double[]> GetList(QueueType type)
        {
            if (type == QueueType.Position)
                return this.positionQueue;
            else if (type == QueueType.Velocity)
                return this.velocityQueue;
            else
                return new List<double[]>();
        }
        
        public void AddBack(QueueType type, double[] point)
        {
            if (point.Length == degreesOfFreedom)
            {
                if (type == QueueType.Position)
                {
                    lock (this)
                    {
                        positionQueue.Add(point);
                    }
                }
                else
                {
                    lock (this)
                    {
                        velocityQueue.Add(point);
                    }
                }
            }
        }

        public void AddRangeBack(QueueType type, double[,] points)
        {
            if (points == null)
                return;
            
            List<double[]> tempQueue = new List<double[]>();
            for (int i = 0; i < points.GetLength(1); i++)
            {
                tempQueue.Add(new double[2] { points[0, i], points[1, i] });
            }
            if (type == QueueType.Position)
            {
                lock (this)
                {
                    positionQueue.AddRange(tempQueue);
                }
            }
            else
            {
                lock (this)
                {
                    velocityQueue.AddRange(tempQueue);
                }
            }

        }
        
        public void Replace(QueueType type, double[,] points)
        {
            if (points == null)
                return;

            List<double[]> tempQueue = new List<double[]>();
            for (int i = 0; i < points.GetLength(1); i++)
            {
                tempQueue.Add(new double[2] { points[0, i], points[1, i] });
            }
            if (type == QueueType.Position)
            {
                lock (this)
                {
                    positionQueue.Clear();
                    positionQueue.AddRange(tempQueue);
                }
            }
            else
            {
                lock (this)
                {
                    velocityQueue.Clear();
                    velocityQueue.AddRange(tempQueue);
                }
            }
        }

        public void Replace(TrajectoryQueue queue)
        {
            lock (this)
            {
                this.positionQueue = new List<double[]>(queue.GetList(QueueType.Position));
                this.velocityQueue = new List<double[]>(queue.GetList(QueueType.Velocity));
            }
        }

        public void Clear()
        {
            lock (this)
            {
                positionQueue.Clear();
                velocityQueue.Clear();
            }
        }

        public bool IsEmpty(QueueType type)
        {
            int count;
            if (type == QueueType.Position)
            {
                lock (this)
                {
                    count = positionQueue.Count;
                }
            }
            else
            {
                lock (this)
                {
                    count = velocityQueue.Count;
                }
            }
            if (count == 0)
                return true;
            else
                return false;
        }

        public string PositionToString()
        {
            string str = String.Empty;
            for (int i = 0; i < positionQueue.Count; i++)
                str = str + positionQueue[i][0].ToString() + ",";
            return str;
        }

    }
}
