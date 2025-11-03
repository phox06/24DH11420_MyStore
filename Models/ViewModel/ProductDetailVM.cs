using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using PagedList;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class ProductDetailVM
    {
        public Product Product { get; set; } 
        public int quantity { get; set; } = 1;
        public decimal estimatedValue { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 3;
        public IPagedList<Product> RelatedProducts { get; set; }
        public IPagedList<Product> TopProducts { get; set; }
    }
}