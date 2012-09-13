using System;
using System.Collections.Generic;
using System.Threading;
using HtmlAgilityPack;
using System.IO;
using System.Text;

namespace FantasyStatistics
{
    public class DownloadManager
    {
        private LinkTour linkTour;

        public DownloadManager(LinkTour _linkTour)
        {
            linkTour = new LinkTour(_linkTour.link, _linkTour.tour, _linkTour.countPage, _linkTour.dbPath);
        }

        public void Start()
        {
            String link = linkTour.link, dbPath = linkTour.dbPath;
            int tour = linkTour.tour;
            int page = linkTour.countPage;

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
