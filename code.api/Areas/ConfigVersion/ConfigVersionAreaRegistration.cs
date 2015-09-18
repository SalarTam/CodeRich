using System.Web.Mvc;

namespace code.api.Areas.ConfigVersion
{
    public class ConfigVersionAreaRegistration : AreaRegistration
    {
        public override string AreaName
        {
            get
            {
                return "ConfigVersion";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context)
        {
            context.MapRoute(
                "ConfigVersion_default",
                "ConfigVersion/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}
