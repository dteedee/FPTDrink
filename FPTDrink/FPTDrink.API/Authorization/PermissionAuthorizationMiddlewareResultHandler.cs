using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;

namespace FPTDrink.API.Authorization
{
	public class PermissionAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
	{
		private readonly AuthorizationMiddlewareResultHandler _defaultHandler = new();

		public async Task HandleAsync(
			RequestDelegate next,
			HttpContext context,
			AuthorizationPolicy policy,
			PolicyAuthorizationResult authorizeResult)
		{
			// Nếu authorization fail và user đã authenticated, trả về thông báo lỗi
			if (!authorizeResult.Succeeded && context.User.Identity?.IsAuthenticated == true)
			{
				// Kiểm tra xem có phải là Forbidden (403) không
				if (authorizeResult.Forbidden)
				{
					context.Response.StatusCode = 403;
					context.Response.ContentType = "application/json";
					await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
					{
						success = false,
						message = "Bạn không có quyền thực hiện chức năng này.",
						error = "FORBIDDEN"
					}));
					return;
				}
			}

			// Nếu không phải Forbidden hoặc chưa authenticated, dùng default handler
			await _defaultHandler.HandleAsync(next, context, policy, authorizeResult);
		}
	}
}


