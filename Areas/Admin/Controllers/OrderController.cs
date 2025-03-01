using PostOFSales.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;

namespace PostOFSales.Areas.Admin.Controllers
{
    public class OrderController : Controller
    {
		Point_Of_SalesEntitiesDb DB = new Point_Of_SalesEntitiesDb();
		// GET: Admin/Order
		public ActionResult Index()
        {
            var saleOrder = DB.SalesOrders.ToList();
            return View(saleOrder);
        }
        public ActionResult Detail(int? id) 
        {
            if(id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            var salesOrder = DB.SalesOrders.Find(id);

            if(salesOrder == null) return HttpNotFound();

            return View(salesOrder);
            
        }
    }
}