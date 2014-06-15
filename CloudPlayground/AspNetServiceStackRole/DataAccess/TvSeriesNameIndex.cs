using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace AspNetServiceStackRole.DataAccess {
    public class TvSeriesNameIndex : AbstractIndexCreationTask<TvSeriesDoc> {
        public TvSeriesNameIndex() {
            Map = series => from serie in series
                            select new {serie.Name};
 
            Indexes = new Dictionary<Expression<Func<TvSeriesDoc, object>>, FieldIndexing> {
                { serie => serie.Name, FieldIndexing.Analyzed },
            };
        }
    }

}