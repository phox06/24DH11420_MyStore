using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class SearchProductVM
    {
        public string SearchTerm { get; set; }
        public List<Product> Products { get; set; }
        
    }
}