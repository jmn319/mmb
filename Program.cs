using System.Threading;

namespace mmb
{
    class Program
    {
        static void Main(string[] args)
        {
            //fill with threaded tasks
            //BotCore.WriteEpisodes();
            Thread aThread = new Thread(TvUtils.RefreshShowsThread);
            Thread bThread = new Thread(TvUtils.RefreshEpisodesThread);
            Thread cThread = new Thread(MovieUtils.RefreshMoviesThread);
            Thread dThread = new Thread(DownloadMgmtUtils.DownloadTorrentFilesThread);
            Thread eThread = new Thread(DownloadMgmtUtils.MonitorAndMoveThread);
            Thread fThread = new Thread(RestRunner.InitRestThread);

            aThread.Start();
            bThread.Start();
            cThread.Start();
            dThread.Start();
            eThread.Start();
            fThread.Start();

            //need any blocking logic or interaction logic? global update flags
            //reset all functionality?
        }
    }
}
