using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class CategoryStatisticVM
    {
        public string CategoryName { get; set; }
        public int ProductCount { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? AvgPrice { get; set; }
    }
}