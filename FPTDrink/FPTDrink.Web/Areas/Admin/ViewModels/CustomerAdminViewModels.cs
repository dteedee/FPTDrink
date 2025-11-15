using System;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminCustomerListItemViewModel
	{
		public string Id { get; set; } = string.Empty;
		public string Username { get; set; } = string.Empty;
		public string FullName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string? Address { get; set; }
		public bool IsActive { get; set; }
		public bool IsEmailVerified { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? LastLoginDate { get; set; }
		public int TotalOrders { get; set; }
		public decimal TotalSpent { get; set; }
		public int Status { get; set; }
	}

	public class AdminCustomerTopSpenderViewModel
	{
		public string CustomerId { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string Email { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public int TotalOrders { get; set; }
		public decimal TotalSpent { get; set; }
		public DateTime? LastOrderDate { get; set; }
	}
}

