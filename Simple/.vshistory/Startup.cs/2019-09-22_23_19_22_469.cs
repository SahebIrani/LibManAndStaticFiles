
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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
using Microsoft.Web.Administration;

using Simple.Data;

namespace Simple
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Configuration = configuration;
            Env = env;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

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

            services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie();

            services.AddAuthorization(options =>
            {
                var basePath = Path.Combine(Env.ContentRootPath, "PrivateFiles");
                var usersPath = Path.Combine(basePath, "Users");

                // When using this policy users are only authorized to access the base directory, the Users directory,
                // and their own directory under Users.
                options.AddPolicy("files", builder =>
                {
                    builder.RequireAuthenticatedUser().RequireAssertion(context =>
                    {
                        var userName = context.User.Identity.Name;
                        userName = userName?.Split('@').FirstOrDefault();
                        if (userName == null)
                        {
                            return false;
                        }
                        var userPath = Path.Combine(usersPath, userName);
                        if (context.Resource is IFileInfo file)
                        {
                            var path = Path.GetDirectoryName(file.PhysicalPath);
                            return string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(path, usersPath, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(path, userPath, StringComparison.OrdinalIgnoreCase)
                                || path.StartsWith(userPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                        }
                        else if (context.Resource is IDirectoryContents dir)
                        {
                            // https://github.com/aspnet/Home/issues/3073
                            // This won't work right if the directory is empty
                            var path = Path.GetDirectoryName(dir.First().PhysicalPath);
                            return string.Equals(path, basePath, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(path, usersPath, StringComparison.OrdinalIgnoreCase)
                                || string.Equals(path, userPath, StringComparison.OrdinalIgnoreCase)
                                || path.StartsWith(userPath + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);
                        }

                        throw new NotImplementedException($"Unknown resource type '{context.Resource.GetType()}'");
                    });
                });
            });


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IAuthorizationService authorizationService)
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
            //URI                                               Response
            //http://<server_address>/StaticFiles/imgs/1.png	MyStaticFiles/images/imgs/1.png
            //http://<server_address>/StaticFiles	            MyStaticFiles/default.html
            //app.UseStaticFiles(); // For the wwwroot folder
            //app.UseFileServer(new FileServerOptions
            //{
            //    FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "StaticFiles")),
            //    RequestPath = "/MyStaticFiles",
            //UseDirectoryBrowser and UseStaticFiles can leak secrets. Disabling directory browsing in production is highly recommended.
            //Carefully review which directories are enabled via UseStaticFiles or UseDirectoryBrowser.
            //The entire directory and its sub-directories become publicly accessible.
            //Store files suitable for serving to the public in a dedicated directory,
            //such as <content_root>/wwwroot.Separate these files from MVC views, Razor Pages(2.x only), configuration files, etc.
            //    EnableDirectoryBrowsing = false,
            //});

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
                //Warning == Enabling ServeUnknownFileTypes is a security risk.It's disabled by default, and its use is discouraged.
                //FileExtensionContentTypeProvider provides a safer alternative to serving files with non-standard extensions.
                ServeUnknownFileTypes = true,
                DefaultContentType = "image/png",
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
                        if (name.Contains("1") || length > 273519)
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

                    //var cachePeriod = env.IsDevelopment() ? "600" : "604800";
                    //// Requires the following import:
                    //// using Microsoft.AspNetCore.Http;
                    //sfrc.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                    //sfrc.Context.Response.Headers["Cache-Control"] = "private, max-age=43200";
                    //sfrc.Context.Response.Headers["Expires"] = DateTime.UtcNow.AddHours(12).ToString("R");
                    //const int durationInSeconds = 60 * 60 * 24;
                    //sfrc.Context.Response.Headers[HeaderNames.CacheControl] = "public,max-age=" + durationInSeconds;
                    //sfrc.Context.Response.Headers.Append("Access-Control-Allow-Origin", "*");
                    //sfrc.Context.Response.Headers.Append("Access-Control-Allow-Headers", "Origin, X-Requested-With, Content-Type, Accept");


                    if (!sfrc.Context.Request.Cookies.ContainsKey(".AspNetCore.Identity.Application"))
                        sfrc.Context.Response.StatusCode = StatusCodes.Status401Unauthorized;


                    //var headers = sfrc.Context.Response.Headers;
                    //var contentType = headers["Content-Type"];
                    //if (contentType != "application/x-gzip" && !sfrc.File.Name.EndsWith(".gz"))
                    //{
                    //    return;
                    //}
                    //var fileNameToTry = sfrc.File.Name.Substring(0, sfrc.File.Name.Length - 3);
                    //var mimeTypeProvider = new FileExtensionContentTypeProvider();
                    //if (mimeTypeProvider.TryGetContentType(fileNameToTry, out var mimeType))
                    //{
                    //    headers.Add("Content-Encoding", "gzip");
                    //    headers["Content-Type"] = mimeType;
                    //}
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

            //Complete the following steps in IIS Manager to remove the IIS static file handler at the server or website level:
            //Navigate to the Modules feature.
            //Select StaticFileModule in the list.
            //Click Remove in the Actions sidebar.

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


            app.Run(async context =>
            {
                if (context.User?.Identities == null)
                    await context.Response.WriteAsync("No user identities");

                foreach (var id in context.User.Identities)
                {
                    var sb = new StringBuilder();

                    sb.AppendLine("Identity");
                    sb.AppendLine($"  Name: {id.Name}");
                    sb.AppendLine($"  Label: {id.Label}");
                    sb.AppendLine($"  AuthType: {id.AuthenticationType}");
                    sb.AppendLine($"  Authenticated?: {id.IsAuthenticated}");
                    var claims = string.Join(", ", id.Claims.Select(c => c.Value));
                    sb.AppendLine($"  Claims: {claims}");

                    await context.Response.WriteAsync(sb.ToString());
                }
            });


            var files = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "PrivateFiles"));

            app.Map("/MapAuthenticatedFiles", branch =>
            {
                MapAuthenticatedFiles(branch, files);
            });

            app.Map("/MapImperativeFiles", branch =>
            {
                MapImperativeFiles(authorizationService, branch, files);
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();
                endpoints.MapRazorPages();
            });
        }

        // Blanket authorization, any authenticated user is allowed access to these resources.
        private static void MapAuthenticatedFiles(IApplicationBuilder branch, PhysicalFileProvider files)
        {
            branch.Use(async (context, next) =>
            {
                if (!context.User.Identity.IsAuthenticated)
                {
                    await context.ChallengeAsync(new AuthenticationProperties()
                    {
                        // https://github.com/aspnet/Security/issues/1730
                        // Return here after authenticating
                        RedirectUri = context.Request.PathBase + context.Request.Path + context.Request.QueryString
                    });
                    return;
                }

                await next();
            });
            branch.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = true,
                FileProvider = files
            });
        }


        // Policy based authorization, requests must meet the policy criteria to be get access to the resources.
        private static void MapImperativeFiles(IAuthorizationService authorizationService, IApplicationBuilder branch, PhysicalFileProvider files)
        {
            branch.Use(async (context, next) =>
            {
                var fileInfo = files.GetFileInfo(context.Request.Path);
                AuthorizationResult result = null;
                if (fileInfo.Exists)
                {
                    result = await authorizationService.AuthorizeAsync(context.User, fileInfo, "files");
                }
                else
                {
                    // https://github.com/aspnet/Home/issues/2537
                    var dir = files.GetDirectoryContents(context.Request.Path);
                    if (dir.Exists)
                    {
                        result = await authorizationService.AuthorizeAsync(context.User, dir, "files");
                    }
                    else
                    {
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        return;
                    }
                }

                if (!result.Succeeded)
                {
                    if (!context.User.Identity.IsAuthenticated)
                    {
                        await context.ChallengeAsync(new AuthenticationProperties()
                        {
                            // https://github.com/aspnet/Security/issues/1730
                            // Return here after authenticating
                            RedirectUri = context.Request.PathBase + context.Request.Path + context.Request.QueryString
                        });
                        return;
                    }
                    // Authenticated but not authorized
                    await context.ForbidAsync();
                    return;
                }

                await next();
            });
            branch.UseFileServer(new FileServerOptions()
            {
                EnableDirectoryBrowsing = true,
                FileProvider = files
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

        //internal static bool IsAuthenticated(this HttpContext context, SharedOptionsBase options)
        //{
        //    if (options.AllowAnonymous)
        //        return true;

        //    var authSchemes = options.AuthenticationSchemes ?? context.Authentication.GetAuthenticationSchemes().Select(desc => desc.AuthenticationScheme).ToArray();

        //    foreach (var authScheme in authSchemes)
        //    {
        //        var cp = context.Authentication.AuthenticateAsync(authScheme).Result;
        //        if (cp == null) continue;
        //        context.User = cp;
        //        break;
        //    }
        //    return (context.User != null && context.User.Identity.IsAuthenticated);
        //}

        //internal static bool IsAuthorized(this HttpContext context, ILibrary library, SharedOptionsBase options)
        //{
        //    return options.AllowAnonymous ||
        //           !options.AuthorizationRequirements.Any() ||
        //           context.RequestServices.GetService<IAuthorizationService>().AuthorizeAsync(context.User, library, options.AuthorizationRequirements).Result;
        //}
    }
}
