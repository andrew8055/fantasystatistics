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
        private SQLiteConnection connection;
        //private static List<Stream> streams;
        private String link, fileName;
        private int tour;

        public HtmlFile(String _link, int _tour, String _fileName)
        {
            link = _link;
            tour = _tour;
            fileName = _fileName;

            const string databaseName = @"FantasyFootballGerman.db";
            connection =
                new SQLiteConnection(string.Format("Data Source={0};", databaseName));
            
            connection.Open();
        }

        public HtmlFile(int _tour, String _fileName)
        {
            tour = _tour;
            fileName = _fileName;

            const string databaseName = @"FantasyFootballGerman.db";
            connection =
                new SQLiteConnection(string.Format("Data Source={0};", databaseName));

            connection.Open();
        }

        public HtmlFile()
        {
            const string databaseName = @"FantasyFootballGerman.db";
            connection =
                new SQLiteConnection(string.Format("Data Source={0};", databaseName));
            //streams = new List<Stream>();

            
        }

        public void DownloadFile()
        {
            WebRequest request = WebRequest.Create(link);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();
            //streams.Add(stream);

            Directory.CreateDirectory(@"tmp");
            
            using (FileStream fileStream = File.Create(@"tmp\" + fileName))
            {
                byte[] inStream = new byte[8 * 1024];
                int len;

                while ((len = stream.Read(inStream, 0, inStream.Length)) > 0)
                {
                    fileStream.Write(inStream, 0, len);
                }
            }

            response.Close();



            //HtmlDocument htmlDoc = new HtmlDocument();

            //htmlDoc.Load(stream, Encoding.UTF8);

            //ParseAndInsert(htmlDoc);

            //response.Close();
            ////doneEvent.Set();

            //Console.WriteLine("Work done for " + link);
        }

        public int ParseAndInsert(HtmlDocument doc)
        {
            HtmlNodeCollection elements, elementsPoints;

            elements = doc.DocumentNode.SelectNodes("//td[@class='name-td alLeft bordR']//a[@class='bold']");

            elementsPoints = doc.DocumentNode.SelectNodes("//td[@class='padR20']");

            int i = 1;
            int schetchik = 0;

            foreach (var node in elements)
            {
                SQLiteCommand command4;
                SQLiteDataReader sqlReader2;

                //lock (connection)
                //{
                    command4 = new SQLiteCommand("Select id, name from users where name='" + node.InnerText + "'", connection);
                    sqlReader2 = command4.ExecuteReader();
                //}
                //Console.WriteLine(sqlReader2[0].ToString() + "" + sqlReader2[1].ToString() + " " + node.InnerText);
                //Console.ReadLine();

                if (!sqlReader2.HasRows)
                {
                    //lock (connection)
                    //{

                    SQLiteCommand command = new SQLiteCommand("INSERT INTO 'Users' ('ID','Name', 'Link') VALUES (NULL, '" + node.InnerText + "', '" + node.Attributes["href"].Value + "');", connection);
                    command.ExecuteNonQuery();
                    //}
                }
                else 
                {
                    //Console.WriteLine("Name: " + node.InnerText + " is exists");
                    ++schetchik;
                }

                SQLiteCommand command3;
                SQLiteDataReader sqlReader;

                //lock (connection)
                //{
                    command3 = new SQLiteCommand("Select id from users where name='" + node.InnerText + "'", connection);
                    sqlReader = command3.ExecuteReader();
                //}

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
                    return 0;
                }

                SQLiteCommand command5;
                SQLiteDataReader sqlReader3;

                //lock (connection)
                //{
                    command5 = new SQLiteCommand("Select counts from points where id=" + id + " and tour=3", connection);
                    sqlReader3 = command5.ExecuteReader();
                //}

                if (!sqlReader3.HasRows)
                {
                    //lock (connection)
                    //{
                        SQLiteCommand command2 = new SQLiteCommand("INSERT INTO 'Points' ('ID','Tour', 'Counts') VALUES (" + id + ", 3, " + elementsPoints[i].InnerText + ");", connection);
                        command2.ExecuteNonQuery();
                    //}
                }
                else
                    Console.WriteLine("Line in table: Points is exists. Id="+id);

                ++i;
            }

            connection.Close();

            return schetchik;
        }
    }

    public class DownloadManager
    {
        private LinkTour linkTour;
        
        public DownloadManager(LinkTour _linkTour)
        {
            linkTour = new LinkTour(_linkTour.link, _linkTour.tour, _linkTour.countPage);
        }

        public void Start()
        {
            String link = linkTour.link;
            int tour = linkTour.tour;
            int page = linkTour.countPage;

            List<Thread> threads = new List<Thread>();
            //var doneEvents = new ManualResetEvent[5];

            for (int i = 1, counter = 0; i <= page; ++i, ++counter)
            {
                if (counter == 30)
                {
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
                    //WaitHandle.WaitAll(doneEvents);
                    counter = 0;
                }
                
                //doneEvents[counter] = new ManualResetEvent(false);
                link = link + i.ToString();
                HtmlFile htmlFile = new HtmlFile(link, tour, i.ToString());
                Thread thread = new Thread(htmlFile.DownloadFile);
                thread.Start();
                threads.Add(thread);
                link = link.Remove(75);
                
                if (i == page)
                {
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
                    //for (int j = counter + 1; j < 5; ++j)
                    //    doneEvents[j] = new ManualResetEvent(true);

                    //WaitHandle.WaitAll(doneEvents);
                }
            }

            int usersExists = 0;

            for (int i = 1; i <= page; ++i)
            {
                HtmlDocument doc = new HtmlDocument();
                link = link + i.ToString();
                String path = @"tmp\" + i.ToString();
                
                doc.Load(path, Encoding.UTF8);

                HtmlFile html = new HtmlFile(tour, i.ToString());

                usersExists += html.ParseAndInsert(doc);

                link = link.Remove(75);
            }

            Console.WriteLine(usersExists + "user exists");

            for (int i = 1; i <= page; ++i)
            {
                link = link + i.ToString();
                String path = @"tmp\" + i.ToString();

                File.Delete(path);

                link.Remove(75);
            }

            Directory.Delete(@"tmp");

            Console.WriteLine("Work done");
        }
    }

    class Program
    {
        
        static void Main(string[] args)
        {
            //HtmlFile file = new HtmlFile();

            String link = @"http://www.sports.ru/fantasy/football/tournament/ratings/leaders/50.html?p=";
            int tour = 3, page = 130;

            //file.DownloadManager(new LinkTour(link, tour, page));

            //---------------------------------

            DownloadManager dw = new DownloadManager(new LinkTour(link, tour, page));
            dw.Start();

            Console.ReadLine();
        }
    }
}
