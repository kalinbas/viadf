using System;
using System.Runtime.Serialization;

namespace viadflib
{
    /// <summary>
    /// Parameters for search
    /// </summary>
    [DataContract]
    public class SearchParams
    {
        /// <summary>
        /// Start of search
        /// </summary>
        [DataMember]
        public SearchPosition StartSearch { get; set; }

        /// <summary>
        /// End of of search
        /// </summary>
        [DataMember]
        public SearchPosition EndSearch { get; set; }

        /// <summary>
        /// Optional: Starttime of travel
        /// </summary>
        [DataMember(IsRequired=false)]
        public DateTime? StartTime { get; set; }

        /// <summary>
        /// Optional: Maximal number of requested results (its possible that less results are returned)
        /// </summary>
        [DataMember(IsRequired = false)]
        public int NrOfResults { get; set; }
    }
}
