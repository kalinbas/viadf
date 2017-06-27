using System;
using System.Linq;
using System.Collections.Generic;
using viadflib.TravelTime.Clipper;
using System.Collections;

namespace viadflib.TravelTime.Utils
{
    public static class PolygonHelper
    {   
        public static List<TravelTimePolygonPath> CreatePolygonPathUsingBitmapArray(List<TravelTimeCircle> circles, double speed, int resolution)
        {
            // create bitmap
            int size = resolution;
            int arraySize = size * size;

            BitArray array = new BitArray(arraySize, false);
            BitArray visitedArray = new BitArray(arraySize, false);

            LatLngBounds bounds = null;
            foreach (var circle in circles)
            {
                if (bounds == null)
                {
                    bounds = LatLngBounds.FromWalkingDistance(circle.Center, circle.TimeLeft, speed);
                }
                else
                {
                    bounds.Extend(LatLngBounds.FromWalkingDistance(circle.Center, circle.TimeLeft, speed));
                }
            }

            // populate bitmaparray
            foreach (var circle in circles)
            {
                var area = LatLngBounds.FromWalkingDistance(circle.Center, circle.TimeLeft, speed);

                int maxY = (int)Math.Floor(size * (bounds.Upper.Lat - area.Lower.Lat) / (bounds.Upper.Lat - bounds.Lower.Lat));
                int minY = (int)Math.Floor(size * (bounds.Upper.Lat - area.Upper.Lat) / (bounds.Upper.Lat - bounds.Lower.Lat));
                int minX = (int)Math.Floor(size * (area.Lower.Lng - bounds.Lower.Lng) / (bounds.Upper.Lng - bounds.Lower.Lng));
                int maxX = (int)Math.Floor(size * (area.Upper.Lng - bounds.Lower.Lng) / (bounds.Upper.Lng - bounds.Lower.Lng));

                for (int x = minX; x <= Math.Min(size - 1, maxX); x++)
                {
                    for (int y = minY; y <= Math.Min(size -1, maxY); y++)
                    {
                        array[x + y * size] = true;
                    }
                }
            }

            // fill all negative fields that connect to border
            for (int i = 0; i < arraySize; i++)
            {
                if (!visitedArray[i] && !array[i])
                {
                    if (i % size == 0 || i % size == size - 1 || i / size == 0 || i / size == size - 1)
                    {
                        // flood fill if borderpiece
                        FloodFillMarkVisited(i, size, array, visitedArray);
                    }
                }
            }

            // traverse and create polygons
            List<TravelTimePolygonPath> paths = new List<TravelTimePolygonPath>();
            for (int i = 0; i < arraySize; i++)
            {
                if (!visitedArray[i])
                {
                    if (array[i])
                    {
                        bool touchesOutside;
                        var path = CreateEdgePolygonPath(i, size, array, visitedArray, bounds, out touchesOutside);
                        FloodFillMarkVisited(i, size, array, visitedArray);

                        // only add path if it touches the outside world - otherwise its an "island" path
                        if (touchesOutside)
                        {
                            paths.Add(path);
                        }
                    }
                }
            }


            // find not visited and create holes
            for (int i = 0; i < arraySize; i++)
            {
                // is it really a hole?
                if (!visitedArray[i] && !array[i])
                {
                    bool touchesOutside;
                    var path = CreateEdgePolygonPath(i, size, array, visitedArray, bounds, out touchesOutside);
                    FloodFillMarkVisited(i, size, array, visitedArray);

                    // reverse so they are interpreted as negative
                    path.Coords.Reverse();
                    paths.Add(path);
                }
            }


            return paths;
        }

        /// <summary>
        /// Marks all connected elements in array as visited in visitedArray
        /// Starting with index
        /// </summary>
        private static void FloodFillMarkVisited(int index, int size, BitArray array, BitArray visitedArray)
        {
            // value of fieds which should be filled
            bool fillValue = array[index];

            Queue<int> toFill = new Queue<int>();
            toFill.Enqueue(index);
            visitedArray[index] = true;
            while (toFill.Count > 0)
            {
                int i = toFill.Dequeue();
                if (i % size > 0 && fillValue == array[i - 1] && !visitedArray[i - 1]) { toFill.Enqueue(i - 1); visitedArray[i - 1] = true; }
                if (i % size < size - 1 && fillValue == array[i + 1] && !visitedArray[i + 1]) { toFill.Enqueue(i + 1); visitedArray[i + 1] = true; }
                if (i / size > 0 && fillValue == array[i - size] && !visitedArray[i - size]) { toFill.Enqueue(i - size); visitedArray[i - size] = true; }
                if (i / size < size - 1 && fillValue == array[i + size] && !visitedArray[i + size]) { toFill.Enqueue(i + size); visitedArray[i + size] = true; }
            }
        }

