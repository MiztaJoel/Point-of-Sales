using PostOFSales.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace PostOFSales.Areas.Admin.Controllers
{
	public class StockController : Controller
	{
	Point_Of_SalesEntitiesDb DB = new Point_Of_SalesEntitiesDb();
		// GET: Admin/StokeIn
		public ActionResult Index()
		{
			return View();
		}
		public ActionResult Create()
		{
			List<SelectListItem> items = new List<SelectListItem>();
			
			
				//var categoryDetails = new SelectList(db.Categories.ToList(), "categoryId", "categoryName");
				//ViewBag.Category_Id = categoryDetails;
				foreach (var item in DB.Categories.ToList()) {
					items.Add(new SelectListItem
					{
						Text = item.categoryName,
						Value = item.categoryId.ToString(),
					

					});
				};

				ViewBag.DropdownListItems = items;
				ViewBag.colorTextbox =  1;



			int selectedValue = Convert.ToInt32(Request.Form["CategoryItem"]);
			
			List<SelectListItem> colorList = new List<SelectListItem>();
			foreach (var item in DB.ColorTypes.ToList())
			{
				colorList.Add(new SelectListItem
				{
					Text = item.ColorType1,
					Value = item.Id.ToString(),


				});
			};

			ViewBag.ColorDropdownListItems = colorList;

			return View();
		}

		[HttpPost]
		public ActionResult Create(FormCollection form)
		{
			List<SelectListItem> items = new List<SelectListItem>();
			string dropDownSelected = Request.Form["CategoryItem"];

			if (dropDownSelected == "") {

				return RedirectToAction("Create");
			}

			int selectedValue = Convert.ToInt32(dropDownSelected);

			if (selectedValue > 0)
			{
				TempData["SelectedValue"]=selectedValue;
				int produc = DB.Products.Where(x => x.category_Id == selectedValue).Count();

				ViewBag.PrdQty = produc;

				foreach (var item in DB.Categories.ToList())
				{
					items.Add(new SelectListItem
					{
						Text = item.categoryName,
						Value = item.categoryId.ToString(),
						Selected = item.categoryId == selectedValue

					});
				};
			}
			ViewBag.RepostDropdownListItems = items;

			List<SelectListItem> colorList = new List<SelectListItem>();
			string colorDropDownSelected = Request.Form["Color_Id"];
			
			//controlling post back when dropdown valuw is empty
			if (colorDropDownSelected == "")
			{
				List<SelectListItem> colorItems = new List<SelectListItem>();
				foreach (var item in DB.ColorTypes.ToList())
				{
					colorItems.Add(new SelectListItem
					{
						Text = item.ColorType1,
						Value = item.Id.ToString(),


					});
				};

				ViewBag.ColorDropdownListItems = colorItems;

				return View();
			}
			//------------------------------------------------------

			//dropdown when the return value is not empty string
			int ColorSelectedValue = Convert.ToInt32(colorDropDownSelected);

			if (ColorSelectedValue > 0) 
			{
				TempData["ColorSelectedValue"] = ColorSelectedValue;

				foreach (var item in DB.ColorTypes.ToList())
				{
					colorList.Add(new SelectListItem
					{
						Text = item.ColorType1,
						Value = item.Id.ToString(),
						Selected = item.Id == ColorSelectedValue
					});
				};

				ViewBag.RepostColorDropdownListItems = colorList;
			}
			//----------------------------------------------------


			var product = DB.Products.AsQueryable();

			int? categoryId = Convert.ToInt32(form["CategoryItem"]);
			var productName = form["ProductName"];
			int? colorType = Convert.ToInt32(form["Color_Id"]);
			string supplierName = form["SupplierName"];
			
			string Description = (form["Description"]);
		

			
			ViewBag.ProductName = productName;


			if (productName != null && colorType != null && categoryId != null)
			{
				//var product = DB.Products.Where(x => x.ProdutName == productName && x.ColorId == colorType);
				product = product.Where(x => x.ProdutName == productName && x.ColorId == colorType && x.category_Id == categoryId);
				if (product != null) {
					ViewBag.Quantity = product;
					ViewBag.EmptyProduct = product.Count();
				}

			}
			if(product.Count() == 1)
			{
				String AmountStock = (form["Stock"]) ;

				if (AmountStock == "")
				{
					return View();
				};
					int? check = 0;

				foreach (Product Quantity in product)
				{

					if (Convert.ToInt32(AmountStock) == 1)
					{
						check = Convert.ToInt32(Quantity.ProductQuantity) + Convert.ToInt32(AmountStock);

					}

					if (Convert.ToInt32(AmountStock) == 2)
					{
						if (Convert.ToInt32(AmountStock) > Convert.ToInt32(Quantity.ProductQuantity))
						{
							ViewBag.insufficient = "insufficient Product to stock out";
							return View();
						}
						else
						{
							check = Convert.ToInt32(Quantity.ProductQuantity) - Convert.ToInt32(AmountStock);

						}
					}
					try { 
					StockIn stockIn = new StockIn
					{
						ProductName = productName,
						CategoryId = categoryId,
						CurrentQuantityBalance = Quantity.ProductQuantity,
						AmountStockIn = form["Stock"],
						SupplierName = supplierName,
						Color_Id = (form["Color_Id"]),
						Color = "",
						StockType = (form["Stock"]),
						DateStock = DateTime.Now,
						Description = Description,
						TotalBalance = check.ToString(),

					};

					if (ModelState.IsValid)
					{
						DB.StockIns.Add(stockIn);
				
					}
						;
					}

					catch (System.Data.Entity.Validation.DbEntityValidationException dbEx)
					{
						Exception raise = dbEx;
						foreach (var validationErrors in dbEx.EntityValidationErrors)
						{
							foreach (var validationError in validationErrors.ValidationErrors)
							{
								String message = string.Format("{0}:{1}",
															   validationErrors.Entry.Entity.ToString(),
															   validationError.ErrorMessage);
								raise = new InvalidOperationException(message, raise);

							}
							throw raise;

						}
					}
					var ProductToUpdateOnDB = DB.Products.Find(Quantity.Product_Id);

					ProductToUpdateOnDB.ProductQuantity = check.ToString();
					if (ModelState.IsValid)
					{
						DB.Entry(ProductToUpdateOnDB).State = System.Data.Entity.EntityState.Modified;
						
					}
					
				}
				DB.SaveChanges();
			}
	
			


			//Console.WriteLine(quantity.ToString());
			//ViewBag.Quantity = quantity;
			return View();
		}

		public JsonResult GetProduct(string term)
		{
			List<string> product;
			product = DB.Products.Where(x => x.ProdutName.StartsWith(term)).Select(y => y.ProdutName).ToList();
			return Json(product, JsonRequestBehavior.AllowGet);
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