using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.Data.Entity;
using PagedList;
using PagedList.Mvc;

namespace _24DH11420_LTTH_BE234.Controllers
{
    public class LayoutController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();
        public ActionResult CategoryMenu()
        {
            var categories = db.Categories.ToList();
            return PartialView("_CategoryMenuPV", categories);
        }
        public ActionResult PersonalMenu()
        {
            var model = new PersonalMenuVM();
            model.IsLoggedIn = User.Identity.IsAuthenticated;

            if (model.IsLoggedIn)
            {
                model.Username = User.Identity.Name;
            }
            var cart = Session["Cart"] as Cart; 
            if (cart != null)
            {
                model.CartCount = cart.Items.Count();
            }
            else
            {
                model.CartCount = 0;
            }

            return PartialView("_PersonalMenuPV", model);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    

// GET: Layout
public ActionResult Index()
        {
            return View();
        }
    }
}
