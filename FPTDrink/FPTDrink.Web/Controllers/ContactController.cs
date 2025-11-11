using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Controllers
{
    public class ContactController : Controller
    {
        public IActionResult Index()
        {
            ViewData["Title"] = "Liên hệ";
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SendMessage([FromForm] ContactMessageViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewData["Title"] = "Liên hệ";
                return View("Index", model);
            }

            TempData["SuccessMessage"] = "Cảm ơn bạn đã liên hệ! Chúng tôi sẽ phản hồi sớm nhất có thể.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Subscribe([FromForm] string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
            {
                TempData["SubscribeError"] = "Vui lòng nhập email hợp lệ.";
                return RedirectToAction("Index");
            }

            TempData["SubscribeSuccess"] = "Đăng ký thành công! Bạn sẽ nhận được voucher giảm giá 30% qua email.";
            return RedirectToAction("Index");
        }
    }

    public class ContactMessageViewModel
    {
        public string? HoTen { get; set; }
        public string? Email { get; set; }
        public string? LoiNhan { get; set; }
    }
}

