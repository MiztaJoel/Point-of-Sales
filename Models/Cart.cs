using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PostOFSales.Models
{
	public class Cart
	{
		public Product Product { get; set; }
		public int Quatity { get; set; }

		public Cart(Product product, int quatity)
		{
			Product = product;
			Quatity = quatity;
		}
	}
}