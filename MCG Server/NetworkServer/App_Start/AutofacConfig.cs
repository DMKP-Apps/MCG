using Autofac;
using Autofac.Integration.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using NetworkServer.Areas.Message.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using System.Web.Mvc;

namespace NetworkServer
{
    public class AutofacConfig
    {
        public static void SetAutofacContainer()
        {
            // http://docs.autofac.org/en/latest/integration/mvc.html
            // http://decompile.it/blog/2014/03/13/webapi-autofac-lifetime-scopes/#more-620

            var builder = new ContainerBuilder();

            builder.RegisterType<NetworkDataRepository>().As<INetworkDataRepository>().SingleInstance();

            // Factories
            //builder.RegisterType<ConfigurationFactory>().As<IConfigurationFactory>().InstancePerRequest();
            //builder.RegisterType<AuthenticationFactory>().As<IAuthenticationFactory>().InstancePerRequest();
            //builder.RegisterType<NLogLogger>().As<ILogger>().InstancePerRequest();

            //// Servivces
            //builder.RegisterAssemblyTypes(typeof(EmploymentService).Assembly)
            //    .Where(t => t.Name.EndsWith("Service"))
            //    .AsImplementedInterfaces().InstancePerRequest();

            //builder.RegisterType<Utility.Security.SecurityWrapper>().As<Utility.Security.ISecurityWrapper>().InstancePerRequest();

            //// Controllers
            //builder.RegisterControllers(Assembly.GetExecutingAssembly());

            //// DB Services
            //builder.RegisterType<BoostContextProvider>().As<Database.Framework.Context.IContextProvider<Database.BariumDBContextWithSyncTables>>().InstancePerRequest();
            //builder.RegisterType<Database.BariumDBContextUnitOfWork>().InstancePerRequest();
            //builder.RegisterType<BoostIdentityProvider>().As<Database.Framework.IdentityProviders.IIdentityProviders>().InstancePerRequest();

            //builder.RegisterAssemblyTypes(typeof(Barium.Services.TM.ApplicationService).Assembly)
            //    .Where(t => t.Name.EndsWith("Service"))
            //    .AsImplementedInterfaces().InstancePerRequest();

            // Filters
            builder.RegisterFilterProvider();

            IContainer container = builder.Build();
            DependencyResolver.SetResolver(new AutofacDependencyResolver(container));
        }
    }
}