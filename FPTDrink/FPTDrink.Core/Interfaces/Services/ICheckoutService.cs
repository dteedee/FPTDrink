using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public class CreateOrderItemRequest
	{
		public string ProductId { get; set; } = string.Empty;
		public int Quantity { get; set; }
	}

	public class CreateOrderRequest
	{
		public string TenKhachHang { get; set; } = string.Empty;
		public string SoDienThoai { get; set; } = string.Empty;
		public string DiaChi { get; set; } = string.Empty;
		public string? Email { get; set; }
		public int TypePayment { get; set; }
		public List<CreateOrderItemRequest> Items { get; set; } = new List<CreateOrderItemRequest>();
	}

	public interface ICheckoutService
	{
		Task<HoaDon> CreateOrderAsync(CreateOrderRequest request, CancellationToken cancellationToken = default);
	}

	public interface IPaymentService
	{
		string CreateVnPayUrl(HoaDon order, int typePaymentVN, string returnUrl, string vnpUrl, string tmnCode, string hashSecret, string clientIp);
	}
}

