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
        private static void UpdateDB()
        {
            String path = "ConfigurationFile.ini";

            if (!File.Exists(path))
            {
                Console.WriteLine("File {0} not found", path);
                Console.ReadLine();
                return;
            }

            DownloadManager dw = DownloadManager.ReadIniFile(path);
            dw.Start();

            Console.ReadLine();
        }
   
        static void Main(string[] args)
        {
            if (args.Length.Equals(0))
            {
                UpdateDB();

                return;
            }

            foreach (var arg in args)
            {
                if (arg.Equals("-updateDB"))
                {
                    UpdateDB();

                    continue;
                }

                if (arg.Substring(0, 12).Equals("-topPlayers:"))
                {
                    String str = arg.Remove(0, 12);

                    int secondTour = Convert.ToInt32(str.Substring(0, str.IndexOf('-')));
                    int firstTour = Convert.ToInt32(str.Substring(str.IndexOf('-') + 1, str.Length - str.IndexOf('-') - 1));

                    String path = "ConfigurationFile.ini";

                    if (!File.Exists(path))
                    {
                        Console.WriteLine("File {0} not found", path);
                        Console.ReadLine();
                        return;
                    }

                    DownloadManager dw = DownloadManager.ReadIniFile(path);
                    dw.SelectTopPlayers(secondTour, firstTour);

                    continue;
                }
            }
            

            Console.ReadLine();
        }
    }
}
