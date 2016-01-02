using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using HtmlAgilityPack;
using mmb.ObjectDefinitions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServiceStack;

namespace mmb
{
    class MovieUtils
    {
        //Updated July 26th
        public static void WriteImdbMovies()
        {
            bool end = false, firstMovie = true;
            int results = 252, incrementer = 1;
            try
            {
                while (!end)
                {
                    if (incrementer < results)
                    {
                        HtmlNodeCollection collection =
                            DownloadMgmtUtils.GetHtmlNodeCollection(firstMovie ? ConfigurationManager.AppSettings["imdb_root"] : ImdbUrlIncrementor(incrementer), "//a");
                        if (firstMovie)
                        {
                            HtmlNodeCollection divCollection = DownloadMgmtUtils.GetHtmlNodeCollection(ConfigurationManager.AppSettings["imdb_root"], "//div");
                            var ppp = divCollection.Where(d => d.Attributes.Count > 0 && d.Attributes[0].Value == "left").ToList();
                            results = Convert.ToInt32(ppp[0].InnerText.Replace("1-250 of", "")
                                        .Replace("\n", "")
                                        .Replace(",", "")
                                        .Replace("titles.", ""));
                            firstMovie = false;
                        }

                        foreach (HtmlNode n in collection)
                        {
                            if (n.Attributes.Count != 1 || !n.Attributes[0].Value.Contains("/title/")) continue;
                            MongoCollection mongoCollection = MongoUtils.GetMongoCollection
                            (
                                @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                                ConfigurationManager.AppSettings["port"],
                                ConfigurationManager.AppSettings["db"],
                                ConfigurationManager.AppSettings["movie_collection"]
                            );
                            var mongoMovie = mongoCollection.FindAs<Movie>(Query.EQ("ImdbCode", n.Attributes[0].Value.Replace("/title/", "").Replace("/", ""))).ToList<Movie>();
                            //Check to see if the movie already exists
                            if (mongoMovie.Count == 0)
                            {
                                mongoCollection.Insert(new Movie()
                                {
                                    ImdbCode = n.Attributes[0].Value.Replace("/title/", "").Replace("/", ""),
                                    ImdbTitle = n.InnerText,
                                    Urls = new List<string>() { "http://www.imdb.com" + n.Attributes[0].Value },
                                    Downloaded = false
                                });
                            }
                            else
                            {
                                mongoMovie[0].ImdbTitle = n.Attributes[0].Value.Replace("/title/", "").Replace("/", "");
                                mongoMovie[0].Urls.Add("http://www.imdb.com" + n.Attributes[0].Value);
                                mongoCollection.Save(mongoMovie[0]);
                            }
                        }
                        incrementer = incrementer + 250;
                    }
                    else { end = true; }
                }
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL write from IMDB. " + e, ConfigurationManager.AppSettings["log_file"]); }
        }

        //Updated August 6th
        public static void InsertImdbEntry(ImdbElement imdbElement)
        {
            try
            {
                MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["movie_collection"]
                    ).Insert(new Movie()
                    {
                        ImdbCode = imdbElement.Id,
                        ImdbTitle = imdbElement.Title,
                        Year = Regex.Replace(imdbElement.TitleDescription, "[^0-9.+-]", "").ToInt(),
                        Urls = new List<string>() {"http://www.imdb.com/" + imdbElement.Id},
                        Downloaded = false
                    });
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL write from IMDB to Mongo. " + e, ConfigurationManager.AppSettings["log_file"]); }

        }

        //Updated August 1st
        public static string ImdbUrlIncrementor(int set)
        { return ConfigurationManager.AppSettings["imdb_root"].Replace("start=1", "start=" + set).
            Replace("sYYYY", DateTime.Now.Year.ToString().Replace("fYYYY", DateTime.Now.Year.ToString() + 3)); }

        public static string YtsUrlIncrementor(int set)
        { return ConfigurationManager.AppSettings["yts_root"] + ConfigurationManager.AppSettings["yts_suffix"] + set; }

        public static string YtsUrl()
        { return ConfigurationManager.AppSettings["yts_root"] + ConfigurationManager.AppSettings["yts_suffix"]; }

        //Updated December 6th - updated YTS logic
        public static void WriteYtsMovieData()
        {
            int setNumber = 1;
            int movies = 1000000;
            try
            {
                MongoCollection mongoCollection = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["movie_collection"]
                );
                while (movies > setNumber * 50)
                {
                    var results = DownloadMgmtUtils.GetJsonObject(YtsUrlIncrementor(setNumber));
                    if (setNumber == 1) { movies = results.movie_count; }
                    
                    foreach (var m in results.movies)
                    {
                        //Check to see if the movie exists in the collection
                        var mongoMovie = mongoCollection.FindAs<Movie>(Query.EQ("ImdbCode", m.imdb_code)).ToList<Movie>();
                        if (mongoMovie.Count == 0 && m.torrents != null)
                        {
                           mongoCollection.Insert(new Movie()
                            {
                                YtsMovieTitle = m.title.Replace(":", ""),
                                ImdbCode = m.imdb_code,
                                CoverImg = m.background_image,
                                Genre = m.genres[0],
                                Year = Convert.ToInt16(m.year),
                                Urls = new List<string>() { m.url },
                                DownloadLogistics = new List<DownloadDetails>()
                                {
                                    new DownloadDetails()
                                    {
                                        Size = m.torrents[0].size,
                                        SizeBytes = m.torrents[0].size_bytes,
                                        TorrentUrl = m.torrents[0].url,
                                        Seeds = Convert.ToInt16(m.torrents[0].seeds),
                                        Quality = m.torrents[0].quality
                                    }
                                }
                            });
                        }
                        //if movie already exists, add/overwrite detail
                        else if (m.torrents != null)
                        { //ILASM!
                            mongoMovie[0].YtsMovieTitle = m.title.Replace(":", "");
                            mongoMovie[0].ImdbTitle = mongoMovie[0].ImdbTitle;
                            mongoMovie[0].CoverImg = m.background_image;
                            mongoMovie[0].Genre = m.genres[0];
                            mongoMovie[0].Year = Convert.ToInt16(m.year);
                            mongoMovie[0].DownloadLogistics = new List<DownloadDetails>()
                            {
                                new DownloadDetails()
                                {
                                    Size = m.torrents[0].size,
                                    SizeBytes = m.torrents[0].size_bytes,
                                    TorrentUrl = m.torrents[0].url,
                                    Seeds = Convert.ToInt16(m.torrents[0].seeds),
                                    Quality = m.torrents[0].quality
                                }
                            };
                            if (!mongoMovie[0].Urls.Contains(m.url))
                                mongoMovie[0].Urls.Add(m.url);
                            mongoCollection.Save(mongoMovie[0]);
                        }
                    }
                    setNumber++;
                }
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL write from YTS. " + e, ConfigurationManager.AppSettings["log_file"]); } 
        }

        //TODO: EMPTY METHOD FILL IN
        public static void RebuildMovies()
        {

        }

        //Updated October 18th
        public static string AddMovieToMyMovies(string Name, string ImdbCode, string Year)
        {
            string r = "";
            try
            {
                Movie m = new Movie() { ImdbTitle = Name, ImdbCode = ImdbCode, Year = Convert.ToInt16(Year) };
                MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["mymovies_collection"]
                    ).Insert(m);
                Log.AppendToLog(Name + " was successfully added in 'My Shows'.",
                    ConfigurationManager.AppSettings["log_file"]);
                r = Name + " was successfully added in 'My Shows'.";
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }
        
        //Updated August 26th
        public static string RemoveMovieFromMyMovies(string name)
        {
            string r = "";
            try
            {
                var wc = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["mymovies_collection"]
                    ).Remove(Query.EQ("ImdbTitle", name));
                if (wc.Ok)
                {
                    Log.AppendToLog(name + " was successfully deleted in 'My Movies'.",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = name + " was successfully deleted in 'My Movies'.";
                }
                else
                {
                    Log.AppendToLog(name + " encountered an error before deletion." + "\n" + wc.ErrorMessage,
                        ConfigurationManager.AppSettings["log_file"]);
                    r = name + " encountered an error before deletion.";
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }

        //Updated July 19th
        public static List<Movie> GetMoviesToDownload()
        {
            List<Movie> resultsList = new List<Movie>();
            try
            {
                List<Movie> m = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["mymovies_collection"]
                ).FindAllAs<Movie>().ToList<Movie>();
                
                foreach (var movie in m)
                {
                    List<Movie> tempList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["movie_collection"]
                    ).FindAs<Movie>(Query.EQ("ImdbCode", movie.ImdbCode)).ToList<Movie>();
                    if (tempList.Count != 0 && tempList[0].DownloadLogistics != null)
                        resultsList.AddRange(tempList);
                }
                return resultsList;
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL GetMoviesToDownload " + e, ConfigurationManager.AppSettings["log_file"]); return resultsList; }
        }

        //Updated July 5th
        public static void SetToDownloaded(string name)
        {
            try
            {
                MongoCollection movieCollection = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["movie_collection"]
                );
                List<Movie> m = movieCollection.FindAs<Movie>(Query.EQ("Name", name)).ToList<Movie>();
                //below cannot be consolidated into other query as it is a subobject property 
                if (m.Count != 0)
                { m[0].Downloaded = true; movieCollection.Save(m[0]); }
                Log.AppendToLog("Downloaded movie updated in db.", ConfigurationManager.AppSettings["log_file"]);
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]); }
        }

