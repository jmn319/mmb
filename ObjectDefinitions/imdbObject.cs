using System.Collections.Generic;

namespace mmb.ObjectDefinitions
{
    public class ImdbElement
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Name { get; set; }
        public string TitleDescription { get; set; }
        public string EpisodeTitle { get; set; }
        public string Description { get; set; }
    }

    public class ImdbObject
    {
        public List<ImdbElement> title_popular { get; set; }
        public List<ImdbElement> title_exact { get; set; }
        public List<ImdbElement> title_substring { get; set; }
    }
}
