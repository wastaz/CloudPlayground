using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using AspNetServiceStackRole.DataAccess;
using ServiceStack;

namespace AspNetServiceStackRole.Dto {
    [Route("/tvseries", "GET")]
    public class TvSeriesQuery : IReturn<TvSeriesQueryResponse> {
        public string NameFragment { get; set; }

        public int Limit { get; set; }
        public int Offset { get; set; }
    }

    public class TvSeriesQueryResponse {
        public IList<TvSeriesDoc> TvSeries { get; set; }
        public int NumberOfEpisodes { get; set; }
        public IList<string> EpisodeNames { get; set; } 
    }
}