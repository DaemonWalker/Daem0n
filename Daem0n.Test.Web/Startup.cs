using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Daem0n.DI;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
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
            services.PostConfigure<ConsoleLifetimeOptions>(_ => _.SuppressStatusMessages = false);
            services.Configure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(0.5));
            services.PostConfigure<HostOptions>(options => options.ShutdownTimeout = TimeSpan.FromSeconds(0.5));
            services.AddSingleton<IList<int>, List<int>>();
            services.AddScoped<IList<string>, List<string>>();
            // var factory = new DefaultServiceProviderFactory();
            //factory.CreateBuilder
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
