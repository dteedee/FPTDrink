using System.Security.Claims;
using FPTDrink.API.DTOs.Public.Checkout;
using FPTDrink.API.Extensions;
using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
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
		private readonly ICartMergeService _cartMergeService;

		public CheckoutController(ICheckoutService checkoutService, IPaymentService paymentService, IHoaDonRepository orderRepo, IConfiguration config, IEmailService emailService, ILogger<CheckoutController> logger, ICartMergeService cartMergeService)
		{
			_checkoutService = checkoutService;
			_paymentService = paymentService;
			_orderRepo = orderRepo;
			_config = config;
			_emailService = emailService;
			_logger = logger;
			_cartMergeService = cartMergeService;
		}

		[HttpPost("order")]
		[Authorize(Policy = "VerifiedCustomer")]
		[ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
		[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
		public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto req, CancellationToken ct)
		{
			if (!ModelState.IsValid) return ValidationProblem(ModelState);
			var customerId = User.FindFirstValue(ClaimTypes.NameIdentifier);
			if (string.IsNullOrWhiteSpace(customerId)) return Forbid();
			var coreReq = new FPTDrink.Core.Interfaces.Services.CreateOrderRequest
			{
				CustomerId = customerId,
				TenKhachHang = req.TenKhachHang,
				SoDienThoai = req.SoDienThoai,
				DiaChi = req.DiaChi,
				Email = req.Email,
				TypePayment = req.TypePayment,
				Items = req.Items.Select(i => new FPTDrink.Core.Interfaces.Services.CreateOrderItemRequest { ProductId = i.ProductId, Quantity = i.Quantity }).ToList()
			};
			var order = await _checkoutService.CreateOrderAsync(coreReq, ct);
			await _cartMergeService.ClearAsync(customerId, ct);
			var persisted = await _orderRepo.GetByIdAsync(order.MaHoaDon, ct);
			// Only send email to admin when order is created, not to customer
			_ = Task.Run(async () =>
			{
				try
				{
					if (persisted != null)
					{
						await SendOrderEmailAsync(persisted, admin: true, ct);
					}
				}
				catch (Exception ex)
				{
					_logger.LogError(ex, "Send order email to admin failed for {OrderCode}", order.MaHoaDon);
				}
			});
			var location = Url.ActionLink(action: "VnPayReturn", controller: "Checkout", values: null, protocol: Request.Scheme);
			OrderDetailDto? dto = persisted != null ? OrderDetailMapper.ToDto(persisted) : null;
			return Created(location ?? string.Empty, new { orderCode = order.MaHoaDon, order = dto });
		}

		[HttpPost("payment/vnpay")]
		[Authorize(Policy = "VerifiedCustomer")]
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
			var url = _paymentService.CreateVnPayUrl(order, req.TypePaymentVN, returnUrl, vnpUrl, tmnCode, hashSecret, req.ClientIp ?? string.Empty);
			return Ok(new { paymentUrl = url });
		}

		[HttpPost("order/{orderCode}/send-email")]
		[ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
		[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
		public async Task<IActionResult> SendInvoice(string orderCode, CancellationToken ct)
		{
			if (string.IsNullOrWhiteSpace(orderCode)) return BadRequest("Mã đơn hàng không hợp lệ.");
			var order = await _orderRepo.GetByIdAsync(orderCode, ct);
			if (order == null) return NotFound("Không tìm thấy đơn hàng.");
			if (string.IsNullOrWhiteSpace(order.Email)) return BadRequest("Đơn hàng không có email khách hàng.");

			try
			{
				await SendOrderEmailAsync(order, admin: false, ct);
				return Ok(new { message = "Hoá đơn đã được gửi tới email khách hàng." });
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Send invoice email failed for {OrderCode}", orderCode);
				return StatusCode(StatusCodes.Status500InternalServerError, "Gửi hoá đơn thất bại. Vui lòng thử lại.");
			}
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
			string secureHash = qs.TryGetValue("vnp_SecureHash", out var hashValues) ? hashValues.ToString() : string.Empty;
			bool validSignature = VerifyHmacSha512(hashSecret, rawData, secureHash);
			string orderCode = qs.TryGetValue("vnp_TxnRef", out var orderValues) ? orderValues.ToString() : string.Empty;
			string responseCode = qs.TryGetValue("vnp_ResponseCode", out var responseValues) ? responseValues.ToString() : string.Empty;
			string status = qs.TryGetValue("vnp_TransactionStatus", out var statusValues) ? statusValues.ToString() : string.Empty;
			long amount = 0;
			if (qs.TryGetValue("vnp_Amount", out var amountValues))
			{
				long.TryParse(amountValues, out amount);
			}
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
					// Do not send email automatically after successful payment
					// Customer will request invoice via SendInvoice endpoint
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
				OrderCode = orderCode ?? string.Empty,
				Amount = amount,
				ResponseCode = responseCode ?? string.Empty,
				TransactionStatus = status ?? string.Empty,
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
			
			_logger.LogInformation("Loading email template from: {TemplatePath}", tplPath);
			_logger.LogInformation("Base directory: {BaseDir}", baseDir);
			_logger.LogInformation("Template file exists: {Exists}", System.IO.File.Exists(tplPath));
			
			string template = System.IO.File.Exists(tplPath) ? await System.IO.File.ReadAllTextAsync(tplPath, ct) : "";
			if (string.IsNullOrWhiteSpace(template))
			{
				_logger.LogWarning("Template file not found or empty, using fallback template. Path: {TemplatePath}", tplPath);
				template = "<p>Đơn hàng #{{MaDon}}</p><table>{{SanPham}}</table><p>Tổng: {{TongTien}} VNĐ</p>";
			}
			else
			{
				_logger.LogInformation("Template loaded successfully from: {TemplatePath}", tplPath);
			}
			var rows = string.Join("", order.ChiTietHoaDons.Select((x, i) =>
			{
				string productName = x.Product?.Title ?? x.ProductId ?? "N/A";
				string rowBg = i % 2 == 0 ? "background: rgba(245, 237, 229, 0.3);" : "background: rgba(237, 228, 216, 0.2);";
				return $"<tr style=\"{rowBg}\">" +
					$"<td style=\"padding: 14px 12px; text-align: center; font-size: 14px; color: #5e4a36; font-weight: 600; border-right: 1px solid rgba(212, 165, 116, 0.2); border-bottom: 1px solid rgba(212, 165, 116, 0.15);\">{i + 1}</td>" +
					$"<td style=\"padding: 14px 12px; text-align: center; font-size: 14px; color: #6b4f33; font-weight: 500; border-right: 1px solid rgba(212, 165, 116, 0.2); border-bottom: 1px solid rgba(212, 165, 116, 0.15);\">{productName}</td>" +
					$"<td style=\"padding: 14px 12px; text-align: center; font-size: 14px; color: #5e4a36; font-weight: 600; border-right: 1px solid rgba(212, 165, 116, 0.2); border-bottom: 1px solid rgba(212, 165, 116, 0.15);\">{x.SoLuong}</td>" +
					$"<td style=\"padding: 14px 12px; text-align: right; font-size: 14px; color: #5e4a36; font-weight: 500; border-right: 1px solid rgba(212, 165, 116, 0.2); border-bottom: 1px solid rgba(212, 165, 116, 0.15);\">{x.GiaBan:N0} VNĐ</td>" +
					$"<td style=\"padding: 14px 12px; text-align: right; font-size: 14px; color: #6b4f33; font-weight: 700; border-bottom: 1px solid rgba(212, 165, 116, 0.15);\">{(x.GiaBan * x.SoLuong):N0} VNĐ</td>" +
					$"</tr>";
			}));
			var thanhTien = order.ChiTietHoaDons.Sum(x => x.GiaBan * x.SoLuong);
			var tongTien = thanhTien; // có thể cộng thêm phí nếu cần
			var body = template
				.Replace("{{NgayDat}}", order.CreatedDate.ToString("dd"))
				.Replace("{{ThangDat}}", order.CreatedDate.ToString("MM"))
				.Replace("{{NamDat}}", order.CreatedDate.ToString("yyyy"))
				.Replace("{{MaDon}}", order.MaHoaDon)
				.Replace("{{TenKhachHang}}", order.TenKhachHang ?? "")
				.Replace("{{DiaChiNhanHang}}", order.DiaChi ?? "")
				.Replace("{{Phone}}", order.SoDienThoai ?? "")
				.Replace("{{Email}}", order.Email ?? "")
				.Replace("{{HinhThucThanhToan}}", order.PhuongThucThanhToan == 1 ? "Thanh toán khi nhận hàng (COD)" : order.PhuongThucThanhToan == 2 ? "Chuyển khoản" : "Mua trực tiếp tại cửa hàng")
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

