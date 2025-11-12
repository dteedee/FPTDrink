using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace FPTDrink.Web.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ tên của bạn")]
        [RegularExpression(@"^[\p{L}\s]{2,50}$", ErrorMessage = "Họ tên không được chứa ký tự đặc biệt hoặc số")]
        public string TenKhachHang { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [RegularExpression(@"^0[1-9]\d{8}$", ErrorMessage = "Số điện thoại phải có 10 số, bắt đầu bằng 0 và số thứ 2 khác 0")]
        public string SoDienThoai { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        [StringLength(255, MinimumLength = 5, ErrorMessage = "Địa chỉ phải có ít nhất 5 ký tự")]
        public string DiaChi { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Email không được chứa dấu và phải có @")]
        public string Email { get; set; } = string.Empty;

        public int TypePayment { get; set; } = 1;
		public int TypePaymentVN { get; set; } = 2;

        public List<CartItem> Items { get; set; } = new();

        public class CartItem
        {
            public string ProductId { get; set; } = string.Empty;
            public int Quantity { get; set; }
        }
    }

    public class CreateOrderResponse
    {
        public string? OrderCode { get; set; }
		public OrderDetailViewModel? Order { get; set; }
    }

    public class VnPayInitResponse
    {
        public string? PaymentUrl { get; set; }
    }
}
