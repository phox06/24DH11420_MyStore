using System.Web;
using System.Web.Mvc;

namespace _24DH11420_LTTH_BE234
{
    public class FilterConfig
    {
        public static void RegisterGlobalFilters(GlobalFilterCollection filters)
        {
            filters.Add(new HandleErrorAttribute());
        }
    }
}
