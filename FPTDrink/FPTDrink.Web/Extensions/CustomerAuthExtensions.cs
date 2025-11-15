using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;

namespace FPTDrink.Web.Extensions
{
	public static class CustomerAuthExtensions
	{
		public static bool IsCustomerAuthenticated(this HttpContext httpContext)
		{
			var token = httpContext.Request.Cookies["customer_token"];
			if (string.IsNullOrWhiteSpace(token))
				return false;

			try
			{
				var handler = new JwtSecurityTokenHandler();
				if (!handler.CanReadToken(token))
					return false;

				var jwtToken = handler.ReadJwtToken(token);
				var expiry = jwtToken.ValidTo;
				
				// Kiểm tra token còn hạn không
				if (expiry < DateTime.UtcNow)
					return false;

				// Kiểm tra có role Customer không
				var role = jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
				return role == "Customer";
			}
			catch
			{
				return false;
			}
		}

		public static string? GetCustomerId(this HttpContext httpContext)
		{
			var token = httpContext.Request.Cookies["customer_token"];
			if (string.IsNullOrWhiteSpace(token))
				return null;

			try
			{
				var handler = new JwtSecurityTokenHandler();
				if (!handler.CanReadToken(token))
					return null;

				var jwtToken = handler.ReadJwtToken(token);
				return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
			}
			catch
			{
				return null;
			}
		}

		public static string? GetCustomerName(this HttpContext httpContext)
		{
			var token = httpContext.Request.Cookies["customer_token"];
			if (string.IsNullOrWhiteSpace(token))
				return null;

			try
			{
				var handler = new JwtSecurityTokenHandler();
				if (!handler.CanReadToken(token))
					return null;

				var jwtToken = handler.ReadJwtToken(token);
				return jwtToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name)?.Value;
			}
			catch
			{
				return null;
			}
		}
	}
}

