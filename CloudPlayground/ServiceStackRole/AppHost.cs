using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Funq;
using ServiceStack;

namespace ServiceStackRole {
    public class AppHost : AppHostBase {

        public AppHost() : base("ServiceStack Apphost", typeof(AppHost).Assembly) { }

        public override void Configure(Container container) {
            
        }
    }
}
