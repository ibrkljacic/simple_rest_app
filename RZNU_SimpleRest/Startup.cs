using Microsoft.Owin;
using Owin;

[assembly: OwinStartupAttribute(typeof(RZNU_SimpleRest.Startup))]
namespace RZNU_SimpleRest
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            //ConfigureAuth(app);
        }
    }
}
