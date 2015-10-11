using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using AutoMapper;
using HtmlAgilityPack;
using mmb.ObjectDefinitions;
using MongoDB.Driver;
using MongoDB.Driver.Builders;
using ServiceStack;
using ServiceStack.Support.Markdown;

namespace mmb
{
    public class TvUtils
    {
        //Updated July 15th
        public static void WriteShows()
        {
            try
            {
                HtmlNodeCollection collection = DownloadMgmtUtils.GetHtmlNodeCollection
                    (ConfigurationManager.AppSettings["show_url"] +
                     ConfigurationManager.AppSettings["show_url_list_postfix"], "//a");

                MongoCollection mongoCollection = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    );

                foreach (
                    HtmlNode link in
                        collection.Where(link => !ConfigurationManager.AppSettings["show_innerhtml_excl"].Split(',')
                            .Any(s => link.InnerHtml.Contains(s))).Where(link => link.Attributes[0].Value != null))
                {
                    mongoCollection.Insert(new TvShow() {Name = link.InnerHtml, Path = link.Attributes[0].Value});
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL Write Show Issue : " + e, ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated July 30th
        public static void RefreshShows()
        {
            try
            {
                HtmlNodeCollection collection = DownloadMgmtUtils.GetHtmlNodeCollection
                    (ConfigurationManager.AppSettings["show_url"] +
                     ConfigurationManager.AppSettings["show_url_list_postfix"], "//a");

                MongoCollection mongoCollection = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    );

                foreach (HtmlNode link in from link in collection
                    where
                        !ConfigurationManager.AppSettings["show_innerhtml_excl"].Split(',')
                            .Any(s => link.InnerHtml.Contains(s))
                    let tvShowList =
                        mongoCollection.FindAs<TvShow>(Query.EQ("Name", link.InnerHtml)).ToList<TvShow>()
                    where tvShowList.Count == 0
                    select link)
                {
                    mongoCollection.Insert(new TvShow() {Name = link.InnerHtml, Path = link.Attributes[0].Value});
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL Refresh Show Issue : " + e, ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated July 30th - Combined with WriteEpisodes
        public static void RefreshEpisodes()
        {
            try
            {
                MongoCollection mongoCollection = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    );

                foreach (var show in mongoCollection.FindAllAs<TvShow>().ToList<TvShow>())
                {
                    if (!ConfigurationManager.AppSettings["show_excl"].Split(',').Any(s => show.Name.Contains(s)))
                    {
                        HtmlNodeCollection collection =
                            DownloadMgmtUtils.GetHtmlNodeCollection(
                                ConfigurationManager.AppSettings["show_url"] + show.Path, "//a");

                        if (collection != null)
                            foreach (var link in collection.Where(link =>
                                !ConfigurationManager.AppSettings["episode_innerhtml_excl"].Split(',')
                                    .Any(s => link.InnerHtml.Contains(s)) &&
                                link.Attributes.Count > 0 && !link.Attributes[0].Value.Contains("tvnews") &&
                                link.Attributes[HrefIndex(link)].Value.Contains("/ep")))
                                UpdateShowFromHtml(show, link.InnerHtml, collection, collection.IndexOf(link));

                        mongoCollection.Save(show);
                    }
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL Refresh Episodes Issue : " + e,
                    ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated August 1st
        public static void RebuildAll(string path)
        {
            try
            {
                MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    ).Drop();

                WriteShows();
                RefreshEpisodes();
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : " + e, ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //TODO: Re-write
        public static List<int> SeFromString(string name)
        {
            name = name.ToLowerInvariant()
                .Replace("xdiv", "")
                .Replace("xvid", "")
                .Replace("x264", "")
                .Replace("divx", "");
            List<char> characters = name.ToCharArray().ToList<char>();
            List<int> result = new List<int>();
            if (characters.Contains('x') && characters.IndexOf('x') != 0 &&
                Char.IsNumber(characters[characters.IndexOf('x') - 1]))
                for (int i = 2; i < characters.Count - 2; i++)
                {
                    if (characters[i] == 'x')
                        if (Char.IsNumber(characters[i - 1]))
                            if (Char.IsNumber(characters[i - 2]))
                            {
                                result.Add(
                                    Convert.ToInt32(String.Concat(characters[i - 2].ToString(),
                                        (string) characters[i - 1].ToString())));
                                if (Char.IsNumber(characters[i + 1]))
                                    if (Char.IsNumber(characters[i + 2]))
                                    {
                                        result.Add(
                                            Convert.ToInt32(String.Concat(characters[i + 1].ToString(),
                                                (string) characters[i + 2].ToString())));
                                        break;
                                    }
                                    else
                                    {
                                        result.Add(Convert.ToInt32(characters[i + 1].ToString()));
                                        break;
                                    }
                            }
                            else
                            {
                                result.Add(Convert.ToInt32(characters[i - 1].ToString()));
                                if (Char.IsNumber(characters[i + 1]))
                                    if (Char.IsNumber(characters[i + 2]))
                                    {
                                        result.Add(
                                            Convert.ToInt32(String.Concat(characters[i + 1].ToString(),
                                                (string) characters[i + 2].ToString())));
                                        break;
                                    }
                                    else
                                    {
                                        result.Add(Convert.ToInt32(characters[i + 1].ToString()));
                                        break;
                                    }
                            }
                        else ;
                    else if (i == characters.Count - 3)
                    {
                        result.Add(0);
                        result.Add(0);
                    }
                }
            else if (characters.Contains('s') &&
                     (characters.IndexOf('s') != 0 ||
                      characters.FindAll(delegate(char a) { return a == 's'; }).Count != 1))
                for (int i = 2; i < characters.Count - 5; i++)
                {
                    if (characters[i] == 's' && i != characters.Count - 6)
                        if (Char.IsNumber(characters[i + 1]))
                            if (Char.IsNumber(characters[i + 2]))
                            {
                                result.Add(
                                    Convert.ToInt32(String.Concat(characters[i + 1].ToString(),
                                        (string) characters[i + 2].ToString())));
                                if (Char.IsNumber(characters[i + 4]))
                                    if (Char.IsNumber(characters[i + 5]))
                                    {
                                        result.Add(
                                            Convert.ToInt32(String.Concat(characters[i + 4].ToString(),
                                                (string) characters[i + 5].ToString())));
                                        break;
                                    }
                                    else
                                    {
                                        result.Add(Convert.ToInt32(characters[i + 4].ToString()));
                                        break;
                                    }
                            }
                            else
                            {
                                result.Add(Convert.ToInt32(characters[i + 1].ToString()));
                                if (Char.IsNumber(characters[i + 3]))
                                    if (Char.IsNumber(characters[i + 4]))
                                    {
                                        result.Add(
                                            Convert.ToInt32(String.Concat(characters[i + 3].ToString(),
                                                (string) characters[i + 4].ToString())));
                                        break;
                                    }
                                    else
                                    {
                                        result.Add(Convert.ToInt32(characters[i + 3].ToString()));
                                        break;
                                    }
                            }
                        else ;
                    else if (i == characters.Count - 6)
                    {
                        result.Add(0);
                        result.Add(0);
                    }
                }
            else
            {
                result.Add(0);
                result.Add(0);
            }
            return result;
        }

        //TODO: Fill in later to parse specificall
        public static List<int> SeFromDiscoveryString(string name)
        {
            return new List<int>();
        }

        //Updated July 1st
        public static string AddShowToMyShows(string showName, int season = 1, int episode = 1)
        {
            string r = "";
            try
            {
                List<TvShow> tvShowList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    ).FindAs<TvShow>(Query.EQ("Name", showName)).ToList<TvShow>();

                if (tvShowList.Count != 0)
                {
                    List<BasicShow> myShowList = MongoUtils.GetMongoCollection
                        (
                            @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                            ConfigurationManager.AppSettings["port"],
                            ConfigurationManager.AppSettings["db"],
                            ConfigurationManager.AppSettings["myshows_collection"]
                        ).FindAs<BasicShow>(Query.EQ("Name", showName)).ToList<BasicShow>();

                    if (myShowList.Count == 0)
                    {
                        MongoUtils.GetMongoCollection
                            (
                                @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                                ConfigurationManager.AppSettings["port"],
                                ConfigurationManager.AppSettings["db"],
                                ConfigurationManager.AppSettings["myshows_collection"]
                            ).Insert(new BasicShow() {Name = tvShowList[0].Name, Episode = episode, Season = season});
                        Log.AppendToLog(showName + " was successfully inserted in 'My Shows'.",
                            ConfigurationManager.AppSettings["log_file"]);
                        r = showName + " was successfully inserted in 'My Shows'.";
                    }
                    else
                    {
                        Log.AppendToLog(showName + " already exists in 'My Shows'.",
                            ConfigurationManager.AppSettings["log_file"]);
                        r = showName + " already exists in 'My Shows'.";
                    }
                }
                else
                {
                    Log.AppendToLog("Error : There is no show by that name in 'Shows'!",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = "Error : There is no show by that name in 'Shows'!";
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }

        //Updated July 1st
        public static string UpdateMyShow(string showName, int season = 1, int episode = 1)
        {
            string r = "";
            try
            {
                List<BasicShow> myShowList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["myshows_collection"]
                    ).FindAs<BasicShow>(Query.EQ("Name", showName)).ToList<BasicShow>();

                if (myShowList.Count != 0)
                {
                    myShowList[0].Season = season;
                    myShowList[0].Episode = episode;
                    MongoUtils.GetMongoCollection
                        (
                            @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                            ConfigurationManager.AppSettings["port"],
                            ConfigurationManager.AppSettings["db"],
                            ConfigurationManager.AppSettings["myshows_collection"]
                        ).Save(myShowList[0]);
                    Log.AppendToLog(showName + " was successfully updated in 'My Shows'.",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = showName + " was successfully updated in 'My Shows'.";
                }
                else
                {
                    Log.AppendToLog("Error : There is no show by that name in 'My Shows'!",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = "Error : There is no show by that name in 'My Shows'!";
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }

        //Updated August 16th
        public static string UpdateCheckMyShow(string showName, int season = 1, int episode = 1)
        {
            string r = "";
            try
            {
                List<BasicShow> myShowList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["myshows_collection"]
                    ).FindAs<BasicShow>(Query.EQ("Name", showName)).ToList<BasicShow>();

                if (myShowList.Count != 0)
                {
                    if (myShowList[0].Season < season)
                    {
                        myShowList[0].Season = season;
                        myShowList[0].Episode = episode;
                    }
                    else if (myShowList[0].Episode < episode)
                        myShowList[0].Episode = episode;
                    MongoUtils.GetMongoCollection
                        (
                            @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                            ConfigurationManager.AppSettings["port"],
                            ConfigurationManager.AppSettings["db"],
                            ConfigurationManager.AppSettings["myshows_collection"]
                        ).Save(myShowList[0]);
                    r = showName + " was successfully updated in 'My Shows'.";
                }
                else
                {
                    Log.AppendToLog("Error : There is no show by that name in 'My Shows'!",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = "Error : There is no show by that name in 'My Shows'!";
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }

        //Updated August 26th
        public static string RemoveShowFromMyShow(string showName)
        {
            string r = "";
            try
            {
                var wc = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["myshows_collection"]
                    ).Remove(Query.EQ("Name", showName));
                if (wc.Ok)
                {
                    Log.AppendToLog(showName + " was successfully deleted in 'My Shows'.",
                        ConfigurationManager.AppSettings["log_file"]);
                    r = showName + " was successfully deleted in 'My Shows'.";
                }
                else
                {
                    Log.AppendToLog(showName + " encountered an error before deletion." + "\n" + wc.ErrorMessage,
                        ConfigurationManager.AppSettings["log_file"]);
                    r = showName + " encountered an error before deletion.";
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
                r = "Error : FATAL " + e;
            }
            return r;
        }

        //Pull video filename out of the torrent file
        //Updated July 1st
        public static string NameFromDownloadString(string name, string season, string episode)
        {
            try
            {
                return
                    Encoding.Default.GetString(
                        File.ReadAllBytes(ConfigurationManager.AppSettings["temp_torrent_download_path"] + @"\" +
                                          name + " S" + season + "E" + episode + ".torrent"));
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL Conversion of file FileName to string : " + e,
                    ConfigurationManager.AppSettings["log_file"]);
                return "";
            }
        }

        //TODO: fill in: what exactly should this do?
        public static void ResetDownloadedInShows()
        {

        }

        //TODO: Increase efficiency here?
        public static int HrefIndex(HtmlNode node)
        {
            int r = 0;
            for (int i = 0; i < node.Attributes.Count; i++)
            {
                if (node.Attributes[i].Name == "href")
                {
                    r = i;
                    break;
                }
            }
            return r;
        }

        //Error handling should be embedded in execution methods, not runner methods
        public static void RefreshShowsThread()
        {
            Console.WriteLine("Refresh shows thread started...");
            while (true)
            {
                RefreshShows();
                //Sleep for Xmin, limit requests to server
                Thread.Sleep(ConfigurationManager.AppSettings["refreshInterval"].ToInt());
            }
        }

        //Error handling should be embedded in execution methods, not runner methods
        public static void RefreshEpisodesThread()
        {
            Console.WriteLine("Refresh episodes thread started...");
            while (true)
            {
                RefreshEpisodes();
                //Sleep for 10min, limit requests to server
                Thread.Sleep(ConfigurationManager.AppSettings["refreshInterval"].ToInt());
            }
        }

        //Updated July 19th
        public static List<BasicShow> GetEpisodesToDownload()
        {
            List<BasicShow> resultsList = new List<BasicShow>();
            try
            {
                List<BasicShow> basicShowsList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["myshows_collection"]
                    ).FindAllAs<BasicShow>().ToList<BasicShow>();

                foreach (var show in basicShowsList)
                {
                    List<TvShow> tvShowsList = MongoUtils.GetMongoCollection
                        (
                            @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                            ConfigurationManager.AppSettings["port"],
                            ConfigurationManager.AppSettings["db"],
                            ConfigurationManager.AppSettings["show_collection"]
                        ).FindAs<TvShow>(Query.EQ("Name", show.Name)).ToList<TvShow>();

                    foreach (var season in tvShowsList[0].Seasons)
                    {
                        if (season.Number == show.Season)
                            resultsList.AddRange(from e in season.Episodes
                                where
                                    e.Number > show.Episode || (e.Number == 1 && show.Season == 1 && show.Episode == 1)
                                select new BasicShow()
                                {
                                    Name = show.Name,
                                    Season = season.Number,
                                    Episode = e.Number
                                });

                        else if (season.Number > show.Season)
                            resultsList.AddRange(season.Episodes.Select(episode => new BasicShow()
                            {
                                Name = show.Name,
                                Season = season.Number,
                                Episode = episode.Number
                            }));
                    }
                }
                return resultsList;
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL GetEpisodesToDownload " + e, ConfigurationManager.AppSettings["log_file"]);
                return resultsList;
            }
        }

        //Updated August 15th
        public static void SetShowDownloadLocations(List<BasicShow> showsToDownload)
        {
            try
            {
                foreach (var e in showsToDownload)
                {
                    if (IsInPending(e)) ;
                    else
                    {
                        List<TvShow> tvShowsList = MongoUtils.GetMongoCollection
                            (
                                @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                                ConfigurationManager.AppSettings["port"],
                                ConfigurationManager.AppSettings["db"],
                                ConfigurationManager.AppSettings["show_collection"]
                            ).FindAs<TvShow>(Query.EQ("Name", e.Name)).ToList<TvShow>();

                        var officialEpisode = from s in tvShowsList[0].Seasons
                            where s.Number == e.Season
                            from epi in s.Episodes
                            where epi.Number == e.Episode
                            select epi;

                        //Grab a download location that is not from the exclusion list and set it for the current episode
                        foreach (var download_perf in ConfigurationManager.AppSettings["download_perf"].Split(','))
                        {
                            foreach ( var d in officialEpisode.ToList<Episode>()[0].DownloadLocations.Where(d => d.Contains(download_perf)))
                            { showsToDownload[showsToDownload.IndexOf(e)].DownloadLocation = d;  break; }
                        }

                        if (showsToDownload[showsToDownload.IndexOf(e)].DownloadLocation == null)
                            foreach (var d in officialEpisode.ToList<Episode>()[0].DownloadLocations.Where(d =>
                                !ConfigurationManager.AppSettings["download_excl"].Split(',').Any(s => d.Contains(s)) &&
                                d.Substring(0, 1) != @"/"))
                                { showsToDownload[showsToDownload.IndexOf(e)].DownloadLocation = d;  break; }
                    }
                }
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL set download locations : " + e,
                    ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated July 5th
        public static void SetToDownloaded(string showName, int season, int episode)
        {
            try
            {
                MongoCollection showCollection = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    );
                List<TvShow> show = showCollection.FindAs<TvShow>(Query.EQ("Name", showName)).ToList<TvShow>();
                //below cannot be consolidated into other query as it is a subobject property 
                foreach (
                    var e in
                        from s in show[0].Seasons
                        where s.Number == season
                        from e in s.Episodes
                        where e.Number == episode
                        select e)
                    e.Downloaded = true;
                showCollection.Save(show[0]);
                Log.AppendToLog("Downloaded show updated in db.", ConfigurationManager.AppSettings["log_file"]);
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : FATAL " + e, ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated August 16th
        public static void MoveToDownloaded(string name, int season, int episode)
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

                var q = Query.And(Query.EQ("Name", name), Query.EQ("Season", season), Query.EQ("Episode", episode));
                downloadedCollection.Insert(pendingCollection.FindAs<Pending>(q));
                pendingCollection.Remove(q, RemoveFlags.Single);
                Log.AppendToLog("Error : Show moved from pending collection to downloaded collection.",
                    ConfigurationManager.AppSettings["log_file"]);
            }
            catch (Exception e)
            {
                Log.AppendToLog(": FATAL Show not moved from pending." + e, ConfigurationManager.AppSettings["log_file"]);
            }
        }

        //Updated July 24th
        public static bool IsInPending(BasicShow show)
        {
            try
            {
                List<Pending> pendingList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["pending_collection"]
                    ).FindAs<Pending>(Query.And
                        (
                            Query.EQ("Name", show.Name),
                            Query.EQ("Season", show.Season),
                            Query.EQ("Episode", show.Episode))
                    ).ToList<Pending>();
                if (pendingList.Count != 0) return true;
                else return false;
            }
            catch (Exception e)
            {
                Log.AppendToLog("Error : Checking pending shows. " + e, ConfigurationManager.AppSettings["log_file"]);
                return false;
            }
        }

        //Updated July 30th
        public static void UpdateShowFromHtml(TvShow show, string InnerHtml, HtmlNodeCollection collection, int parentNodeLocation)
        {
            //http://eztv.it/forum/30969/10-oclock-live-s02e10-hdtv-x264-c4tv/
            try
            {
                List<int> seInfo = SeFromString(InnerHtml);
                if (seInfo.Count != 0)
                {
                    string episodeName = InnerHtml;
                    List<string> downloadLocation = new List<string>();

                    //Populate download locations list
                    for (int j = parentNodeLocation; j < collection.Count - 1; j++)
                    {
                        if (collection[j].Attributes.Count > 0 && collection[j].Attributes[0].Value.Contains("forum"))
                            break;
                        else
                            for (int p = 0; p < (int) Math.Min(collection[j].Attributes.Count, 15.0); p++)
                                if (collection[j].Attributes[p].Name == "title" &&
                                    collection[j].Attributes[p].Value.Contains("Download M"))
                                    downloadLocation.Add(collection[j].Attributes[0].Value);
                    }
                    //No seasons exist
                    if (show.Seasons == null)
                    {
                        if (seInfo.Count != 0)

                            #region Create New Season

                            show.Seasons = new List<Season>()
                            {
                                new Season()
                                {
                                    Name = seInfo[0].ToString(),
                                    Number = seInfo[0],
                                    Episodes = new List<Episode>()
                                    {
                                        new Episode()
                                        {
                                            Name = episodeName,
                                            Number = seInfo[1],
                                            DownloadLocations = downloadLocation
                                        }
                                    }
                                }
                            };

                        #endregion
                    }
                    //Seasons exist and season/episode data is non-null
                    else if (seInfo.Count != 0)
                    {
                        //if there does not exist a season with the number from season/episode informatioon
                        if (!show.Seasons.Exists(s => s.Number == seInfo[0]))

                            #region Create New Season

                            show.Seasons.Add(
                                new Season()
                                {
                                    Name = seInfo[0].ToString(),
                                    Number = seInfo[0],
                                    Episodes = new List<Episode>()
                                    {
                                        new Episode()
                                        {
                                            Name = episodeName,
                                            Number = seInfo[1],
                                            DownloadLocations = downloadLocation
                                        }
                                    }
                                });
                            #endregion

                        else
                        //grab season where season is equal to the current season and the epsiode is new
                            foreach (
                                var s in
                                    show.Seasons.Where(s => s.Number == seInfo[0])
                                        .Where(s => !s.Episodes.Exists(x => x.Number == seInfo[1])))
                            {
                                show.Seasons[show.Seasons.IndexOf(s)].Episodes.Add(
                                    new Episode()
                                    {
                                        Name = episodeName,
                                        Number = seInfo[1],
                                        DownloadLocations = downloadLocation
                                    });
                            }

                        //grab season where season is the current season and the epsiode is the current episode
                        foreach(var s in show.Seasons.Where(s => s.Number == seInfo[0]))
                            foreach (var e in from e in s.Episodes where e.Number == seInfo[1] select e)
                                //Append to list if the episode was already loaded into the db
                                show.Seasons[show.Seasons.IndexOf(s)].Episodes[s.Episodes.IndexOf(e)].DownloadLocations =
                                    show.Seasons[show.Seasons.IndexOf(s)].Episodes[s.Episodes.IndexOf(e)].DownloadLocations.Union(
                                        downloadLocation).ToList<string>();
                    }
                }
            }
            catch (Exception e)
            { Log.AppendToLog("Error : FATAL UpdateShowFromHtml " + e, ConfigurationManager.AppSettings["log_file"]); }
        }
    }
}