        private static TravelTimePolygonPath CreateEdgePolygonPath(int startIndex, int size, BitArray array, BitArray visitedArray, LatLngBounds bounds, out bool touchesOutside)
        {
            TravelTimePolygonPath path = new TravelTimePolygonPath();

            // select value which identifies a wall
            bool wallValue = !array[startIndex];

            touchesOutside = false;

            // start walking right
            const int startDirection = 2;

            int currentDirection = startDirection;
            int currentIndex = startIndex;

            // do until start point reached again
            while (startIndex != currentIndex || currentDirection != 2 || path.Coords.Count == 0)
            {
                // check left of walking direction
                bool outsideWall;
                if (IsWall(array, visitedArray, currentIndex, size, (currentDirection - 1 + 4) % 4, wallValue, out outsideWall))
                {
                    // if there is ONE outside wall - it touches outside
                    touchesOutside = outsideWall || touchesOutside;

                    // add wall left of walking direction
                    path.Coords.Add(GetWallCordinates(currentIndex, size, (currentDirection - 1 + 4) % 4, bounds));

                    // check in front
                    if (IsWall(array, visitedArray, currentIndex, size, currentDirection, wallValue, out outsideWall))
                    {
                        // turn right
                        currentDirection = (currentDirection + 1) % 4;
                    }
                    else
                    {
                        currentIndex = GetNextIndexInDirection(currentIndex, size, currentDirection).Value;
                    }
                }
                else
                {
                    // turn left & make one step
                    currentDirection = (currentDirection - 1 + 4) % 4;
                    currentIndex = GetNextIndexInDirection(currentIndex, size, currentDirection).Value;
                }
            }
            return path;
        }

        private static LatLng GetWallCordinates(int index, int size, int direction, LatLngBounds bounds)
        {
            double x = (index % size) + (direction == 1 || direction == 3 ? 0.5 : direction == 2 ? 1 : 0);
            double y = (index / size) + (direction == 0 || direction == 2 ? 0.5 : direction == 3 ? 1 : 0);


            double lng = bounds.Lower.Lng + (x / size) * (bounds.Upper.Lng - bounds.Lower.Lng);
            double lat = bounds.Lower.Lat + ((size - y) / size) * (bounds.Upper.Lat - bounds.Lower.Lat);
            return new LatLng(lat, lng);
        }

        private static int? GetNextIndexInDirection(int index, int size, int direction)
        {
            if (direction == 0)
            {
                return index % size == 0 ? (int?)null : index - 1;
            }
            // up
            if (direction == 1)
            {
                return index / size == 0 ? (int?)null : index - size;
            }
            // right
            if (direction == 2)
            {
                return index % size == size - 1 ? (int?)null : index + 1;
            }
            // down
            return index / size == size - 1 ? (int?)null : index + size;
        }

        private static bool IsWall(BitArray array, BitArray visitedArray, int index, int size, int direction, bool wallValue, out bool outsideWall)
        {
            int? nextIndex = GetNextIndexInDirection(index, size, direction);

            // its a wall if there is no other index in this direction OR if its a wall
            if (!nextIndex.HasValue || wallValue == array[nextIndex.Value])
            {
                outsideWall = !nextIndex.HasValue || visitedArray[nextIndex.Value];
                return true;
            }
            outsideWall = false;
            return false;
        }

        public static List<TravelTimePolygonPath> CreatePolygonPaths(List<TravelTimeCircle> circles, double speed)
        {
            List<TravelTimePolygonPath> paths = new List<TravelTimePolygonPath>();

            var clipper = new Clipper.Clipper();

            foreach (var circle in circles)
            {
                clipper.AddPolygon(CreateCirclePolygon(circle.Center, circle.TimeLeft, 12, speed), PolyType.ptSubject);
            }

            List<List<IntPoint>> solution = new List<List<IntPoint>>();

            clipper.Execute(ClipType.ctUnion, solution, PolyFillType.pftPositive, PolyFillType.pftPositive);

            foreach (var solutionItem in solution)
            {
                var path = new TravelTimePolygonPath();
                foreach (var intPoint in solutionItem)
                {
                    path.Coords.Add(new LatLng(intPoint.X / 10000000.0, intPoint.Y / 10000000.0));
                }
                paths.Add(path);
            }

            return paths;
        }

        private static List<IntPoint> CreateCirclePolygon(LatLng center, double timeMins, int edges, double speed)
        {
            int degreeStep = 360 / edges;

            double kms = (timeMins / 60.0) * speed;

            var circlePoints = new List<IntPoint>();
            for (int x = 0; x < 360; x += degreeStep)
            {
                var angleRadian = x * Math.PI / 180.0;

                LatLng newPoint = center.MoveKm(Math.Cos(angleRadian) * kms, Math.Sin(angleRadian) * kms);
                circlePoints.Add(new IntPoint(newPoint.Lat * 10000000, newPoint.Lng * 10000000));
            }
            return circlePoints;
        }

    }
}
