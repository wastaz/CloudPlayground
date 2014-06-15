using System;
using System.Collections.Generic;
using System.Xml.Linq;
using RestSharp;
using ServiceStack.Html;

namespace AspNetServiceStackRole.TvRageApi {
    public class TvRageApiHandler {
        private readonly RestClient client;
        
        public TvRageApiHandler() {
            client = new RestClient("http://services.tvrage.com");
        }

        public int GetTvRageId(string showName) {
            var request = new RestRequest("feeds/search.php");
            request.AddParameter("show", showName);

            var response = client.Execute(request);
            var doc = XDocument.Parse(response.Content);
            return int.Parse(doc.Element("Results").Element("show").Element("showid").Value);
        }

        public TvRageInfo GetTvRageShowInfo(int tvRageId) {
            var request = new RestRequest("feeds/full_show_info.php");
            request.AddParameter("sid", tvRageId);

            var response = client.Execute(request);
            var doc = XDocument.Parse(response.Content);

            var showElement = doc.Element("Show");
            var result = new TvRageInfo {
                                            ShowName = showElement.Element("name").Value,
                                            NumberOfSeasons = int.Parse(showElement.Element("totalseasons").Value),
                                            Started = DateTime.Parse(showElement.Element("started").Value),
                                            Status = showElement.Element("status").Value,
                                            Ended =
                                                showElement.Element("status").Value.ToLowerInvariant().Equals("ended")
                                                    ? DateTime.Parse(showElement.Element("ended").Value)
                                                    : (DateTime?)null,
                                        };

            foreach(var season in showElement.Element("Episodelist").Elements("Season")) {
                var currentSeason = new TvRageSeasonInfo {
                                                             SeasonNumber = int.Parse(season.Attribute("no").Value),
                                                         };
                result.Seasons.Add(currentSeason);

                foreach(var episode in season.Elements("episode")) {
                    currentSeason.Episodes.Add(new TvRageEpisodeInfo {
                                                                         AirDate = DateTime.Parse(episode.Element("airdate").Value),
                                                                         EpisodeNumber = int.Parse(episode.Element("epnum").Value),
                                                                         Title = episode.Element("title").Value,
                                                                     });
                }
            }

            return result;

        }
    }

    public class TvRageInfo {
        public TvRageInfo() {
            Seasons = new List<TvRageSeasonInfo>();
        }

        public string ShowName { get; set; }
        public int NumberOfSeasons { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Ended { get; set; }
        public string Status { get; set; }
        public List<TvRageSeasonInfo> Seasons { get; private set; } 
    }

    public class TvRageSeasonInfo {
        public TvRageSeasonInfo() {
            Episodes = new List<TvRageEpisodeInfo>();
        }

        public int SeasonNumber { get; set; }
        public List<TvRageEpisodeInfo> Episodes { get; private set; } 
    }

    public class TvRageEpisodeInfo {
        public int EpisodeNumber { get; set; }
        public DateTime AirDate { get; set; }
        public string Title { get; set; }
    }
}