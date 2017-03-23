using System.Web;
using RZNU_SimpleRest.App_Start;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using AspNetHaack;

[assembly: PreApplicationStartMethod(typeof(FormsAuthenticationConfig), "Register")]
namespace RZNU_SimpleRest.App_Start {
    public static class FormsAuthenticationConfig {
        public static void Register() {
            DynamicModuleUtility.RegisterModule(typeof(SuppressFormsAuthenticationRedirectModule));
        }
    }
}
