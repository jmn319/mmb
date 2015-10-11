using System.Collections.Generic;
using MongoDB.Bson;

namespace mmb.ObjectDefinitions
{
    public class Movie
    {
        public ObjectId _id { get; set; }
        public string YtsMovietitle { get; set; }
        public string ImdbTitle { get; set; }
        public List<string> Urls { get; set; }
        public int Year { get; set; }
        public string ImdbCode { get; set; }
        public string CoverImg { get; set; }
        public string Genre { get; set; }
        public List<DownloadDetails> DownloadLogistics { get; set; }
        public bool Downloaded { get; set; }
    }

    public class DownloadDetails
    {
        public string Quality { get; set; }
        public string Size { get; set; }
        public string SizeBytes { get; set; }
        public int Seeds { get; set; }
        public string TorrentUrl { get; set; }
    }

    class YtsReturn
    {
        public int MovieCount { get; set; }
        public List<Yts> MovieList { get; set; }
    }

    class Yts
    {
        public string MovieId { get; set; }
        public string State { get; set; }
        public string MovieUrl { get; set; }
        public string MovieTitle { get; set; }
        public string MovieTitleClean { get; set; }
        public string MovieYear { get; set; }
        public string AgeRating { get; set; }
        public string DateUploaded { get; set; }
        public int DateUploadedEpoch { get; set; }
        public string Quality { get; set; }
        public string CoverImage { get; set; }
        public string ImdbCode { get; set; }
        public string ImdbLink { get; set; }
        public string Size { get; set; }
        public string SizeByte { get; set; }
        public string MovieRating { get; set; }
        public string Genre { get; set; }
        public string Uploader { get; set; }
        public string UploaderUid { get; set; }
        public string Downloaded { get; set; }
        public string TorrentSeeds { get; set; }
        public string TorrentPeers { get; set; }
        public string TorrentUrl { get; set; }
        public string TorrentHash { get; set; }
        public string TorrentMagnetUrl { get; set; }
    }
}
