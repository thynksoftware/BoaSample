using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Boa.Sample.Data;
using Boa.Sample.Models;
using Microsoft.AspNetCore.Http;
using Boa.Sample.Controllers;
using Boa.Sample.Services;

namespace Boa.Sample
{
    public class Startup
    {
        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                 .SetBasePath(env.ContentRootPath)
                 .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                 .AddJsonFile("nogit-appsettings.json", optional: true, reloadOnChange: true);
            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<RequestLocalizationOptions>(
                options =>
                    {
                        var supportedCultures = new List<CultureInfo>
                        {
                            new CultureInfo("en-GB"),
                        };

                        options.DefaultRequestCulture = new RequestCulture(culture: "en-GB", uiCulture: "en-GB");
                        options.SupportedCultures = supportedCultures;
                        options.SupportedUICultures = supportedCultures;
                    });
            services.AddCors();
            services.AddMvc();
            services.AddOptions();
            services.AddDbContext<BoaIntegrationDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("BoaIntegrationDbContext")));
            services.AddIdentity<User, IdentityRole>(options =>
                {
                    options.Password.RequireDigit = false;
                    options.Password.RequireLowercase = false;
                    options.Password.RequireUppercase = false;
                    options.Password.RequireNonAlphanumeric = false;
                    options.Password.RequiredLength = 0;
                })
                .AddEntityFrameworkStores<BoaIntegrationDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailService, EmailService>();
            services.AddTransient<IEmailHelperService, EmailHelperService>();
            services.AddTransient<IViewRenderService, ViewRenderService>();
            services.AddSingleton<ICacheRepository, MemoryCacheRepository>();

            services.ConfigureApplicationCookie(options => options.Cookie.Name = "BoaIntegration");
            services.Configure<EmailSettings>(Configuration.GetSection(EmailSettings.ConfigurationKey));
            services.Configure<BoaOptions>(Configuration.GetSection(BoaOptions.ConfigurationKey));
            services.AddAuthentication()
                .AddFacebook(options =>
                {
                    options.AppId = Configuration["auth:facebook:appid"];
                    options.AppSecret = Configuration["auth:facebook:appsecret"];
                });
            services.AddAntiforgery(o =>
            {
                o.Cookie.SameSite = SameSiteMode.None;
                o.SuppressXFrameOptionsHeader = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseCors(x => { x.AllowAnyHeader().AllowAnyMethod().AllowAnyOrigin(); });
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes =>
            {
                routes.MapRoute(
                    name: "default",
                    template: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
