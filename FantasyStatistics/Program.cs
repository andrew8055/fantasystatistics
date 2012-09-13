using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;
using HtmlAgilityPack;
using System.Data.SQLite;
using System.Data.Common;

namespace FantasyStatistics
{
    class Program
    {
        
        static void Main(string[] args)
        {
            String link = @"http://www.sports.ru/fantasy/football/tournament/ratings/leaders/52.html?p=";
            int tour = 3, page = 290;
            String dbPath = @"FantasyFootballEngland.db";

            DownloadManager dw = new DownloadManager(new LinkTour(link, tour, page, dbPath));
            dw.Start();

            Console.ReadLine();
        }
    }
}
