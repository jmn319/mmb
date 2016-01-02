using System.Collections.Generic;
using MongoDB.Bson;

namespace mmb.ObjectDefinitions
{
    public class Movie
    {
        public ObjectId _id { get; set; }
        public string YtsMovieTitle { get; set; }
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

    class YtsReturnHeaders
    {
        public string status { get; set; }
        public string status_message { get; set; }
        public List<YtsReturn_v2> data { get; set; }
    }

    class YtsReturn_v2
    {
        public int movie_count { get; set; }
        public int limit { get; set; }
        public int page_number { get; set; }
        public List<Yts_v2> movies { get; set; }
    }

    class Yts_v2
    {
        public string id { get; set; }
        public string url { get; set; }
        public string imdb_code { get; set; }
        public string title { get; set; }
        public string title_english { get; set; }
        public string title_long { get; set; }
        public string slug { get; set; }
        public string year { get; set; }
        public string rating { get; set; }
        public int runtime { get; set; }
        public List<string> genres { get; set; }
        public string summary { get; set; }
        public string description_full { get; set; }
        public string synopsis { get; set; }
        public string yt_trailer_code { get; set; }
        public string language { get; set; }
        public string mpa_rating { get; set; }
        public string background_image { get; set; }
        public string background_image_original { get; set; }
        public string small_cover_image { get; set; }
        public string medium_cover_image { get; set; }
        public string state { get; set; }
        public List<DownloadDetails_v2> torrents { get; set; }
        public string date_uploaded { get; set; }
        public int date_uploaded_unix { get; set; }
    }

    public class DownloadDetails_v2
    {
        public string url { get; set; }
        public string hash { get; set; }
        public string quality { get; set; }
        public int seeds { get; set; }
        public int peers { get; set; }
        public string size { get; set; }
        public string size_bytes { get; set; }
        public string date_uploaded { get; set; }
        public string date_uploaded_unix { get; set; }
    }
}
