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
        // Phương thức này chạy TRƯỚC BẤT KỲ Action nào trong các controller kế thừa nó
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // Lấy tên của action (hàm) đang được gọi, ví dụ: "Index", "Login"
            string actionName = filterContext.ActionDescriptor.ActionName;

            // Nếu action là "Login" (dù là GET hay POST),
            // thì không làm gì cả và cho phép nó chạy.
            if (actionName == "Login")
            {
                base.OnActionExecuting(filterContext);
                return; // Dừng lại, không kiểm tra session
            }

            // --- Đây là logic kiểm tra cũ ---
            // Với TẤT CẢ CÁC action khác, hãy kiểm tra Session
            var session = Session["UserRole"];

            if (session == null || session.ToString() != "Admin")
            {
                // Nếu không phải Admin, NÉM về trang Login
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