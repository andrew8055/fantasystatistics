using System;
using System.Net;
using HtmlAgilityPack;
using System.Data.SQLite;
using System.IO;

namespace FantasyStatistics
{
    public class HtmlFile
    {
        private SQLiteConnection connection;
        private String link, fileName;
        private int tour;

        public HtmlFile(String _link, int _tour, String _fileName)
        {
            link = _link;
            tour = _tour;
            fileName = _fileName;
        }

        public HtmlFile(int _tour, String _fileName, String _dbPath)
        {
            tour = _tour;
            fileName = _fileName;

            connection =
                new SQLiteConnection(string.Format("Data Source={0};", _dbPath));

            connection.Open();
        }

        public void DownloadFile()
        {
            WebRequest request = WebRequest.Create(link);

            WebResponse response = request.GetResponse();
            Stream stream = response.GetResponseStream();

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
        }

        public void ParseAndInsert(HtmlDocument doc)
        {
            HtmlNodeCollection elements, elementsPoints;

            elements = doc.DocumentNode.SelectNodes("//td[@class='name-td alLeft bordR']//a[@class='bold']");

            elementsPoints = doc.DocumentNode.SelectNodes("//td[@class='padR20']");

            int i = 1;

            foreach (var node in elements)
            {
                using (var sqlSel = new SQLiteCommand("Select id, name from users where link=@link", connection))
                {
                    sqlSel.Parameters.Add(new SQLiteParameter("@link", node.Attributes["href"].Value));
                    
                    SQLiteDataReader sqlReader = sqlSel.ExecuteReader();

                    while (!sqlReader.Read())
                    {
                        using (var sqlIns = new SQLiteCommand("INSERT INTO 'Users' ('ID','Name', 'Link') VALUES (NULL, @name, @link);", connection))
                        {
                            sqlIns.Parameters.Add(new SQLiteParameter("@name", node.InnerText));
                            sqlIns.Parameters.Add(new SQLiteParameter("@link", node.Attributes["href"].Value));

                            sqlIns.ExecuteNonQuery();
                        }

                        break;
                    }
                }

                using (var sqlSel = new SQLiteCommand("Select id from users where link=@link", connection))
                {
                    sqlSel.Parameters.Add(new SQLiteParameter("@link", node.Attributes["href"].Value));

                    SQLiteDataReader sqlReader = sqlSel.ExecuteReader();

                    int id = 0;

                    while (sqlReader.Read())
                    {
                        id = Convert.ToInt32(sqlReader[0].ToString());

                        break;
                    }

                    using (var sqlSel2 = new SQLiteCommand("SELECT id, tour, counts FROM points where id=@id and tour=@tour", connection))
                    {
                        sqlSel2.Parameters.Add(new SQLiteParameter("@id", id));
                        sqlSel2.Parameters.Add(new SQLiteParameter("@tour", tour));

                        SQLiteDataReader sqlReader2 = sqlSel2.ExecuteReader();

                        while (!sqlReader2.Read())
                        {

                            using (var sqlIns = new SQLiteCommand("INSERT INTO points(id, tour, counts) VALUES(@id, @tour, @counts)", connection))
                            {
                                sqlIns.Parameters.Add(new SQLiteParameter("@id", id));
                                sqlIns.Parameters.Add(new SQLiteParameter("@tour", tour));
                                sqlIns.Parameters.Add(new SQLiteParameter("@counts", Convert.ToInt32(elementsPoints[i].InnerText)));

                                sqlIns.ExecuteNonQuery();
                            }

                            break;
                        }
                    }
                }
                
                ++i;
            }

            connection.Close();
        }
    }
}
