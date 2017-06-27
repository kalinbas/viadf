using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace viadflib.TravelTime.Utils
{
    public class JsonHelper
    {
        public static T Deserialize<T>(string json)
        {
            var serializer = new DataContractJsonSerializer(typeof(T));
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
            return (T)serializer.ReadObject(stream);
        }

    }
}
