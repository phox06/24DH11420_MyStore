using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using Microsoft.Ajax.Utilities;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using DbOrder = _24DH11420_LTTH_BE234.Models.Order;
using Item = PayPal.Api.Item;
using Payer = PayPal.Api.Payer;
namespace _24DH11420_LTTH_BE234.Controllers
{
    public class PaypalController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();

        // Hàm trợ giúp để lấy dịch vụ giỏ hàng
        private CartService GetCartService()
        {
            return new CartService(Session);
        }

        // Hàm trợ giúp để lấy thông tin xác thực API
        private APIContext GetApiContext()
        {
            var config = new Dictionary<string, string>
{
    { "mode", ConfigurationManager.AppSettings["PayPalMode"] },
    { "clientId", ConfigurationManager.AppSettings["PayPalClientId"] }, // Use correct casing
    { "clientSecret", ConfigurationManager.AppSettings["PayPalClientSecret"] }
};

            // Phần còn lại của mã giữ nguyên
            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken)
            {
                Config = config
            };
            return apiContext;
        }

        // GET: Paypal/PaymentWithPaypal
        // GET: Paypal/PaymentWithPaypal
        public ActionResult PaymentWithPaypal()
        {
            // Lấy ApiContext
            APIContext apiContext = GetApiContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {
                    // --- 1. GIAI ĐOẠN 1: Tạo thanh toán (Create Payment) ---

                    // Lấy lại model checkout và giỏ hàng từ Session
                    var cart = GetCartService().GetCart();
                    var checkoutModel = Session["CheckoutModel"] as CheckoutVM;

                    if (cart == null || checkoutModel == null || !cart.Items.Any())
                    {
                        // Nếu không có giỏ hàng, quay lại trang giỏ hàng
                        return RedirectToAction("Index", "Cart");
                    }

                    var guid = Convert.ToString((new Random()).Next(100000));
                    var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPaypal?";

                    // ==================================================================
                    // BẮT ĐẦU SỬA LỖI LÀM TRÒN
                    // ==================================================================

                    // 1. Tạo danh sách item VÀ tính tổng phụ (subtotal)
                    var itemList = new ItemList() { items = new List<Item>() };
                    decimal subtotal = 0;
                    // Bạn có thể đặt tỷ giá này trong web.config
                    decimal exchangeRate = 23000;

                    foreach (var item in cart.Items)
                    {
                        // Tính giá USD của 1 sản phẩm
                        decimal itemPriceUSD = Math.Round(item.UnitPrice / exchangeRate, 2);

                        var paypalItem = new Item()
                        {
                            name = item.ProductName,
                            currency = "USD",
                            price = itemPriceUSD.ToString(),
                            quantity = item.Quantity.ToString(),
                            sku = item.ProductID.ToString()
                        };
                        itemList.items.Add(paypalItem);

                        // 2. Cộng dồn vào tổng phụ (subtotal)
                        subtotal += (itemPriceUSD * item.Quantity);
                    }

                    // 3. Làm tròn tổng phụ lần cuối cùng
                    subtotal = Math.Round(subtotal, 2);

                    // ==================================================================
                    // KẾT THÚC SỬA LỖI LÀM TRÒN
                    // ==================================================================

                    var payment = new Payment
                    {
                        intent = "sale",
                        payer = new Payer { payment_method = "paypal" },
                        transactions = new List<Transaction>
                {
                    new Transaction
                    {
                        amount = new Amount
                        {
                            currency = "USD",
                            // 4. SỬ DỤNG subtotal đã tính làm total
                            total = subtotal.ToString(), 
                            // 5. SỬ DỤNG subtotal đã tính làm details.subtotal
                            details = new Details { tax = "0", shipping = "0", subtotal = subtotal.ToString() }
                        },
                        // 6. SỬ DỤNG itemList đã tạo
                        item_list = itemList,
                        description = "Thanh toán đơn hàng cho MaSV_MyStore.",
                        invoice_number = guid
                    }
                },
                        redirect_urls = new RedirectUrls
                        {
                            return_url = baseURI + "guid=" + guid,
                            cancel_url = baseURI + "cancel=true" // Dùng cho trường hợp người dùng bấm Hủy
                        }
                    };

                    var createdPayment = payment.Create(apiContext);

                    var approvalUrl = createdPayment.links.FirstOrDefault(lnk => lnk.rel == "approval_url");

                    // Lưu paymentId để thực thi sau
                    Session["paymentId"] = createdPayment.id;

                    // Chuyển hướng người dùng đến PayPal
                    return Redirect(approvalUrl.href);
                }
                else
                {
                    // --- 2. GIAI ĐOẠN 2: Thực thi thanh toán (Execute Payment) ---

                    var paymentId = Session["paymentId"] as string;

                    if (string.IsNullOrEmpty(paymentId))
                    {
                        return RedirectToAction("FailureView");
                    }

                    var paymentExecution = new PaymentExecution { payer_id = payerId };
                    var payment = new Payment { id = paymentId };

                    var executedPayment = payment.Execute(apiContext, paymentExecution);

                    if (executedPayment.state.ToLower() == "approved")
                    {
                        // THANH TOÁN THÀNH CÔNG
                        // (Mã này từ bước trước, giữ nguyên)
                        var cart = GetCartService().GetCart();
                        var checkoutModel = Session["CheckoutModel"] as CheckoutVM;
                        var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);

                        // --- SAO CHÉP LOGIC TẠO ĐƠN HÀNG TỪ ORDERCONTROLLER ---
                        // (Sử dụng DbOrder alias nếu bạn gặp lỗi xung đột)
                        var order = new Models.Order
                        {
                            CustomerID = customer.CustomerID,
                            OrderDate = DateTime.Now,
                            TotalAmount = cart.TotalValue(),
                            PaymentMethod = checkoutModel.PaymentMethod,
                            DeliveryMethod = checkoutModel.DeliveryMethod,
                            ShippingAddress = checkoutModel.ShippingAddress,
                            PaymentStatus = "Đã thanh toán Paypal"
                        };
                        db.Orders.Add(order);
                        db.SaveChanges();

                        foreach (var item in cart.Items)
                        {
                            var orderDetail = new OrderDetail
                            {
                                OrderID = order.OrderID,
                                ProductID = item.ProductID,
                                Quantity = item.Quantity,
                                UnitPrice = item.UnitPrice
                            };
                            db.OrderDetails.Add(orderDetail);
                        }
                        db.SaveChanges();
                        // ----------------------------------------------------

                        // Dọn dẹp Session
                        GetCartService().ClearCart();
                        Session["CheckoutModel"] = null;
                        Session["paymentId"] = null;

                        // Chuyển đến trang Thành công
                        return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
                    }
                    else
                    {
                        // Thanh toán thất bại (ví dụ: người dùng chọn hủy)
                        return RedirectToAction("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {
                // Ghi lại log lỗi (ex)
                // Đây là cách debug quan trọng:
                // Đặt breakpoint (F9) ở dòng return dưới đây.
                // Khi breakpoint được kích hoạt, hãy di chuột qua biến 'ex'
                // và xem 'ex.Message' hoặc 'ex.InnerException.Message'
                // để biết lỗi chính xác từ PayPal.

                // Hoặc bạn có thể ném lỗi ra để xem chi tiết
                // throw ex; 

                return RedirectToAction("FailureView");
            }
        }

        // GET: Paypal/FailureView
        public ActionResult FailureView()
        {
            return View();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    

// GET: Paypal
public ActionResult Index()
        {
            return View();
        }
    }
}
