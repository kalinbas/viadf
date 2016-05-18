using System;

namespace viadflib.TravelTime
{
    public class LatLng
    {
        /// <summary>
        /// Aprox. the same for lat / lng
        /// </summary>
        private const double KM_IN_DEGREES_LAT = 1 / 110.649;
        private const double KM_IN_DEGREES_LNG = 1 / 105.3;

        public LatLng MoveKm(double latKm, double lngKm)
        {
            return new LatLng(((Lat / KM_IN_DEGREES_LAT) + latKm) * KM_IN_DEGREES_LAT, ((Lng / KM_IN_DEGREES_LNG) + lngKm) * KM_IN_DEGREES_LNG);
        }

        public LatLng(double lat, double lng)
        {
            Lat = lat;
            Lng = lng;
        }
    
        public double DistanceInKmTo(LatLng other)
        {
            double distanceLat = Math.Abs(Lat - other.Lat) / KM_IN_DEGREES_LAT;
            double distanceLng = Math.Abs(Lng - other.Lng) / KM_IN_DEGREES_LNG;

            return Math.Sqrt(distanceLat * distanceLat + distanceLng * distanceLng);
        }

        public double Lat { get; set; }
        public double Lng { get; set; }
    }
}
