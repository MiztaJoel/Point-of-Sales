using PagedList;
using PostOFSales.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using System.Web.Services.Description;

namespace PostOFSales.Areas.Admin.Controllers
{
	public class ProductController : Controller
	{
		Point_Of_SalesEntitiesDb DB = new Point_Of_SalesEntitiesDb();

		public ActionResult Create()
		{
			try
			{
				using (Point_Of_SalesEntitiesDb db = new Point_Of_SalesEntitiesDb())
				{
					var productDetails = new SelectList(db.ProductTypes.ToList(), "Id", "TypeName");
					ViewBag.ProductTypeId = productDetails;

					var categoryDetails = new SelectList(db.Categories.ToList(), "categoryId", "categoryName");
					ViewBag.Category_Id = categoryDetails;

					var modelTypes = new SelectList(db.ProductModels.ToList(), "ModelId", "Name");
					ViewBag.Model_Id = modelTypes;

					var ColorType = new SelectList(db.ColorTypes.ToList(), "Id", "ColorType1");
					ViewBag.ColorType = ColorType;
				}

			}
			catch (Exception e)
			{
				Console.WriteLine(e.Message);
			}

			return View();
		}
		[HttpPost]
		[ValidateAntiForgeryToken]
		public ActionResult Create([Bind(Include = "Id, Quantity,Name,UnitPrice,Barcode,Category_Id,Model_Id,ProductTypeId,ImageFile,ColorId")] ProductModelType productModel)
		{
			if (ModelState.IsValid)
			{
				try
				{
					using (Point_Of_SalesEntitiesDb db = new Point_Of_SalesEntitiesDb())
					{
						var productDetails = new SelectList(db.ProductTypes.ToList(), "Id", "TypeName");
						ViewBag.ProductTypeId = productDetails;

						var categoryDetails = new SelectList(db.Categories.ToList(), "categoryId", "categoryName");
						ViewBag.Category_Id = categoryDetails;

						var modelTypes = new SelectList(db.ProductModels.ToList(), "ModelId", "Name");
						ViewBag.Model_Id = modelTypes;

						var ColorType = new SelectList(db.ColorTypes.ToList(), "Id", "ColorType");
						ViewBag.ColorType = ColorType;
					}
				}
				catch (Exception e)
				{
					Console.WriteLine(e.Message);
				}

				if (productModel.ImageFile != null && productModel.ImageFile.ContentLength > 0)
				{
					string fileExtension = Path.GetExtension(productModel.ImageFile.FileName);

					if (fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".jpg")
					{
						string fileName = Guid.NewGuid().ToString() + fileExtension;
						string upLoadFolderPath = Server.MapPath("~/UploadedImage/");

						if (!Directory.Exists(upLoadFolderPath))
						{
							Directory.CreateDirectory(upLoadFolderPath);
						}
						string ImagePath = upLoadFolderPath + fileName;
						string ImageSave = "~/UploadedImage/" + fileName;
						productModel.ImageFile.SaveAs(ImagePath);

						Product product = new Product
						{
							ProdutName = productModel.Name,
							Barcode = productModel.Barcode,
							category_Id = Convert.ToInt32(productModel.Category_Id),
							Color = productModel.Color,
							DateCreated = DateTime.Now,
							Description = productModel.Description,
							Image = ImageSave,
							UnitPrice = productModel.UnitPrice,
							ProductQuantity = productModel.Quantity,
							Model_Id = Convert.ToInt32(productModel.Model_Id),
							ProductTypeId = Convert.ToInt32(productModel.ProductTypeId),
							ColorId = productModel.ColorId
						};
						if (ModelState.IsValid)
						{
							DB.Products.Add(product);
							DB.SaveChanges();
						}

						return RedirectToAction("index");
					}

				}
			}
			else { ModelState.AddModelError(" ", "Only image files(.jpg,.jpeg,.png) are allowed"); }
			return View(productModel);
		}
		[ActionName("Index")]
		public ActionResult Index(string sortBy,string searchBy,string search, string searchAll, int? page)
		{
			//searchby for radio button, sortBy hyperlink on header element, searc textbox associated with raddio button
			ViewBag.SortNameParameter = string.IsNullOrEmpty(sortBy) ? "Name desc" : sortBy;
			ViewBag.SortDateParameter = sortBy == "Date" ? "Date desc" : "Date";
			var product = DB.Products.AsQueryable();

			//var product = DB.Products.OrderByDescending(x=>x.Product_Id).ToList();

			//Search All

			if (searchAll != null)
			{
				product = product.Where(x=>x.ProdutName.Contains(searchAll) 
														||x.Description.Contains(searchAll)
														||x.Color.Contains(searchAll)
														||x.ProductType.TypeName.Contains(searchAll)
														||x.ProductModel.Name.Contains(searchAll)
														||x.ColorType.ColorType1.Contains(searchAll)
														);
			}

			if(searchBy == "Name")
			{
				product = product.Where(x => x.ProdutName.StartsWith(search) || search == null);
			
			}
			else
			{
				product = product.Where(x => x.Color == search || search == null);
			};

			switch (sortBy)
			{
				case "Name desc":
					product = product.OrderByDescending(x=>x.ProdutName);
					break;

				case "Date desc":
					product = product.OrderByDescending(x=>x.DateCreated);
					break;

				case "date":
					product = product.OrderBy((x)=>x.DateCreated);
					break;

				default:
					product=product.OrderBy(x=>x.ProdutName);
					break;
			}


			return View(product.ToList().ToPagedList(page ?? 1, 3));
		}

