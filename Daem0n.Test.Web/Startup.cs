using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daem0n.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Daem0n.Test.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            services.Configure<ConsoleLifetimeOptions>(_ => { });
            services.PostConfigure<ConsoleLifetimeOptions>(_ => { });
            services.Configure<HostOptions>(_ => { });
            services.PostConfigure<HostOptions>(_ => { });
            services.Configure<SocketTransportOptions>(_ => { });
            services.PostConfigure<SocketTransportOptions>(_ => { }); 
            services.Configure<KestrelServerOptions>(_ => { });
            services.PostConfigure<KestrelServerOptions>(_ => { });
            services.Configure<FormOptions>(_ => { });
            services.PostConfigure<FormOptions>(_ => { });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
