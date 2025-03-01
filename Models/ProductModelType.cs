using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace PostOFSales.Models
{
	public class ProductModelType
	{
		public int Id { get; set; }
		[Required]
		public string Quantity { get; set; }
		[Required]
		public string Name { get; set; }
		public string Description { get; set; }
		public string Color { get; set; }
		[Display(Name = "Price Per Unit")]
		public int UnitPrice { get; set; }
		public string Barcode { get; set; }
		public Nullable<System.DateTime> DateCreated { get; set; }
		[Display(Name ="Category Name")]
		public int Category_Id { get; set; }
		[Display(Name = "Model Id")]
		[Required]
		public int Model_Id { get; set; }
		[Display(Name = "Product Type")]
		[Required]
		public int ProductTypeId { get; set; }
		[Display(Name = "Browse Image")]
		public HttpPostedFileBase ImageFile { get; set; }

		public int ColorId { get; set; }


	}
}