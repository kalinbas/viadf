
using System.Collections.Generic;
using System.Linq;

namespace viadflib.TravelTime
{
    public class LatLngBounds
    {
        public LatLng Lower { get; set; }
        public LatLng Upper { get; set; }

        public LatLngBounds(LatLng lower, LatLng upper)
        {
            Lower = lower;
            Upper = upper;
        }

        public static LatLngBounds FromWalkingDistance(LatLng latLng, double timeMin, double walkSpeedKmH)
        {
            double kms = walkSpeedKmH * (timeMin / 60.0);
            return new LatLngBounds(latLng.MoveKm(-kms, -kms), latLng.MoveKm(kms, kms));
        }

        public bool Contains(LatLngBounds other)
        {
            return Lower.Lat <= other.Lower.Lat && Lower.Lng <= other.Lower.Lng && Upper.Lat >= other.Upper.Lat && Upper.Lng >= other.Upper.Lng;
        }

        public void Extend(LatLng point)
        {
            if (point.Lat < Lower.Lat) Lower.Lat = point.Lat;
            if (point.Lat > Upper.Lat) Upper.Lat = point.Lat;
            if (point.Lng < Lower.Lng) Lower.Lng = point.Lng;
            if (point.Lng > Upper.Lng) Upper.Lng = point.Lng;
        }

        public void Extend(LatLngBounds bounds)
        {
            Extend(bounds.Lower);
            Extend(bounds.Upper);
        }

        public static LatLngBounds FromList(List<LatLng> points)
        {
            LatLngBounds bounds = new LatLngBounds(points.First(), points.Last());
            foreach (var point in points)
            {
                if (point.Lat < bounds.Lower.Lat) bounds.Lower.Lat = point.Lat;
                if (point.Lat > bounds.Upper.Lat) bounds.Upper.Lat = point.Lat;
                if (point.Lng < bounds.Lower.Lng) bounds.Lower.Lng = point.Lng;
                if (point.Lng > bounds.Upper.Lng) bounds.Upper.Lng = point.Lng;
            }
            return bounds;
        }
    }


}
