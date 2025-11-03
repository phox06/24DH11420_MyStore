using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using PagedList;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class HomeProductVM
    {
        public string SearchTerm { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 6;
        public List<Product> FeaturedProducts { get; set; }
        public IPagedList<Product> NewProducts { get; set; }
    }
}