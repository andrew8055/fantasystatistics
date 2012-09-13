using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FantasyStatistics
{
    public class LinkTour
    {
        public String link, dbPath;
        public int tour, countPage;

        public LinkTour(String _link, int _tour, int _countPage, String _dbPath)
        {
            link = _link;
            tour = _tour;
            countPage = _countPage;
            dbPath = _dbPath;
        }
    }
}
