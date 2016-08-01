using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(WebAppWithOAuth.Startup))]
namespace WebAppWithOAuth
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
        }
    }
}
