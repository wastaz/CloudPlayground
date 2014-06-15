using System.Linq;
using Raven.Client.Indexes;

namespace AspNetServiceStackRole.DataAccess {
    public class TvSeriesNumberOfEpisodesReduceIndex : AbstractMultiMapIndexCreationTask<TvSeriesNumberOfEpisodesReduced> {
        public TvSeriesNumberOfEpisodesReduceIndex() {
            AddMap<TvSeriesDoc>(series => from serie in series
                                          select new {ShowId = serie.Id, EpisodeCount = 0});
            AddMap<EpisodeDoc>(episodes => from episode in episodes
                                           select new {ShowId = "TvSeriesDocs/" + episode.Id.Split('/')[0], EpisodeCount = 1});
            

            Reduce = results => from result in results
                                group result by new {result.ShowId}
                                into g
                                select new {
                                               ShowId = g.Key.ShowId.ToLowerInvariant(),
                                               EpisodeCount = g.Sum(x => x.EpisodeCount),
                                           };
        }
    }

    public class TvSeriesNumberOfEpisodesReduced {
        public string ShowId { get; set; }
        public int EpisodeCount { get; set; }
    }
}