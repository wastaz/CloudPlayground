using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.WebSockets;
using AspNetServiceStackRole.DataAccess;
using AspNetServiceStackRole.Dto;
using AspNetServiceStackRole.TvRageApi;
using Raven.Client;
using ServiceStack;

namespace AspNetServiceStackRole.Services {
    public class TvSeriesService : Service {
        private readonly IDocumentStore docStore;
        private readonly TvRageApiHandler tvRageApiHandler;

        public TvSeriesService(IDocumentStore docStore, TvRageApiHandler tvRageApiHandler) {
            this.docStore = docStore;
            this.tvRageApiHandler = tvRageApiHandler;
        }

        public TvSeriesQueryResponse Get(TvSeriesQuery request) {
            using(var session = docStore.OpenSession()) {

                if(!string.IsNullOrEmpty(request.NameFragment)) {
                    var series =
                        session.Query<TvSeriesDoc, TvSeriesNameIndex>()
                            .Where(doc => doc.Name.StartsWith(request.NameFragment.ToLowerInvariant()))
                            .Skip(request.Offset)
                            .Take(request.Limit);
                    var epNames = new List<String>();
                    var response = new TvSeriesQueryResponse {TvSeries = new[] {series.First()}, EpisodeNames = epNames};
                    string id = session.Advanced.GetDocumentId(series.First()).ToLowerInvariant(); // "tvseriesdocs/" + series.First().Id;
                    response.NumberOfEpisodes =
                        session.Query<TvSeriesNumberOfEpisodesReduced, TvSeriesNumberOfEpisodesReduceIndex>()
                            .Where(tvi => tvi.ShowId == id)
                            .ToList()
                            .First()
                            .EpisodeCount;
                    foreach(var serie in series) {
                        var episodes = session.Advanced.LoadStartingWith<EpisodeDoc>(serie.Id.ToString());
                        epNames.AddRange(episodes.Select(ep => string.Format("{0}x{1} - {2}", ep.SeasonNumber, ep.EpisodeNumber, ep.Name)));
                    }



                    return response;
                } else {
                    var series = session.Query<TvSeriesDoc>().Skip(request.Offset).Take(request.Limit);
                    return new TvSeriesQueryResponse {TvSeries = series.ToList()};
                }
            }
        }

        public void Post(TvSeriesCommand request) {
            using(var session = docStore.OpenSession()) {
                int tvRageId = tvRageApiHandler.GetTvRageId(request.Name);
                var showInfo = tvRageApiHandler.GetTvRageShowInfo(tvRageId);
                var doc = new TvSeriesDoc {
                                              Name = showInfo.ShowName,
                                              TvRageId = tvRageId,
                                          };
                session.Store(doc);

                foreach(var season in showInfo.Seasons) {
                    foreach(var episode in season.Episodes) {
                        var epDoc = new EpisodeDoc {
                                                       SeasonNumber = season.SeasonNumber,
                                                       EpisodeNumber = episode.EpisodeNumber,
                                                       Name = episode.Title,
                                                       AirDate = episode.AirDate,
                                                       Id = doc.Id + "/episodes/" + season.SeasonNumber + "/" + episode.EpisodeNumber,
                                                   };
                        session.Store(epDoc);
                    }
                }

                session.SaveChanges();
            }
        }
        
    }
}