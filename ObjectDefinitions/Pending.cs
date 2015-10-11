using MongoDB.Bson;

namespace mmb.ObjectDefinitions
{
    public class Pending
    {
        public ObjectId _id { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public bool Show { get; set; }
        public bool Movie { get; set; }
    }

    public class Download
    {
        public ObjectId _id { get; set; }
        public string FileName { get; set; }
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public bool Show { get; set; }
        public bool Movie { get; set; }
    }
}