        //Updated July 5th
        public static void MoveToDownloaded(string name)
        {
            try
            {
                MongoCollection downloadedCollection = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["downloaded_collection"]
                );

                MongoCollection pendingCollection = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["pending_collection"]
                );

                MongoCollection myMovieCollection = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["mymovies_collection"]
                );

                var q = Query.And(Query.EQ("Name", name));
                downloadedCollection.Insert(pendingCollection.FindOneAs<Pending>(Query.EQ("Name", name)));
                //var p = pendingCollection.FindOneAs<Pending>(Query.EQ("Name", name));
                pendingCollection.Remove(Query.EQ("Name", name));
                myMovieCollection.Remove(Query.EQ("ImdbTitle", name));
                Log.AppendToLog("Movie moved from pending collection to downloaded collection.", ConfigurationManager.AppSettings["log_file"]);
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]); }
        }

        //Updated July 24th
        public static bool IsInPending(Movie m)
        {
            try
            {
                List<Pending> pendingList = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["pending_collection"]
                ).FindAs<Pending>(Query.EQ("Name", m.YtsMovieTitle)).ToList<Pending>();
                if (pendingList.Count != 0) return true;
                else return false;
            }
            catch (Exception e)
            { Log.AppendToLog("Error : Checking pending movies. " + e, ConfigurationManager.AppSettings["log_file"]); return false; }
        }

        //Wrote December 23rd
        public static bool IsInDownloaded(Movie m)
        {
            try
            {
                List<Pending> downloadedList = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    ConfigurationManager.AppSettings["downloaded_collection"]
                ).FindAs<Pending>(Query.EQ("Name", m.YtsMovieTitle)).ToList<Pending>();
                if (downloadedList.Count != 0) return true;
                else return false;
            }
            catch (Exception e)
            { Log.AppendToLog("Error : Checking pending movies. " + e, ConfigurationManager.AppSettings["log_file"]); return false; }
        }

        //Updated July 1st
        public static string NameFromDownloadString(string name)
        {
            try
            { return Encoding.Default.GetString(File.ReadAllBytes(ConfigurationManager.AppSettings["temp_torrent_download_path"] + @"\" + name + ".torrent")); }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL Conversion of movie file to string : " + e, ConfigurationManager.AppSettings["log_file"]); return ""; }
        }

        //Updated August 1st
        public static void RefreshMoviesThread()
        {
            Console.WriteLine("Refresh movies thread started...");
            while (true)
            {
                //WriteImdbMovies();
                WriteYtsMovieData();
                //Sleep for specified time, limit requests to server
                Thread.Sleep(ConfigurationManager.AppSettings["refreshInterval"].ToInt());
            }
        }
    }
}
