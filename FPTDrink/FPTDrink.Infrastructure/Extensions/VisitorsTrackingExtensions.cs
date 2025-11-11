using FPTDrink.Infrastructure.Middlewares;

namespace FPTDrink.Infrastructure.Extensions
{
	public static class VisitorsTrackingExtensions
	{
		public static IApplicationBuilder UseVisitorsTracking(this IApplicationBuilder app)
		{
			return app.UseMiddleware<VisitorsTrackingMiddleware>();
		}
	}
}


