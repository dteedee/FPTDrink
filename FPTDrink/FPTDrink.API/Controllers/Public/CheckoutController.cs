using FPTDrink.API.DTOs.Public.Checkout;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace FPTDrink.API.Controllers.Public
{
	[ApiController]
	[Route("api/public/[controller]")]
	public class CheckoutController : ControllerBase
	{
		private readonly ICheckoutService _checkoutService;
		private readonly IPaymentService _paymentService;
		private readonly IHoaDonRepository _orderRepo;
		private readonly IConfiguration _config;
		private readonly IEmailService _emailService;
		private readonly ILogger<CheckoutController> _logger;

		public CheckoutController(ICheckoutService checkoutService, IPaymentService paymentService, IHoaDonRepository orderRepo, IConfiguration config, IEmailService emailService, ILogger<CheckoutController> logger)
		{
			_checkoutService = checkoutService;
			_paymentService = paymentService;
			_orderRepo = orderRepo;
			_config = config;
			_emailService = emailService;
			_logger = logger;
		}

		[HttpPost("order")]
		[ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var coreReq = new FPTDrink.Core.Interfaces.Services.CreateOrderRequest
			{
				TenKhachHang = req.TenKhachHang,
				SoDienThoai = req.SoDienThoai,
				DiaChi = req.DiaChi,
				Email = req.Email,
				CCCD = req.CCCD,
				TypePayment = req.TypePayment,
				Items = req.Items.Select(i => new FPTDrink.Core.Interfaces.Services.CreateOrderItemRequest { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
			};
			var order = await _checkoutService.CreateOrderAsync(coreReq, ct);
			_ = Task.Run(async () =>
			{
				try
				{
					await SendOrderEmailAsync(order, admin: true, ct);
					if (!string.IsNullOrWhiteSpace(order.Email))
					{
						await SendOrderEmailAsync(order, admin: false, ct);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Send order email failed for {OrderCode}", order.MaHoaDon);
				}
			});
			var location = Url.ActionLink(action: "VnPayReturn", controller: "Checkout", values: null, protocol: Request.Scheme);
			return Created(location ?? string.Empty, new { orderCode = order.MaHoaDon });
		}

		[HttpPost("payment/vnpay")]
		[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> InitVnPay([FromBody] VnPayInitRequest req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var order = await _orderRepo.GetByIdAsync(req.OrderCode, ct);
			if (order == null) return NotFound("Không tìm thấy đơn hàng");
			string returnUrl = req.ReturnUrlOverride ?? _config["VNPay:ReturnUrl"] ?? "";
			string vnpUrl = _config["VNPay:Url"] ?? "";
			string tmnCode = _config["VNPay:TmnCode"] ?? "";
			string hashSecret = _config["VNPay:HashSecret"] ?? "";
			var url = _paymentService.CreateVnPayUrl(order, req.TypePaymentVN, returnUrl, vnpUrl, tmnCode, hashSecret);
			return Ok(new { paymentUrl = url });
		}

		[HttpGet("payment/vnpay-return")]
		[ProducesResponseType(typeof(VnPayReturnDto), StatusCodes.Status200OK)]
		public async Task<IActionResult> VnPayReturn(CancellationToken ct)
		{
			var qs = HttpContext.Request.Query;
			// verify signature
			var dict = qs.Where(kv => kv.Key.StartsWith("vnp_") && kv.Key != "vnp_SecureHash" && kv.Key != "vnp_SecureHashType")
				.ToDictionary(k => k.Key, v => v.Value.ToString());
			var rawData = string.Join("&", dict.OrderBy(k => k.Key).Select(kv => $"{kv.Key}={Uri.EscapeDataString(kv.Value)}"));
			string hashSecret = _config["VNPay:HashSecret"] ?? "";
			string secureHash = qs["vnp_SecureHash"];
			bool validSignature = VerifyHmacSha512(hashSecret, rawData, secureHash);
			string orderCode = qs["vnp_TxnRef"];
			string responseCode = qs["vnp_ResponseCode"];
			string status = qs["vnp_TransactionStatus"];
			long amount = 0;
			long.TryParse(qs["vnp_Amount"], out amount);
			amount = amount / 100;
			bool success = validSignature && responseCode == "00" && status == "00";

			if (!string.IsNullOrEmpty(orderCode))
			{
				var order = await _orderRepo.GetByIdAsync(orderCode, ct);
				if (order != null && success)
				{
					order.TrangThai = 2;
					_orderRepo.Update(order);
					await _orderRepo.SaveChangesAsync(ct);
					// send success email to customer
					if (!string.IsNullOrWhiteSpace(order.Email))
					{
						await SendPaidEmailAsync(order, amount, ct);
					}
				}
				else if (order != null && !success && !string.IsNullOrWhiteSpace(order.Email))
				{
					try
					{
						await SendFailedEmailAsync(order, responseCode, ct);
					}
					catch (Exception ex)
					{
						_logger.LogError(ex, "Send failed-payment email failed for {OrderCode}", order.MaHoaDon);
					}
				}
			}
			return Ok(new VnPayReturnDto
			{
				OrderCode = orderCode,
				Amount = amount,
				ResponseCode = responseCode,
				TransactionStatus = status,
				Success = success
			});
		}

		private static bool VerifyHmacSha512(string secret, string data, string expected)
		{
			if (string.IsNullOrWhiteSpace(secret) || string.IsNullOrWhiteSpace(expected)) return false;
			using var h = new System.Security.Cryptography.HMACSHA512(System.Text.Encoding.UTF8.GetBytes(secret));
			var bytes = h.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
			var hex = BitConverter.ToString(bytes).Replace("-", "").ToUpperInvariant();
			return string.Equals(hex, expected.ToUpperInvariant(), StringComparison.Ordinal);
		}

		private async Task SendOrderEmailAsync(FPTDrink.Core.Models.HoaDon order, bool admin, CancellationToken ct)
		{
			string to = admin ? (_config["Email:Admin"] ?? _config["Email:Sender"] ?? "") : (order.Email ?? "");
			if (string.IsNullOrWhiteSpace(to)) return;
			// load template
			string baseDir = AppContext.BaseDirectory;
			string tplFile = admin ? "invoice_admin.html" : "invoice.html";
			string tplPath = System.IO.Path.Combine(baseDir, "Templates", tplFile);
			string template = System.IO.File.Exists(tplPath) ? await System.IO.File.ReadAllTextAsync(tplPath, ct) : "";
			if (string.IsNullOrWhiteSpace(template))
			{
				template = "<p>Đơn hàng #{{MaDon}}</p><table>{{SanPham}}</table><p>Tổng: {{TongTien}} VNĐ</p>";
			}
			var rows = string.Join("", order.ChiTietHoaDons.Select((x, i) =>
				$"<tr><td style=\"text-align:center; width: 40px;\">{i + 1}</td><td style=\"text-align:center;width: 150px;\">{x.ProductId}</td><td style=\"text-align:center;width: 80px;\">{x.SoLuong}</td><td style=\"text-align:center;\">{x.GiaBan:N0}</td><td style=\"text-align:center;\">{(x.GiaBan * x.SoLuong):N0}</td></tr>"));
			var thanhTien = order.ChiTietHoaDons.Sum(x => x.GiaBan * x.SoLuong);
			var tongTien = thanhTien; // có thể cộng thêm phí nếu cần
			var body = template
				.Replace("{{NgayDat}}", order.CreatedDate.ToString("dd"))
				.Replace("{{ThangDat}}", order.CreatedDate.ToString("MM"))
				.Replace("{{NamDat}}", order.CreatedDate.ToString("yyyy"))
				.Replace("{{MaDon}}", order.MaHoaDon)
				.Replace("{{TenKhachHang}}", order.TenKhachHang ?? "")
				.Replace("{{CCCD}}", order.Cccd ?? "")
				.Replace("{{DiaChiNhanHang}}", order.DiaChi ?? "")
				.Replace("{{Phone}}", order.SoDienThoai ?? "")
				.Replace("{{HinhThucThanhToan}}", order.PhuongThucThanhToan == 1 ? "COD" : order.PhuongThucThanhToan == 2 ? "Chuyển khoản" : "Mua trực tiếp")
				.Replace("{{SanPham}}", rows)
				.Replace("{{ThanhTien}}", string.Format("{0:N0}", thanhTien))
				.Replace("{{TongTien}}", string.Format("{0:N0}", tongTien));
			string subject = admin ? $"Đơn hàng mới #{order.MaHoaDon}" : $"Hoá đơn #{order.MaHoaDon}";
			await _emailService.SendAsync(to, subject, body, ct);
		}

		private async Task SendPaidEmailAsync(FPTDrink.Core.Models.HoaDon order, long amount, CancellationToken ct)
		{
			string to = order.Email ?? "";
			if (string.IsNullOrWhiteSpace(to)) return;
			string baseDir = AppContext.BaseDirectory;
			string tplPath = System.IO.Path.Combine(baseDir, "Templates", "payment_success.html");
			string template = System.IO.File.Exists(tplPath) ? await System.IO.File.ReadAllTextAsync(tplPath, ct) : "";
			if (string.IsNullOrWhiteSpace(template))
			{
				template = "<p>Thanh toán thành công đơn hàng #{{MaDon}}. Số tiền: {{SoTien}} VNĐ.</p>";
			}
			var body = template
				.Replace("{{TenKhachHang}}", order.TenKhachHang ?? "")
				.Replace("{{MaDon}}", order.MaHoaDon)
				.Replace("{{NgayDat}}", order.CreatedDate.ToString("dd"))
				.Replace("{{ThangDat}}", order.CreatedDate.ToString("MM"))
				.Replace("{{NamDat}}", order.CreatedDate.ToString("yyyy"))
				.Replace("{{SoTien}}", string.Format("{0:N0}", amount));
			await _emailService.SendAsync(to, $"Thanh toán thành công #{order.MaHoaDon}", body, ct);
		}

		private async Task SendFailedEmailAsync(FPTDrink.Core.Models.HoaDon order, string responseCode, CancellationToken ct)
		{
			string to = order.Email ?? "";
			if (string.IsNullOrWhiteSpace(to)) return;
			string baseDir = AppContext.BaseDirectory;
			string tplPath = System.IO.Path.Combine(baseDir, "Templates", "payment_failed.html");
			string template = System.IO.File.Exists(tplPath) ? await System.IO.File.ReadAllTextAsync(tplPath, ct) : "";
			if (string.IsNullOrWhiteSpace(template))
			{
				template = "<p>Thanh toán đơn #{{MaDon}} không thành công. Mã lỗi: {{MaLoi}}</p>";
			}
			var body = template
				.Replace("{{TenKhachHang}}", order.TenKhachHang ?? "")
				.Replace("{{MaDon}}", order.MaHoaDon)
				.Replace("{{NgayDat}}", order.CreatedDate.ToString("dd"))
				.Replace("{{ThangDat}}", order.CreatedDate.ToString("MM"))
				.Replace("{{NamDat}}", order.CreatedDate.ToString("yyyy"))
				.Replace("{{MaLoi}}", responseCode ?? "");
			await _emailService.SendAsync(to, $"Thanh toán chưa thành công #{order.MaHoaDon}", body, ct);
		}
	}
}


