using PostOFSales.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PostOFSales.Areas.Admin.Controllers
{
    public class ShoppingCartController : Controller
    {
		Point_Of_SalesEntitiesDb DB = new Point_Of_SalesEntitiesDb();
        private string _Cart = "Cart";
		// GET: Admin/ShoppingCart
		public ActionResult Index()
        {
            return View();
        }

        public ActionResult OrderNow(int? id)
        {
            if (id == null) 
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var prodQuantity = DB.Products.Find(id);

			if (Convert.ToInt32(prodQuantity.ProductQuantity) < 1)
            {
                ViewBag.outOfStock = "This" + " " + prodQuantity.ProdutName + " "+ "product you are requesting for is out of stock";
				return View("index");
			}
            
            if (Session[_Cart] == null)
            {
                List<Cart> lstcarts = new List<Cart>
                {
                    new Cart(DB.Products.Find(id),1)
                };
                Session[_Cart] = lstcarts;
            }
            else
            {
                List<Cart> lstCart = (List<Cart>)Session[_Cart];
                //This step blow are added for update quantity of data

                int check =IsExistingCheck(id);
                if (check == -1)
                    lstCart.Add(new Cart(DB.Products.Find(id), 1));
                else
                    lstCart[check].Quatity++;
                Session[_Cart] = lstCart;
            }
            return View("index");
        }

        public ActionResult UpdateCart(FormCollection frc)
        {
            string[] quantities = frc.GetValues("quantity");
            


            
            List<Cart> lstCart = (List<Cart>)Session[_Cart];
            if (lstCart == null) return View("index");


            for (int i = 0; i < lstCart.Count; i++)
            {
                Product product = new Product();
                int check = Convert.ToInt32(product.ProductQuantity);
                
               
                lstCart[i].Quatity = Convert.ToInt32(quantities[i]);
             


            }
            Session[_Cart] = lstCart;

            foreach (Cart cart in lstCart)
            {
                var product = DB.Products.Find(cart.Product.Product_Id);
                var name = product.ProdutName;
                if (Convert.ToInt32(product.ProductQuantity) < cart.Quatity)
                {
                    ViewBag.insufficient = "The " + name + " " + "is greater than available product in the store";
                    return View("index");
                }

            }
                return View("index");
        }
        public ActionResult Delete(int?id)
        {
			if (id == null)
			{
				return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			}
			int check = IsExistingCheck(id);
			List<Cart> lstCart = (List<Cart>)Session[_Cart];
			lstCart.RemoveAt(check);
			return View("Index");
		}

        public ActionResult CheckOut()
        {
            return View();
        }


		public ActionResult ProcessOrder(FormCollection frc)
		{
			List<Cart> lstCart = (List<Cart>)Session[_Cart];
            if (lstCart != null)
            {
				SalesOrder salesOrder = new SalesOrder()
				{
					CustomerName = frc["cusName"],
					CustomerPhoneNo = frc["cusPhone"],
					CustomerEmail = frc["cusEmail"],
					customerAddress = frc["cusAddress"],
					SaleOrderDate = DateTime.Now,
					InvoiceDescription = "buying of Good",
					AmountPaid = lstCart.Sum(x => x.Quatity * x.Product.UnitPrice).ToString(),
					status = 0,
					PaymentMethod = "cash"
				};

				DB.SalesOrders.Add(salesOrder);
				DB.SaveChanges();

				foreach (Cart cart in lstCart)
                {
                  
                
                        try { 
							        OrderDetail orderDetail = new OrderDetail()
                                {
                                    SaleOrder_Id = salesOrder.SaleOrderId,
                                    ProductId = cart.Product.Product_Id,
                                    Quantity = cart.Quatity.ToString(),
                                    Price = cart.Product.UnitPrice.ToString(),
                                    Date = salesOrder.SaleOrderDate

                                };
                                DB.OrderDetails.Add(orderDetail);
						        DB.SaveChanges();

                        //var product = DB.Products.Find(cart.Product.Product_Id);
                        Product productFrmDB = DB.Products.Single(x => x.Product_Id == cart.Product.Product_Id);
                        var productLeft = Convert.ToInt32(productFrmDB.ProductQuantity) - Convert.ToInt32(cart.Quatity);

                        productFrmDB.ProductQuantity =productLeft.ToString();


						DB.Entry(productFrmDB).State = System.Data.Entity.EntityState.Modified;
						DB.SaveChanges();


                    } 

                        catch(System.Data.Entity.Validation.DbEntityValidationException dbEx)
                        {
                            Exception raise = dbEx;
                            foreach (var validationErrors in dbEx.EntityValidationErrors)
                            { 
                                foreach(var validationError in validationErrors.ValidationErrors)
                                {
                                    String message = string.Format("{0}:{1}",
                                                                   validationErrors.Entry.Entity.ToString(),
                                                                   validationError.ErrorMessage);
                                    raise = new InvalidOperationException(message,raise);

								}
                                throw raise;
                           
                            }
                        }
					}
                }
			Session.Remove(_Cart);
			return View("OrderSuccess");
		}
		private int IsExistingCheck(int? id) {
            List<Cart> lstCart = (List<Cart>)Session[_Cart];
            for (int i = 0; i < lstCart.Count; i++)
            {
                if (lstCart[i].Product.Product_Id == id) return i;
            }
            return -1;
        }

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				DB.Dispose();
			}
			base.Dispose(disposing);
		}
	}

}