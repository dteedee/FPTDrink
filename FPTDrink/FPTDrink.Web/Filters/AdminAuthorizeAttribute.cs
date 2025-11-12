using FPTDrink.Web.Constants;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;

namespace FPTDrink.Web.Filters
{
	public class AdminAuthorizeAttribute : ActionFilterAttribute
	{
		public override void OnActionExecuting(ActionExecutingContext context)
		{
			var httpContext = context.HttpContext;
			if (httpContext.Session == null)
			{
				context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
				{
					area = "Admin",
					controller = "Auth",
					action = "Login"
				}));
				return;
			}

			var userId = httpContext.Session.GetString(AdminSessionKeys.UserId);
			if (string.IsNullOrWhiteSpace(userId))
			{
				context.Result = new RedirectToRouteResult(new RouteValueDictionary(new
				{
					area = "Admin",
					controller = "Auth",
					action = "Login"
				}));
				return;
			}
			base.OnActionExecuting(context);
		}
	}
}

