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
    public class LinkTour
    {
        public String link;
        public int tour, countPage;

        public LinkTour(String _link, int _tour, int _countPage)
        {
            link = _link;
            tour = _tour;
            countPage = _countPage;
        }
    }

    public class HtmlFile
    {
        private static SQLiteConnection connection;
        private static List<Stream> streams;

        public HtmlFile()
        {
            const string databaseName = @"FantasyFootballGerman.db";
            connection =
                new SQLiteConnection(string.Format("Data Source={0};", databaseName));
            streams = new List<Stream>();

            connection.Open();
        }

        private static void DownloadFile(Object _link)
        {
            String link = _link as String;

            WebRequest request = WebRequest.Create(link);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

            streams.Add(stream);

            HtmlDocument htmlDoc = new HtmlDocument();
            htmlDoc.Load(stream, Encoding.UTF8);

            ParseAndInsert(htmlDoc);

            response.Close();

            Console.WriteLine("Work done for " + link);
        }

        public static void ParseAndInsert(HtmlDocument doc)
        {
            HtmlNodeCollection elements = doc.DocumentNode.SelectNodes("//td[@class='name-td alLeft bordR']//a[@class='bold']");

            HtmlNodeCollection elementsPoints = doc.DocumentNode.SelectNodes("//td[@class='padR20']");

            int i = 1;
            foreach (var node in elements)
            {
                SQLiteCommand command4 = new SQLiteCommand("Select id, name from users where name='" + node.InnerText + "'", connection);
                SQLiteDataReader sqlReader2 = command4.ExecuteReader();
                //Console.WriteLine(sqlReader2[0].ToString() + "" + sqlReader2[1].ToString() + " " + node.InnerText);
                //Console.ReadLine();

                if (!sqlReader2.HasRows)
                {
                    lock (connection)
                    {
                        SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Users' ('ID','Name', 'Link') VALUES (NULL, '" + node.InnerText + "', '" + node.Attributes["href"].Value + "');", connection);
                        command.ExecuteNonQuery();
                    }
                }

                SQLiteCommand command3 = new SQLiteCommand("Select id from users where name='" + node.InnerText + "'", connection);
                SQLiteDataReader sqlReader = command3.ExecuteReader();

                int id = 0;
                if (sqlReader.HasRows)
                {
                    foreach (DbDataRecord record in sqlReader)
                    {
                        id = Convert.ToInt32(record[0].ToString());
                    }

                }

                if (id == 0)
                {
                    connection.Close();
                    Console.WriteLine("Error to enter line into Points");
                    Console.ReadLine();
                    return;
                }

                SQLiteCommand command5 = new SQLiteCommand("Select counts from points where id=" + id + " and tour=3", connection);
                SQLiteDataReader sqlReader3 = command5.ExecuteReader();

                if (!sqlReader3.HasRows)
                {
                    lock (connection)
                    {
                        SQLiteCommand command2 = new SQLiteCommand("INSERT INTO 'Points' ('ID','Tour', 'Counts') VALUES (" + id + ", 3, " + elementsPoints[i].InnerText + ");", connection);
                        command2.ExecuteNonQuery();
                    }
                }
                else
                    Console.WriteLine("Line in table: Points is exists. Id="+id);

                ++i;
            }
        }

        public void DownloadManager(Object _link)
        {
            LinkTour kvp = _link as LinkTour;

            String link = kvp.link;
            int tour = kvp.tour;
            int page = kvp.countPage;
            List<Thread> threads = new List<Thread>();

            for (int i = 1, firstVal = 0, lastVal = 0; i <= page; ++i, ++firstVal)
            {
                if (firstVal == 0 && lastVal == 0)
                {
                    firstVal = i;
                    if (i + 5 > page)
                        lastVal = i;
                    else
                        lastVal = i + 5;
                }

                Thread thread = new Thread(new ParameterizedThreadStart(HtmlFile.DownloadFile));

                link = link + i.ToString();

                thread.Start(link);

                threads.Add(thread);

                link = link.Remove(75);

                if (firstVal == lastVal)
                {
                    firstVal = -1; lastVal = 0;

                    for (int j = 0; j < threads.Count; ++j)
                    {
                        if (threads[j].IsAlive)
                        {
                            Thread.Sleep(100);
                            j = 0;
                            continue;
                        }
                    }
                    foreach (var th in threads)
                        th.Abort();

                    threads.Clear();
                    Console.WriteLine(i);
                }
            }

            //connection.Open();

            //HtmlDocument doc = new HtmlDocument();

            //foreach (var stream in streams)
            //{
            //    doc.Load(stream, Encoding.UTF8);

            //    ParseAndInsert(doc);
            //}

            connection.Close();
        }
    }
    class Program
    {
        
        static void Main(string[] args)
        {
            HtmlFile file = new HtmlFile();

            String link = @"http://www.sports.ru/fantasy/football/tournament/ratings/leaders/50.html?p=";
            int tour = 3, page = 130;

            file.DownloadManager(new LinkTour(link, tour, page));

            Console.WriteLine("Work done");

            Console.ReadLine();
        }
    }
}
