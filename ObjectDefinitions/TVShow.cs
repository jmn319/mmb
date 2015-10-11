using System.Collections.Generic;
using MongoDB.Bson;

namespace mmb.ObjectDefinitions
{
    public class TvShow
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public List<Season> Seasons { get; set; }
    }

    public class Season
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public List<Episode> Episodes { get; set; }
    }

    public class Episode
    {
        public string Name { get; set; }
        public int Number { get; set; }
        public bool Downloaded { get; set; }
        public List<string> DownloadLocations { get; set; }
    }

    public class BasicShow
    {
        public ObjectId _id { get; set; }
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string DownloadLocation { get; set; }
    }
}
