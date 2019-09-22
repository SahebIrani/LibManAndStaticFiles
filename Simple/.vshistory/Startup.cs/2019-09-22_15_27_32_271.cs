
using System;
using System.IO;
using System.Net;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Web.Administration;

using Simple.Data;

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

            services.AddControllers();
            services.AddRazorPages();

            services.AddDirectoryBrowser();
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

            //FileExtensionContentTypeProvider provider = new FileExtensionContentTypeProvider();
            //provider.Mappings[".image"] = "image/png";
            //provider.Mappings.Remove(".mp4");

            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
            //    //RequestPath = new PathString("/imgs"),
            //    //ContentTypeProvider = provider,
            //    ContentTypeProvider = new MyContentTypeProvider(),
            //    DefaultContentType = "image/png",
            //    HttpsCompression = HttpsCompressionMode.Compress,
            //    ServeUnknownFileTypes = true,
            //    OnPrepareResponse = (response) =>
            //    {
            //        response.Context.Response.Headers[HeaderNames.CacheControl] = $"public,max-age=31536000";
            //        response.Context.Response.Headers[HeaderNames.Pragma] = $"public,max-age=31536000";
            //        response.Context.Response.Headers[HeaderNames.Expires] = DateTime.UtcNow.AddYears(1).ToString("R");

            //        if (response.Context.Request.Path.StartsWithSegments("/imgs"))
            //        {
            //            if (response.Context.User.Identity.IsAuthenticated)
            //            {
            //                if (response.Context.User.IsInRole("Admin"))
            //                {
            //                    return;
            //                }
            //                else
            //                {
            //                    response.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            //                    throw new UnauthorizedAccessException();
            //                }
            //            }
            //            else
            //            {
            //                response.Context.Response.Redirect("/Identity/Account/Login");
            //            }
            //        }
            //    }
            //});

            //UseDefaultFiles must be called before UseStaticFiles to serve the default file.
            //default.htm default.html index.htm index.html
            //app.UseDefaultFiles();
            // Serve my app-specific default file, if present.
            //DefaultFilesOptions options = new DefaultFilesOptions();
            //options.DefaultFileNames.Clear();
            //options.DefaultFileNames.Add("mydefault.html");
            //app.UseDefaultFiles(options);

            //app.UseStaticFiles(); // For the wwwroot folder
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
            //    RequestPath = "/StaticFiles"
            //});

            //app.UseStaticFiles(); // For the wwwroot folder
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "imgs")),
            //    RequestPath = "/MyImages"
            //});
            //app.UseDirectoryBrowser(new DirectoryBrowserOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles", "imgs")),
            //    RequestPath = "/MyImages"
            //});

            //UseFileServer combines the functionality of UseStaticFiles, UseDefaultFiles, and UseDirectoryBrowser.
            //app.UseFileServer();
            //The following code builds upon the parameterless overload by enabling directory browsing:
            //app.UseFileServer(enableDirectoryBrowsing: true);

            //Using the file hierarchy and preceding code, URLs resolve as follows:
            //URI                                                       Response
            //http://<server_address>/StaticFiles/images/banner1.svg	MyStaticFiles/images/banner1.svg
            //http://<server_address>/StaticFiles	                    MyStaticFiles/default.html
            app.UseStaticFiles(); // For the wwwroot folder
            app.UseFileServer(new FileServerOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
                RequestPath = "/MyStaticFiles",
                EnableDirectoryBrowsing = false,
            });

            //https://www.iana.org/assignments/media-types/media-types.xhtml    See MIME content types.
            // Set up custom content types - associating file extension to MIME type
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".htm3"] = "text/html";
            provider.Mappings[".image"] = "image/png";
            // Replace an existing mapping
            provider.Mappings[".rtf"] = "application/x-msdownload";
            // Remove MP4 videos.
            provider.Mappings.Remove(".mp4");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs")),
                RequestPath = "/MyImages",
                ContentTypeProvider = provider,
                DefaultContentType = "image/png",
                ServeUnknownFileTypes = true,
                HttpsCompression = HttpsCompressionMode.Compress,
                RedirectToAppendTrailingSlash = true,
                OnPrepareResponse = (sfrc) =>
                {
                    if (sfrc.File.Exists && !sfrc.File.IsDirectory)
                    {
                        var name = sfrc.File.Name;
                        var lastModify = sfrc.File.LastModified;
                        var length = sfrc.File.Length;
                        var pp = sfrc.File.PhysicalPath;
                        if (name.Contains("1") && length > 273519)
                            sfrc.Context.Response.Redirect("/Identity/Account/Login");
                    }

                    var startsWithSegmentsImages = sfrc.Context.Request.Path.StartsWithSegments("/MyImages");
                    var isAuthenticated = sfrc.Context.User.Identity.IsAuthenticated;
                    var isInRoleAdmin = sfrc.Context.User.IsInRole("Admin");
                    if (startsWithSegmentsImages)
                    {
                        if (isAuthenticated)
                        {
                            if (isInRoleAdmin)
                            {
                                return;
                            }
                            else
                            {
                                sfrc.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                                throw new UnauthorizedAccessException();
                            }
                        }
                        else
                        {
                            sfrc.Context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                            sfrc.Context.Response.Redirect("/Identity/Account/Login");
                        }
                    }
                }
            });
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "imgs")),
                RequestPath = "/MyImages",
                RedirectToAppendTrailingSlash = true,
            });

            //Static File Middleware understands almost 400 known file content types
            //If no middleware handles the request, a 404 Not Found response is returned.
            //The following code enables serving unknown types and renders the unknown file as an image:
            //app.UseStaticFiles(new StaticFileOptions
            //{
            //    ServeUnknownFileTypes = true,
            //    DefaultContentType = "image/png"
            //});

            using (ServerManager serverManager = new ServerManager())
            {
                Configuration config = serverManager.GetWebConfiguration("Contoso");

                // var directoryBrowseSection = config.GetSection("system.webServer/directoryBrowse");

                //enabled Optional Boolean attribute.
                //Specifies whether directory browsing is enabled (true) or disabled (false) on the Web server.
                //The default value is false.

                //directoryBrowseSection["enabled"] = true;

                //showFlags Optional flags attribute.
                //The showFlags attribute can have one or more of the following possible values.
                //If you specify more than one value, separate the values with a comma (,).
                //The default values are Date, Time, Size, Extension.
                //Value       Description
                //Date        Includes the last modified date for a file or directory in a directory listing.
                //Extension   Includes a file name extension for a file in a directory listing.
                //LongDate    Includes the last modified date in extended format for a file in a directory listing.
                //None        Specifies that only the file or directory names are returned in a directory listing.
                //Size        Includes the file size for a file in a directory listing.
                //Time        Includes the last modified time for a file or directory in a directory listing.

                //directoryBrowseSection["showFlags"] = @"Date, Time, Size, Extension, LongDate";

                //serverManager.CommitChanges();
            }

            //◘◘◘◘◘◘◘◘

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }

        public class MyContentTypeProvider : FileExtensionContentTypeProvider
        {
            public MyContentTypeProvider()
            {
                Mappings.Add(".image", "image/pngn");
                Mappings.Remove(".json");
            }
        }
    }
}