		public ActionResult Detail(int? id)
		{
			if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			//var product = DB.Products.Single(prod=>prod.Product_Id ==id);
			Product product = DB.Products.Find(id);
			if (product == null) return HttpNotFound();

			return View(product);
		}

		public ActionResult Edit(int? id)
		{


			if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
			Product product = DB.Products.Find(id);

			//dropdown for product model
			var productModel = DB.ProductModels.OrderBy(x => x.Name).ToList();

			ViewBag.productModelList = ToSelectList(productModel);

			//dropdown for category 

			var productCategory = DB.Categories.OrderBy(x => x.categoryName).ToList();
			ViewBag.categoryModelList = ToSelectListCategory(productCategory);

			//dropdown for productType

			var productType = DB.ProductTypes.OrderBy(x => x.TypeName).ToList();
			ViewBag.productTypeList = ToSelectListProduct(productType);

			return View(product);
		}
		[HttpPost]
		public ActionResult Edit(Product product, HttpPostedFileBase image)
		{
			Product productfrmDb = DB.Products.Single(x => x.Product_Id == product.Product_Id);
			string ImagePathToTarget = productfrmDb.Image;


			if (image != null)
			{
				string fileExtension = Path.GetExtension(image.FileName);

				if (fileExtension.ToLower() == ".jpeg" || fileExtension.ToLower() == ".png" || fileExtension.ToLower() == ".jpg")
				{
					string fileName = Guid.NewGuid().ToString() + fileExtension;
					string upLoadFolderPath = Server.MapPath("~/UploadedImage/");

					string ImagePath = upLoadFolderPath + fileName;
					string ImageSave = "~/UploadedImage/" + fileName;



					//string[] targetPath = ImagePathToTarget.Split('/');
					//string ImageGuid = targetPath[2];

					string physicalPath = Server.MapPath(ImagePathToTarget);

					productfrmDb.ProdutName = product.ProdutName;
					productfrmDb.Barcode = product.Barcode;
					productfrmDb.category_Id = product.category_Id;
					productfrmDb.Color = product.Color;
					productfrmDb.DateCreated = DateTime.Today;
					productfrmDb.Description = product.Description;

					productfrmDb.Image = ImageSave;
					productfrmDb.UnitPrice = product.UnitPrice;
					productfrmDb.ProductQuantity = product.ProductQuantity;
					productfrmDb.Model_Id = product.Model_Id;
					productfrmDb.ProductTypeId = product.ProductTypeId;


					FileInfo fileInfo = new FileInfo(physicalPath);

					if (ModelState.IsValid)
					{
						if (fileInfo.Exists)
						{
							fileInfo.Delete();
							DB.Entry(productfrmDb).State = System.Data.Entity.EntityState.Modified;
							//Path to save after saving database
							image.SaveAs(ImagePath);
							DB.SaveChanges();
							return RedirectToAction("Index");
							//return Json(new {success=true, Message = "File deleted Successfully"});


						}
						else
						{
							return Json(new { success = false, Message = "file not Deleted" });
						}

					};

				}
			}
			else
			{
				productfrmDb.ProdutName = product.ProdutName;
				productfrmDb.Barcode = product.Barcode;
				productfrmDb.category_Id = product.category_Id;
				productfrmDb.Color = product.Color;
				productfrmDb.DateCreated = DateTime.Today;
				productfrmDb.Description = product.Description;

				productfrmDb.Image = ImagePathToTarget;

				productfrmDb.UnitPrice = product.UnitPrice;
				productfrmDb.ProductQuantity = product.ProductQuantity;
				productfrmDb.Model_Id = product.Model_Id;
				productfrmDb.ProductTypeId = product.ProductTypeId;

				DB.Entry(productfrmDb).State = System.Data.Entity.EntityState.Modified;
				DB.SaveChanges();


			}

			return RedirectToAction("ProductList");
		}

