using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Net.Http.Headers;

using Simple.Data;

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;

namespace Simple
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

			services.AddDbContext<ApplicationDbContext>(options =>
				options.UseSqlServer(
					Configuration.GetConnectionString("DefaultConnection")));
			services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
				.AddEntityFrameworkStores<ApplicationDbContext>();

			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
				app.UseDatabaseErrorPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			var provider = new FileExtensionContentTypeProvider();
			provider.Mappings[".htm3"] = "text/html";
			provider.Mappings[".image"] = "image/png";
			provider.Mappings[".image"] = "image/jpeg";
			provider.Mappings.Remove(".mp4");

			//app.UseStaticFiles();
			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
				HttpsCompression = HttpsCompressionMode.Compress,
				ContentTypeProvider = new FileExtensionContentTypeProvider
				{
					Mappings = new Dictionary<string, string>
					{
						{ "","" },
						{ "","" },
						{ "","" },
						{ "","" },
					}
				},
				DefaultContentType = "None",
				RequestPath = new PathString("StaticFiles"),
				ServeUnknownFileTypes = true,
				OnPrepareResponse = (response) =>
				{
					response.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age=31536000";
					response.Context.Response.Headers[HeaderNames.Pragma] = $"public,max-age=31536000";
					response.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddYears(1).ToString("R");

					if (response.Context.Request.Path.StartsWithSegments("/imgs"))
						if (response.Context.User.Identity.IsAuthenticated)
						{
							if (!response.Context.User.IsInRole("Admin"))
								throw new Exception("Not authenticated .. !!!!");

							response.Context.Response.Redirect("/Identity/Account/Login");
						}
				}
			});

			app.UseStaticFiles(new StaticFileOptions
			{
				FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
				OnPrepareResponse = x =>
				{
					if (x.Context.User.Identity.IsAuthenticated) return;
					x.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
				}
			});

			app.UseDefaultFiles();

			app.UseRouting();

			app.UseAuthentication();
			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
			});
		}
	}
}
