using System;

namespace FantasyStatistics
{
    public class Configuration
    {
        public String link, dbPath;
        public int tour, countPage;

        public Configuration(String _link, int _tour, int _countPage, String _dbPath)
        {
            link = _link;
            tour = _tour;
            countPage = _countPage;
            dbPath = _dbPath;
        }
    }
}
