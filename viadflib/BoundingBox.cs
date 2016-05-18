namespace viadflib
{
    class BoundingBox
    {
        public BoundingBox(double lat1, double lat2, double lng1, double lng2)
        {
            LatMin = lat1;
            LatMax = lat2;
            LngMin = lng1;
            LngMax = lng2;
        }

        public double LatMin { get; set; }
        public double LatMax { get; set; }
        public double LngMin { get; set; }
        public double LngMax { get; set; }

        public bool Intersect(BoundingBox other)
        {
            return other.LatMax >= LatMin && other.LatMin <= LatMax && other.LngMax >= LngMin && other.LngMin <= LngMax;
        }

        public bool Contains(double lat, double lng)
        {
            return lat >= LatMin && lat < LatMax && lng >= LngMin && lng <= LngMax;
        }
    }
}
