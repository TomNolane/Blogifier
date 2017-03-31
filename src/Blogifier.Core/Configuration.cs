﻿using System.Reflection;
using Blogifier.Core.Common;
using Blogifier.Core.Data;
using Blogifier.Core.Data.Interfaces;
using Blogifier.Core.Data.Repositories;
using Blogifier.Core.Middleware;
using Blogifier.Core.Services.FileSystem;
using Blogifier.Core.Services.Syndication.Rss;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;

namespace Blogifier.Core
{
	public class Configuration
    {
		public static void InitServices(IServiceCollection services)
		{
			services.AddTransient<IRssService, RssService>();
			services.AddTransient<IBlogStorage, BlogStorage>();

			AddDatabase(services);

			AddFileProviders(services);
		}

		public static void InitApplication(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			app.UseMiddleware<EmbeddedResources>();

			ApplicationSettings.WebRootPath = env.WebRootPath;
			ApplicationSettings.ContentRootPath = env.ContentRootPath;

            var loader = new AppSettingsLoader();
            loader.LoadFromConfigFile();
		}

		static void AddDatabase(IServiceCollection services)
		{
			services.AddSingleton<IUnitOfWork, UnitOfWork>();

            services.AddDbContext<BlogifierDbContext>(options =>
                options.UseInMemoryDatabase());

            //services.AddDbContext<BlogifierDbContext>(options =>
            //        options.UseSqlServer(ApplicationSettings.ConnectionString));
        }

		static void AddFileProviders(IServiceCollection services)
		{
			var assemblyName = "Blogifier.Core";

			try
			{
				var assembly = Assembly.Load(new AssemblyName(assemblyName));

				services.Configure<RazorViewEngineOptions>(options =>
				{
                    // in some environments provider order matters
                    if(ApplicationSettings.PrependFileProvider)
					    options.FileProviders.Insert(0, new EmbeddedFileProvider(assembly, assemblyName));
                    else
					    options.FileProviders.Add(new EmbeddedFileProvider(assembly, assemblyName));
				});
			}
			catch { }
		}
	}
}