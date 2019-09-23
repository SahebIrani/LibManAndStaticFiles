namespace Simple.Others
{
    using Cofoundry.Web;

    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.StaticFiles;

    public class SimpleStaticFileOptionsConfiguration : IStaticFileOptionsConfiguration
    {
        public void Configure(StaticFileOptions options)
        {
            options.OnPrepareResponse = OnPrepareResponse;
        }

        private void OnPrepareResponse(StaticFileResponseContext context)
        {
            context.Context.Response.Headers.Add("Cache-Control", new[] { "public,max-age=31536000" });
        }
    }
}
