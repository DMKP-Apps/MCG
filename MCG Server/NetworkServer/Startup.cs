using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using Owin;
using NetworkServer.Areas.Message;

[assembly: OwinStartup(typeof(NetworkServer.Startup))]

namespace NetworkServer
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }

       // public void ConfigureServices(IServiceCollection services)
        //{
            // Add framework services.
        //    services.AddMvc();

        //    services.AddSingleton<ITodoRepository, TodoRepository>();
        //}

    }
}
