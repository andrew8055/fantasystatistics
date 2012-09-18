using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using System.IO;
using System.Text;
using IniParser;
using System.Data.SQLite;

namespace FantasyStatistics
{
    public class Manager
    {
        private Configuration config;

        private Manager(Configuration _config)
        {
            config = new Configuration(_config.link, _config.tour, _config.countPage, _config.dbPath);
        }

        public static Manager ReadIniFile(String _path)
        {
            String link = "", dbPath = "";
            int tour = 0, countPage  = 0;
            FileIniDataParser parser = new FileIniDataParser();

            IniData parserData = parser.LoadFile(_path);

            foreach (SectionData section in parserData.Sections)
            {
                foreach (KeyData kvp in section.Keys)
                {
                    switch(kvp.KeyName)
                    {
                        case "link":
                            link = kvp.Value;
                            break;
                        case "dbPath":
                            dbPath = kvp.Value;
                            break;
                        case "tour":
                            tour = Convert.ToInt32(kvp.Value);
                            break;
                        case "countPage":
                            countPage = Convert.ToInt32(kvp.Value);
                            break;
                    }
                }
            }

            return new Manager(new Configuration(link, tour, countPage, dbPath));
        }

        public void Leaders()
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", config.dbPath)))
            {
                connection.Open();

                using (SQLiteCommand sqlSel = new SQLiteCommand("SELECT us.name, p.counts, us.link FROM users us, points p WHERE us.id=p.id order by p.counts desc limit 50", connection))
                {
                    SQLiteDataReader reader = sqlSel.ExecuteReader();

                    Console.WriteLine("Leaders fantasy tournament");
                    Console.WriteLine("Name |   Points  |   Link\n");

                    while (reader.Read())
                    {
                        Console.WriteLine("{0}  |  {1}  |  {2}", reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                    }
                }

                connection.Close();
            }
        }

        public void SelectTopPlayersFor3Tours()
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", config.dbPath)))
            {
                connection.Open();

                using (SQLiteCommand sqlSel = new SQLiteCommand("SELECT max(tour) FROM points", connection))
                {
                    SQLiteDataReader reader = sqlSel.ExecuteReader();

                    if (!reader.HasRows)
                    {
                        Console.WriteLine("База данных не содержит ни одного тура");
                        return;
                    }

                    int maxTour = Convert.ToInt32(reader[0]);

                    for (int i = maxTour, counter = 0; i > 1 && counter < 3; --i, ++counter)
                    {
                        Console.WriteLine("Top fantasy-players for {0} tours\n", maxTour - i + 1);
                        SelectTopPlayers(maxTour, i - 1);
                        Console.Write("\n");
                    }
                }

                connection.Close();
            }
        }

        public void SelectTopPlayers(int _startIndex, int _endIndex)
        {
            using (SQLiteConnection connection = new SQLiteConnection(string.Format("Data Source={0};", config.dbPath)))
            {
                connection.Open();

                using (SQLiteCommand sqlSel = new SQLiteCommand("SELECT us.name, p2.counts-p1.counts, us.link FROM points p1, points p2, users us WHERE p1.id=p2.id and p1.tour=@endIndex and p2.tour=@startIndex and p1.id=us.id order by p2.counts-p1.counts desc limit 40", connection))
                {
                    sqlSel.Parameters.Add(new SQLiteParameter("@startIndex", _startIndex));
                    sqlSel.Parameters.Add(new SQLiteParameter("@endIndex", _endIndex));

                    SQLiteDataReader reader = sqlSel.ExecuteReader();

                    while (reader.Read())
                    {
                        Console.WriteLine("{0}  |  {1}  |  {2}", reader[0].ToString(), reader[1].ToString(), reader[2].ToString());
                    }
                }

                connection.Close();
            }
        }

        public void StartUpdateDB()
        {
            String link = config.link, dbPath = config.dbPath;
            int tour = config.tour;
            int page = config.countPage;

            List<Thread> threads = new List<Thread>();

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
                    counter = 0;
                }

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
                }
            }

            for (int i = 1; i <= page; ++i)
            {
                HtmlDocument doc = new HtmlDocument();
                link = link + i.ToString();
                String path = @"tmp\" + i.ToString();

                doc.Load(path, Encoding.UTF8);

                HtmlFile html = new HtmlFile(tour, i.ToString(), dbPath);

                html.ParseAndInsert(doc);

                link = link.Remove(75);
            }

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
}
