using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace viadflib
{
    /// <summary>
    /// List of search results
    /// </summary>
    [DataContract]
    public class SearchResultModel
    {
        [DataMember]
        public List<SearchResult> Results { get; set; }
    }

    /// <summary>
    /// Single search result
    /// </summary>
    [DataContract]
    public class SearchResult
    {
        /// <summary>
        /// Step of the search result
        /// </summary>
        [DataMember]
        public List<SearchResultItem> Items { get; set; }

        /// <summary>
        /// Start of the search, as specified in SearchParams
        /// </summary>
        [DataMember]
        public SearchPosition Start { get; set; }

        /// <summary>
        /// End of the search, as specified in SearchParams
        /// </summary>
        [DataMember]
        public SearchPosition End { get; set; }

        /// <summary>
        /// Total estimated travel time for this result (in minutes)
        /// </summary>
        [DataMember]
        public double TotalTime { get; set; }

        /// <summary>
        /// Total distance traveled in km
        /// </summary>
        [DataMember]
        public double TotalDistance { get; set; }

        /// <summary>
        /// Total price in MXN
        /// </summary>
        [DataMember]
        public double TotalPrice { get; set; }

        /// <summary>
        /// If there was a StartTime specified in SearchParams this is returned
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? StartTravelTime { get; set; }

        /// <summary>
        /// If there was a StartTime specified in SearchParams this is returned
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        public DateTime? EndTravelTime { get; set; }

        public override string ToString()
        {
            return string.Join(" | ", Items.Select(x => x.Start.Name + " -> " + x.End.Name + " " + (x.Route == null ? "" : "(" + x.Route.Name + ")")).ToArray());
        }
    }

    /// <summary>
    /// Item of a search result. This can be for example: Metro Linea 2 de Zocalo a Bellas Artes
    /// </summary>
    [DataContract]
    public class SearchResultItem
    {
        public SearchResultItem()
        {
            Path = new List<SearchPosition>();
        }

        /// <summary>
        /// Distance in km
        /// </summary>
        [DataMember]
        public double Distance { get; set; }

        /// <summary>
        /// Price in MXN
        /// </summary>
        [DataMember]
        public double Price { get; set; }

        /// <summary>
        /// Time in minutes
        /// </summary>
        [DataMember]
        public double Time { get; set; }

        /// <summary>
        /// Type of transport (por ejemplo: Metro)
        /// </summary>
        [DataMember]
        public ResultType Type { get; set; }

        /// <summary>
        /// Linea de transporte (por ejemplo: Linea 2)
        /// </summary>
        [DataMember]
        public ResultRoute Route { get; set; }

        /// <summary>
        /// Estacion o parada donde empieza (por ejemplo: Metro Bellas Artes)
        /// </summary>
        [DataMember]
        public ResultRoutePiece Start { get; set; }

        /// <summary>
        /// Estacion o parada donde termina (por ejemplo: Metro Zocalo)
        /// </summary>
        [DataMember]
        public ResultRoutePiece End { get; set; }

        /// <summary>
        /// Name of start point
        /// </summary>
        [DataMember]
        public string StartName { get; set; }

        /// <summary>
        /// Name of direction
        /// </summary>
        [DataMember]
        public string InDirection { get; set; }

        /// <summary>
        /// Name of end point
        /// </summary>
        [DataMember]
        public string EndName { get; set; }

        /// <summary>
        /// Path of stations / crossings between Start and End (for drawing on map)
        /// </summary>
        [DataMember]
        public List<SearchPosition> Path { get; set; }

        /// <summary>
        /// Estimated time when to arrive at start point
        /// </summary>
        public DateTime StartTravelTime { get; set; }

        /// <summary>
        /// Estimated time when to arrive at end point
        /// </summary>
        public DateTime EndTravelTime { get; set; }
    }

    /// <summary>
    /// The type of transport used
    /// </summary>
    [DataContract]
    public class ResultType
    {
        /// <summary>
        /// Name of the transport type
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        public static ResultType FromType(Type type)
        {
            if (type != null)
            {
                ResultType resultType = new ResultType();
                resultType.Name = type.Name;
                return resultType;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// The route of transport used 
    /// </summary>
    [DataContract]
    public class ResultRoute
    {
        public int ID { get; set; }

        /// <summary>
        /// Name
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Name where route starts
        /// </summary>
        [DataMember]
        public string FromName { get; set; }

        /// <summary>
        /// Name where route ends
        /// </summary>
        [DataMember]
        public string ToName { get; set; }

        /// <summary>
        /// Type of this route
        /// </summary>
        [DataMember]
        public ResultType Type { get; set; }

        public int? SplitRoutePieceID { get; set; }

        public static ResultRoute FromRoute(Route route)
        {
            if (route != null)
            {
                ResultRoute res = new ResultRoute();
                res.ID = route.ID;
                res.Name = route.Name;
                res.FromName = route.FromName;
                res.ToName = route.ToName;
                res.Type = ResultType.FromType(route.Type);
                res.SplitRoutePieceID = route.SplitRoutePieceID;
                return res;
            }
            else
            {
                return null;
            }
        }
    }

    /// <summary>
    /// Station / Street crossing of a route
    /// </summary>
    [DataContract]
    public class ResultRoutePiece
    {
        public int ID { get; set; }

        /// <summary>
        /// Coordinate Latitude
        /// </summary>
        [DataMember]
        public double Lat { get; set; }

        /// <summary>
        /// Coordinate Longitude
        /// </summary>
        [DataMember]
        public double Lng { get; set; }

        /// <summary>
        /// Name (if its a station with a name)
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Corresponding route
        /// </summary>
        [DataMember]
        public ResultRoute Route { get; set; }

        public static ResultRoutePiece FromRoutePiece(RoutePiece piece)
        {
            ResultRoutePiece res = new ResultRoutePiece();
            res.ID = piece.ID;
            res.Lat = piece.Lat;
            res.Lng = piece.Lng;
            res.Name = piece.Name;
            res.Route = ResultRoute.FromRoute(piece.Route);
            return res;
        }
    }
}
