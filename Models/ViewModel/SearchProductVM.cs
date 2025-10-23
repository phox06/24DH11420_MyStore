using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;
using PagedList;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class SearchProductVM
    {
        public string SearchTerm { get; set; }
        public PagedList.IPagedList<Product> Products { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string SortOrder { get; set; }
        public int PageNumber { get; set; }
        public int PageSize { get; set; } = 10;
        



    }
}