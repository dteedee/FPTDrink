using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FPTDrink.Core.Models;

namespace FPTDrink.Web.Areas.Admin.ViewModels
{
	public class AdminOrderListItemViewModel
	{
		public string OrderCode { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string? Email { get; set; }
		public DateTime CreatedDate { get; set; }
		public int Status { get; set; }
		public int PaymentMethod { get; set; }
		public decimal TotalAmount { get; set; }
	}

	public class AdminOrderItemViewModel
	{
		public string? ProductId { get; set; }
		public string? ProductTitle { get; set; }
		public int Quantity { get; set; }
		public decimal UnitPrice { get; set; }
		public decimal Discount { get; set; }
		public decimal LineTotal => Quantity * UnitPrice;
	}

	public class AdminOrderDetailViewModel
	{
		public string OrderCode { get; set; } = string.Empty;
		public string CustomerName { get; set; } = string.Empty;
		public string Phone { get; set; } = string.Empty;
		public string? Email { get; set; }
		public string Address { get; set; } = string.Empty;
		public int PaymentMethod { get; set; }
		public int Status { get; set; }
		public DateTime CreatedDate { get; set; }
		public DateTime? UpdatedDate { get; set; }
		public string? Notes { get; set; }
		public List<AdminOrderItemViewModel> Items { get; set; } = new();
		public decimal TotalAmount { get; set; }
		public decimal DiscountAmount { get; set; }
		public decimal ShippingFee { get; set; }
	}

	public class AdminOrderStatusUpdateViewModel
	{
		[Required]
		public string OrderCode { get; set; } = string.Empty;

		[Display(Name = "Trạng thái mới")]
		[Required(ErrorMessage = "Vui lòng chọn trạng thái mới")]
		[Range(0, 4, ErrorMessage = "Trạng thái không hợp lệ")]
		public int NewStatus { get; set; }
	}
}

