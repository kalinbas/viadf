using System;

namespace viadflib.TravelTime.Utils
{
    public class Singleton<T> where T : class, new()
    {
        Singleton() { }

        private static readonly Lazy<T> _instance = new Lazy<T>(() => new T());

        public static T Instance { get { return _instance.Value; } }
    }
}
