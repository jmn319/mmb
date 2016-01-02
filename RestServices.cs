using System;
using System.Linq;
using System.Collections.Generic;
using ServiceStack.Text;
using System.Configuration;
using mmb.ObjectDefinitions;
using ServiceStack.Redis; //use in the future?
using MongoDB.Driver;
using ServiceStack;

namespace mmb
{
    //TODO: Decide how much error handling we need here... I suspect not much. 
    [Route("/get/{MediaType}/{All}/{Pending}/{Downloaded}", "GET")]
    public class GetInfoRequest
    {
        public string Name { get; set; }
        public string Season { get; set; }
        public string Episode { get; set; }
        public string MediaType { get; set; }
        public bool Show { get; set; }
        public bool Movie { get; set; }
        public string Pending { get; set; }
        public string All { get; set; }
        public string Downloaded { get; set; }
        public bool Saved { get; set; }
    }

    [Route("/get", "POST")]
    public class PostInfoRequest : GetObject
    { }

    [Route("/modify", "POST")]
    public class ModifyInfoRequest : ModifyObject
    {  }

    [Route("/search/{MediaType}/{Local}/{Query}", "GET")]
    [Route("/search", "POST")]
    public class Search
    {
        public string MediaType { get; set; }
        public string Local { get; set; }
        public string Query { get; set; }
    }

    [Route("/shows/myshows/update/{Name}/{Season}/{Episode}", "GET")]
    [Route("/shows/myshows/update", "POST")]
    public class UpdateShowMyShows
    {
        public string Name { get; set; }
        public string Season { get; set; }
        public string Episode { get; set; }
    }

   [Route("/rebuild", "GET")]
    [Route("/rebuild", "POST")]
    public class RebuildAll
    { public string Name { get; set; } }

    [EnableCors]
    internal class Services : Service
    {
        public object Get(GetInfoRequest request)
        {
            //get/{MediaType}/{All}/{Pending}/{Downloaded}
            if (request.MediaType == "show")
            {
                if (request.Pending == "true")
                {
                    Console.WriteLine("Pending shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(true, true)); 
                }
                else if (request.Downloaded == "true") 
                {
                    Console.WriteLine("Downloaded shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(true, false)); 
                }
                else
                {
                    if (request.All == "true")
                    {
                        Console.WriteLine("All shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetShows(true));  
                    }
                    else
                    {
                        Console.WriteLine("My shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetShows(false)); 
                    }
                }
            }

            else
            {
                if (request.Pending == "true")
                {
                    Console.WriteLine("Pending movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(false, true));
                }
                else if (request.Downloaded == "true")
                {
                    Console.WriteLine("Downloaded movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(false, false));
                }
                else
                {
                    if (request.All == "true")
                    {
                        Console.WriteLine("All movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetMovies(true));
                    }
                    else
                    {
                        Console.WriteLine("My movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetMovies(false));
                    }
                }
            }
        }

        public object Post(PostInfoRequest request)
        {
            if (request.MediaType == "show")
            {
                if (request.Pending)
                {
                    Console.WriteLine("Pending shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(true, true));
                }
                else if (request.Downloaded)
                {
                    Console.WriteLine("Downloaded shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(true, false));
                }
                else
                {
                    if (request.All)
                    {
                        Console.WriteLine("All shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetShows(true));
                    }
                    else
                    {
                        Console.WriteLine("My shows request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetShows(false));
                    }
                }
            }

            else
            {
                if (request.Pending)
                {
                    Console.WriteLine("Pending movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(false, true));
                }
                else if (request.Downloaded)
                {
                    Console.WriteLine("Downloaded movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetPendingDownloaded(false, false));
                }
                else
                {
                    if (request.All)
                    {
                        Console.WriteLine("All movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetMovies(true));
                    }
                    else
                    {
                        Console.WriteLine("My movies request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                        return JsonSerializer.SerializeToString(RestUtils.GetMovies(false));
                    }
                }
            }
        }

        public object Get(Search request)
        {
            if (request.MediaType == "show")
            {
                if (request.Local == "true")
                {
                    Console.WriteLine("Local show search request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetLocalSearch(true, request.Query));
                }

                else
                {
                    Console.WriteLine("External show search request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    //TODO: Implement this funationality
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = "I don't understand your request... yet" }); 
                }
            }
            else
            {
                if (request.Local == "true")
                {
                    Console.WriteLine("Local movie search request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetLocalSearch(false, request.Query));
                }

                else
                {
                    Console.WriteLine("IMDB search request: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(RestUtils.GetSearchImdbMovies(request.Query));
                }   
            }
        }

        public object Post(Search request)
        {
            Console.WriteLine("Search post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
            return Get(request);
        }

        public object Post(ModifyInfoRequest request)
        {
            //Handle null objects on the client side code
            if (request.Show)
            {
                if (request.ModType == "delete")
                {
                    Console.WriteLine("Delete show post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = TvUtils.RemoveShowFromMyShow(request.Name)});
                }
                else if (request.ModType == "add")
                {
                    Console.WriteLine("Add show post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", 
                        Value = TvUtils.AddShowToMyShows(request.Name, request.Season, request.Episode)});
                }
                else if (request.ModType == "modify")
                {
                    Console.WriteLine("Modify show post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", 
                        Value = TvUtils.UpdateMyShow(request.Name, request.Season, request.Episode)});
                }
                else
                { return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = "I don't understand your request"}); }
            }
            else
            {
                if (request.ModType == "delete")
                {
                    Console.WriteLine("Delete movie post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = MovieUtils.RemoveMovieFromMyMovies(request.Name) });
                }
                else if (request.ModType == "add")
                {
                    Console.WriteLine("Add movie post: " + DateTime.Now.ToString("M/d/yyyy H:mm:ss:ff"));
                    return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response",
                        Value = MovieUtils.AddMovieToMyMovies(request.Name, request.ImdbCode, request.Year) });
                }
                else if (request.ModType == "modify")
                //TODO: build this out... or not? is it needed?
                { return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = "I don't understand your request... yet" }); }
                else
                { return JsonSerializer.SerializeToString(new ReturnClass() { Key = "response", Value = "I don't understand your request" }); }
            }
        }
    }

    public class ReturnClass
    {
        public object Key { get; set; }
        public object Value { get; set; }
    }

    public class ShowReturnClass
    {
        public object Name { get; set; }
        public object Season { get; set; }
        public object Episode { get; set; }
    }

    public class MovieReturnClass
    { public object Name { get; set; } }

    public class NestedShowReturnClass
    { public Dictionary<string, List<List<MovieReturnClass>>> Show { get; set; } }

    public class AppHost : AppHostHttpListenerBase
    {
        public AppHost() : base("AppHost", typeof (Services).Assembly)
        { }

        public override void Configure(Funq.Container container)
        { Plugins.Add(new CorsFeature(allowedMethods: "GET, POST")); }
    }
}
