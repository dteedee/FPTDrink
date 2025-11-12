using FPTDrink.Core.Interfaces.Repositories;
using FPTDrink.Web.Constants;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Web.Filters
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false)]
	public class PermissionAuthorizeAttribute : ActionFilterAttribute
	{
		public string FeatureCode { get; }
		public string[] AllowedRoles { get; }

		public PermissionAuthorizeAttribute(string featureCode, params string[] allowedRoles)
		{
			FeatureCode = featureCode;
			AllowedRoles = allowedRoles ?? Array.Empty<string>();
		}

		public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
		{
			var httpContext = context.HttpContext;
			var userId = httpContext.Session?.GetString(AdminSessionKeys.UserId);
			var roleName = httpContext.Session?.GetString(AdminSessionKeys.RoleName);

			// BƯỚC 1: Kiểm tra đăng nhập
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

			// BƯỚC 2: Kiểm tra role
			if (string.IsNullOrWhiteSpace(roleName))
			{
				context.Result = new ViewResult
				{
					ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
						new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
						context.ModelState)
                    {
                        ["Message"] = "Bạn không có quyền thực hiện chức năng này."
                    }

                };
				return;
			}

			// BƯỚC 3: Kiểm tra role có trong AllowedRoles không
			// Nếu có AllowedRoles và role không nằm trong đó thì từ chối ngay
			if (AllowedRoles.Length > 0 &&
				!AllowedRoles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
			{
				context.Result = new ViewResult
				{
					ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
						new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
						context.ModelState)
                    {
                        ["Message"] = "Bạn không có quyền thực hiện chức năng này."
                    }

                };
				return;
			}

			// BƯỚC 4: "Quản lý" mặc định có full quyền (không cần kiểm tra DB)
			if (string.Equals(roleName, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				await next();
				return;
			}

			// BƯỚC 5: Các role khác ("Kế toán", "Thu ngân") PHẢI kiểm tra permission trong database
			// Đây là điều kiện BẮT BUỘC - không có bypass
			var scopeFactory = httpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
			using var scope = scopeFactory.CreateScope();
			var rolesRepo = scope.ServiceProvider.GetRequiredService<IChucVuRepository>();
			var permRepo = scope.ServiceProvider.GetRequiredService<IPhanQuyenRepository>();

			var role = await rolesRepo.Query()
				.FirstOrDefaultAsync(r => r.TenChucVu == roleName);

			if (role == null)
			{
				context.Result = new ViewResult
				{
					ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
						new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
						context.ModelState)
                    {
                        ["Message"] = "Bạn không có quyền thực hiện chức năng này."
                    }

                };
				return;
			}

			// Kiểm tra permission trong database - BẮT BUỘC
			// Nếu không có permission trong DB, từ chối ngay
			var hasPerm = await permRepo.Query()
				.Where(p => p.IdchucVu == role.Id && p.MaChucNang == FeatureCode)
				.AnyAsync();

			if (!hasPerm)
			{
				context.Result = new ViewResult
				{
					ViewName = "Error",
                    ViewData = new Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary(
						new Microsoft.AspNetCore.Mvc.ModelBinding.EmptyModelMetadataProvider(),
						context.ModelState)
                    {
                        ["Message"] = "Bạn không có quyền thực hiện chức năng này."
                    }

                };
				return;
			}

			// Có quyền, cho phép thực hiện action
			await next();
		}
	}
}

