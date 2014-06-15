using System;
using System.Collections.Generic;

namespace AspNetServiceStackRole.DataAccess {
    public class TvSeriesDoc {
        public TvSeriesDoc() {
            Seasons = new List<SeasonDoc>();
        }

        public Guid Id { get; set; }
        public string Name { get; set; }
        public int TvRageId { get; set; }

        public IList<SeasonDoc> Seasons { get; private set; }
    }

    public class SeasonDoc {
        public int SeasonNumber { get; set; }

    }

    public class EpisodeDoc {
        public int SeasonNumber { get; set; }
        public int EpisodeNumber { get; set; }
        public string Name { get; set; }
        public DateTime AirDate { get; set; }
        public string Id { get; set; }
    }
}