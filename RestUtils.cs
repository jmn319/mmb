using ServiceStack.Text;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using mmb.ObjectDefinitions;
using MongoDB.Bson;
using MongoDB.Driver;

namespace mmb
{
    public class RestUtils
    {
        //Updated August 23rd
        public static List<GetReturnObject> GetShows(bool all)
        {
            List<GetReturnObject> resultList = new List<GetReturnObject>();
            try
            {
                var showList = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    all ? ConfigurationManager.AppSettings["show_collection"] : ConfigurationManager.AppSettings["myshows_collection"]
                ).FindAllAs<BsonDocument>().ToList<BsonDocument>();
                if (all)
                {
                    resultList.AddRange(showList.Select(s => new GetReturnObject()
                    {
                        Name = (string)s["Name"],
                        Movie = false,
                        Show = true
                    }));
                }
                else
                    resultList.AddRange(showList.Select(s => new GetReturnObject()
                    {
                        Name = (string)s["Name"],
                        Movie = false,
                        Show = true,
                        Episode = (int)s["Episode"],
                        Season = (int)s["Season"]
                    }));
                resultList = resultList.OrderBy(s => s.Name).ToList();
            }
            catch (Exception e)
            { Log.AppendToLog(" FATAL get shows error. " + e, ConfigurationManager.AppSettings["log_file"]); }
            return resultList;
        }

        //Updated August 23rd
        public static List<GetReturnObject> GetMovies(bool all)
        {
            List<GetReturnObject> resultList = new List<GetReturnObject>();
            try
            {
                List<BsonDocument> movieList = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    //all ? ConfigurationManager.AppSettings["movie_collection"] : ConfigurationManager.AppSettings["mymovies_collection"]
                    "yts_movies"
                ).FindAllAs<BsonDocument>().ToList<BsonDocument>();
                resultList.AddRange(movieList.Select(m => new GetReturnObject()
                {
                    //Name = m.ImdbTitle ?? m.YtsMovietitle, 
                    Movie = true, 
                    Show = false, 
                    //ImdbCode = m.ImdbCode, 
                    //Year = m.Year
                }));
                resultList = resultList.OrderBy(m => m.Name).ToList();
            }
            catch (Exception e)
            { Log.AppendToLog(" FATAL get movies error. " + e, ConfigurationManager.AppSettings["log_file"]); }
            return resultList;
        }

        //Updated August 23rd
        public static List<GetReturnObject> GetPendingDownloaded(bool isShow, bool isPending)
        {
            List<GetReturnObject> resultList = new List<GetReturnObject>();
            try
            {
                List<Pending> pendingList = MongoUtils.GetMongoCollection
                (
                    @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                    ConfigurationManager.AppSettings["port"],
                    ConfigurationManager.AppSettings["db"],
                    isPending ? ConfigurationManager.AppSettings["pending_collection"] : ConfigurationManager.AppSettings["downloaded_collection"]
                ).FindAllAs<Pending>().ToList<Pending>();
                resultList.AddRange(pendingList.Select(p => 
                    p.Show == isShow ? new GetReturnObject() {Name = p.Name, Movie = false, Show = true} : 
                    new GetReturnObject() {Name = p.Name, Movie = true, Show = false}));
                resultList = resultList.OrderBy(s => s.Name).ToList();
            }
            catch (Exception e)
            { Log.AppendToLog(" FATAL get pending/download error. " + e, ConfigurationManager.AppSettings["log_file"]); }
            return resultList;
        }

        //Updated August 19th
        public static List<GetReturnObject> GetSearchImdbMovies(string Query)
        {
            List<GetReturnObject> resultList = new List<GetReturnObject>();
            try
            {
                ImdbObject obj = JsonSerializer.DeserializeFromString<ImdbObject>
                    (DownloadMgmtUtils.GetImdbJsonQuery("http://www.imdb.com/xml/find?json=1&nr=0&tt=on&q=" + Query));
                List<ImdbElement> initialResultsList = new List<ImdbElement>();
                
                if (obj.title_exact == null)
                    if (obj.title_popular == null)
                        if (obj.title_substring == null)
                            initialResultsList.Add(new ImdbElement() { Title = "Error", Description = " not found"});
                        else
                            initialResultsList = obj.title_substring;
                    else
                        if (obj.title_substring == null)
                            initialResultsList = obj.title_popular;
                        else
                            initialResultsList = obj.title_substring.Union(obj.title_popular).ToList();
                else
                    if (obj.title_popular == null)
                        if (obj.title_substring == null)
                            initialResultsList = obj.title_exact;
                        else
                            initialResultsList = obj.title_substring.Union(obj.title_exact).ToList();
                    else
                        if (obj.title_substring == null)
                            initialResultsList = obj.title_popular.Union(obj.title_exact).ToList();
                        else
                            initialResultsList = obj.title_substring.Union(obj.title_exact.Union(obj.title_popular)).ToList();

                foreach (var e in initialResultsList)
                {
                    e.Description = e.Description.Split(' ')[0].Split(',')[0];
                    e.Title = e.Title.Split(',')[0];
                }
                List<ImdbElement> filteredResultList = initialResultsList.Where(e => !ConfigurationManager.AppSettings["imdb_search_excl"].Split(',').Any(s => e.Description.Contains(s))).ToList();
                filteredResultList.Sort(delegate(ImdbElement c1, ImdbElement c2) { return c2.Description.CompareTo(c1.Description); });
                foreach (var r in filteredResultList)
                {
                    resultList.Add(new GetReturnObject()
                    {
                        Movie = true,
                        Show = false,
                        Name = r.Title,
                        ImdbCode = r.Id,
                        Year = Convert.ToInt32(r.Description)
                    });
                }
            }
            catch (Exception e)
            { Log.AppendToLog(" FATAL search IMDB movies error. " + e, ConfigurationManager.AppSettings["log_file"]); }
            return resultList;
        }

        //Updated August 23rd
        public static List<GetReturnObject> GetLocalSearch(bool isShow, string Query)
        {
            List<GetReturnObject> resultList = new List<GetReturnObject>();
            try
            {
                if (isShow)
                {
                    List<TvShow> showList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["show_collection"]
                    ).FindAllAs<TvShow>().ToList<TvShow>();
                    showList = showList.Where(s => s.Name.ToLowerInvariant().Contains(Query.ToLowerInvariant())).ToList();
                    resultList.AddRange(showList.Select(s => new GetReturnObject() { Name = s.Name, Movie = false, Show = true }));
                    resultList = resultList.OrderBy(s => s.Name).ToList();
                }
                else
                {
                    List<Movie> movieList = MongoUtils.GetMongoCollection
                    (
                        @"mongodb://" + ConfigurationManager.AppSettings["mongoHost"] + @"/",
                        ConfigurationManager.AppSettings["port"],
                        ConfigurationManager.AppSettings["db"],
                        ConfigurationManager.AppSettings["movie_collection"]
                    //isShow ? ConfigurationManager.AppSettings["show_collection"] : ConfigurationManager.AppSettings["movie_collection"]
                    ).FindAllAs<Movie>().ToList<Movie>();
                    movieList = movieList.Where(m => m.ImdbTitle.ToLowerInvariant().Contains(Query.ToLowerInvariant())).ToList();
                    resultList.AddRange(movieList.Select(m => new GetReturnObject() { Name = m.ImdbTitle, Movie = true, Show = false }));
                    resultList = resultList.OrderBy(m => m.Name).ToList();
                }
            }
            catch (Exception e)
            { Log.AppendToLog(" FATAL local search error. " + e, ConfigurationManager.AppSettings["log_file"]); }
            return resultList;
        }
    }
}
