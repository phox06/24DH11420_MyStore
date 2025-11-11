using _24DH11420_LTTH_BE234.Models;
using _24DH11420_LTTH_BE234.Models.ViewModel;
using PayPal.Api;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.Mvc;
using Item = PayPal.Api.Item;
using Payer = PayPal.Api.Payer;
namespace _24DH11420_LTTH_BE234.Controllers
{
    public class PaypalController : Controller
    {
        private MyStoreEntities db = new MyStoreEntities();


        private CartService GetCartService()
        {
            return new CartService(Session);
        }


        private APIContext GetApiContext()
        {
            var config = new Dictionary<string, string>
{
            { "mode", ConfigurationManager.AppSettings["PayPalMode"] },
            { "clientId", ConfigurationManager.AppSettings["PayPalClientId"] },
            { "clientSecret", ConfigurationManager.AppSettings["PayPalClientSecret"] }
};


            var accessToken = new OAuthTokenCredential(config).GetAccessToken();
            var apiContext = new APIContext(accessToken)
            {
                Config = config
            };
            return apiContext;
        }


        // GET: Paypal/PaymentWithPaypal
        public ActionResult PaymentWithPaypal()
        {

            APIContext apiContext = GetApiContext();

            try
            {
                string payerId = Request.Params["PayerID"];

                if (string.IsNullOrEmpty(payerId))
                {

                    var cart = GetCartService().GetCart();
                    var checkoutModel = Session["CheckoutModel"] as CheckoutVM;

                    if (cart == null || checkoutModel == null || !cart.Items.Any())
                    {

                        return RedirectToAction("Index", "Cart");
                    }

                    var guid = Convert.ToString((new Random()).Next(100000));
                    var baseURI = Request.Url.Scheme + "://" + Request.Url.Authority + "/Paypal/PaymentWithPaypal?";




                    var itemList = new ItemList() { items = new List<Item>() };
                    decimal subtotal = 0;
                    decimal exchangeRate = 23000;

                    foreach (var item in cart.Items)
                    {
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


                        subtotal += (itemPriceUSD * item.Quantity);
                    }


                    subtotal = Math.Round(subtotal, 2);



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

                            total = subtotal.ToString(),

                            details = new Details { tax = "0", shipping = "0", subtotal = subtotal.ToString() }
                        },

                        item_list = itemList,
                        description = "Thanh toán đơn hàng cho MaSV_MyStore.",
                        invoice_number = guid
                    }
                },
                        redirect_urls = new RedirectUrls
                        {
                            return_url = baseURI + "guid=" + guid,
                            cancel_url = baseURI + "cancel=true"
                        }
                    };

                    var createdPayment = payment.Create(apiContext);

                    var approvalUrl = createdPayment.links.FirstOrDefault(lnk => lnk.rel == "approval_url");


                    Session["paymentId"] = createdPayment.id;


                    return Redirect(approvalUrl.href);
                }
                else
                {

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

                        var cart = GetCartService().GetCart();
                        var checkoutModel = Session["CheckoutModel"] as CheckoutVM;
                        var customer = db.Customers.SingleOrDefault(c => c.Username == User.Identity.Name);


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


                        GetCartService().ClearCart();
                        Session["CheckoutModel"] = null;
                        Session["paymentId"] = null;


                        return RedirectToAction("OrderSuccess", "Order", new { id = order.OrderID });
                    }
                    else
                    {

                        return RedirectToAction("FailureView");
                    }
                }
            }
            catch (Exception ex)
            {

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
