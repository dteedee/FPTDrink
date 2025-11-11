using System.Security.Cryptography;
using System.Text;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Http;

namespace FPTDrink.Infrastructure.Middlewares
{
	public class VisitorsTrackingMiddleware
	{
		private readonly RequestDelegate _next;

		public VisitorsTrackingMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task Invoke(HttpContext context, IVisitorsOnlineTracker tracker)
		{
			string ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			string ua = context.Request.Headers.UserAgent.ToString();
			string path = context.Request.Path.ToString();
			// Hash to avoid storing raw UA
			using var sha = SHA256.Create();
			var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes($"{ip}|{ua}|{path}"));
			string key = Convert.ToBase64String(bytes);
			tracker.Touch(key, TimeSpan.FromMinutes(5));
			await _next(context);
		}
	}
}


