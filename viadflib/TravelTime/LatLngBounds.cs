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
    }


}
