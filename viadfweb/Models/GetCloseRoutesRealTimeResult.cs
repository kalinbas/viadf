using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace viadfweb.Models
{
    public class GetCloseRoutesRealTimeResult
    {
        public string route { get; set; }

        public string direction { get; set; }

        public string timeLeft { get; set; }

        public long time { get; set; }
    }
}