using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using System.ComponentModel.DataAnnotations;

namespace _24DH11420_LTTH_BE234.Models.ViewModel
{
    public class CheckoutVM
    {
        public List<CartItem> CartItems { get; set; }

        public int CustomerID { get; set; }
        public string Username { get; set; }

        [Display(Name = "Ngày đặt hàng")]
        public DateTime OrderDate { get; set; }

        [Display(Name = "Tổng giá trị")]
        public decimal TotalAmount { get; set; }

        [Display(Name = "Trạng thái thanh toán")]
        public string PaymentStatus { get; set; }

        [Required]
        [Display(Name = "Phương thức thanh toán")]
        public string PaymentMethod { get; set; }

        [Required]
        [Display(Name = "Phương thức giao hàng")]
        public string DeliveryMethod { get; set; }

        [Required]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        // Các thuộc tính tĩnh của đơn hàng
        public List<OrderDetail> OrderDetails { get; set; }
    }
}