		public ActionResult Delete(int? id)
		{
			if (id == null) return new HttpStatusCodeResult(HttpStatusCode.BadRequest);

			Product product = DB.Products.Find(id);


			if (product == null) return new HttpStatusCodeResult(HttpStatusCode.NotFound);
			string ImagePathToTarget = product.Image;
			//string[] targetPath = ImagePathToTarget.Split('/');
			//string ImageGuid = targetPath[2];
			string physicalPath = Server.MapPath(ImagePathToTarget);

			FileInfo fileInfo = new FileInfo(physicalPath);
			if (ModelState.IsValid)
			{
				try
				{
					if (fileInfo.Exists)
					{
						fileInfo.Delete();
						DB.Products.Remove(product);
						DB.SaveChanges();
					}
					else
					{
						Console.WriteLine("This Image can't be deleted");
					}
				}
				catch (Exception ex) {
					throw new Exception(ex.Message);
				}
			}
			return RedirectToAction("Index");
		}



		[NonAction]
		public SelectList ToSelectList(List<ProductModel> productModels)
		{
			List<SelectListItem> list = new List<SelectListItem>();

			foreach (ProductModel productModel in productModels)
			{
				list.Add(new SelectListItem()
				{
					Text = productModel.Name,
					Value = productModel.ModelId.ToString(),
				});
			}
			return new SelectList(list, "value", "Text");
		}



		[NonAction]
		public SelectList ToSelectListCategory(List<Category> categories)
		{
			List<SelectListItem> list = new List<SelectListItem>();

			foreach (Category categoryModel in categories)
			{
				list.Add(new SelectListItem()
				{
					Text = categoryModel.categoryName,
					Value = categoryModel.categoryId.ToString(),
				});
			}
			return new SelectList(list, "value", "Text");
		}

		[NonAction]
		public SelectList ToSelectListProduct(List<ProductType> productTypes)
		{
			List<SelectListItem> list = new List<SelectListItem>();

			foreach (ProductType productType in productTypes)
			{
				list.Add(new SelectListItem()
				{
					Text = productType.TypeName,
					Value = productType.Id.ToString(),
				});
			}
			return new SelectList(list, "value", "Text");
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