using System;
using System.Configuration;
using MongoDB.Driver;

namespace mmb
{
    class MongoUtils
    {
        //connect to mongo
        public static MongoCollection<Type> GetMongoCollection(string host, string port, string db, string collection)
        {
            try
            {
                MongoServer session = new MongoClient(host).GetServer();
                MongoDatabase dbInstance = session.GetDatabase(db);
                return dbInstance.GetCollection<Type>(collection);
            }
            catch (Exception e)
            { Log.AppendToLog("Error : " + e, ConfigurationManager.AppSettings["log_file"]); return null; }
        }
        
        public static MongoDatabase GetMongoDb(string host, string port, string db)
        {
            try
            {
                MongoServer session = new MongoClient(host).GetServer();
                return session.GetDatabase(db);
            }
            catch (Exception e)
            { Log.AppendToLog("Error : " + e, ConfigurationManager.AppSettings["log_file"]); return null; }
        }
    }
}
