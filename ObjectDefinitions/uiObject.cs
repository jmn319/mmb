namespace mmb.ObjectDefinitions
{
    public class GetObject
    {
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public string MediaType { get; set; }
        public bool Show { get; set; }
        public bool Movie { get; set; }
        public bool Pending { get; set; }
        public bool All { get; set; }
        public bool Downloaded { get; set; }
        public bool Saved { get; set; }
    }

    public class ModifyObject
    {
        public string ModType { get; set; }
        public string Name { get; set; }
        public string MediaType { get; set; }
        public bool Show { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public bool Movie { get; set; }
        public string ImdbCode { get; set; }
        public string Year { get; set; }
    }

    public class GetReturnObject
    {
        public string Name { get; set; }
        public int Season { get; set; }
        public int Episode { get; set; }
        public bool Show { get; set; }
        public bool Movie { get; set; }
        public string ImdbCode { get; set; }
        public int Year { get; set; }
    }
}
