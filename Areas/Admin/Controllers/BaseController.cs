using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;

namespace _24DH11420_LTTH_BE234.Areas.Admin.Controllers
{
    public class BaseController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            
            string actionName = filterContext.ActionDescriptor.ActionName;

            
            if (actionName == "Login")
            {
                base.OnActionExecuting(filterContext);
                return; 
            }

           
            var session = Session["UserRole"];

            if (session == null || session.ToString() != "Admin")
            {
                
                filterContext.Result = new RedirectToRouteResult(
                    new RouteValueDictionary(new
                    {
                        controller = "Home",
                        action = "Login",
                        area = "Admin"
                    })
                );
            }

            base.OnActionExecuting(filterContext);
        }
    }
}