using FPTDrink.Web.Constants;
using FPTDrink.Web.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[AdminAuthorize]
	public abstract class AdminBaseController : Controller
	{
		protected string? CurrentUserId => HttpContext.Session.GetString(AdminSessionKeys.UserId);
		protected string? CurrentUsername => HttpContext.Session.GetString(AdminSessionKeys.Username);
		protected string? CurrentDisplayName => HttpContext.Session.GetString(AdminSessionKeys.DisplayName);
		protected string? CurrentRole => HttpContext.Session.GetString(AdminSessionKeys.RoleName);

		public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
		{
			ViewBag.AdminDisplayName = CurrentDisplayName;
			ViewBag.AdminRole = CurrentRole;
			base.OnActionExecuting(context);
		}
	}
}

