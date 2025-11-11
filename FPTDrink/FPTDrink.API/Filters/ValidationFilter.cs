using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace FPTDrink.API.Filters
{
	public class ValidationFilter : IActionFilter
	{
		public void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.ModelState.IsValid)
			{
				var problemDetails = new ValidationProblemDetails(context.ModelState)
				{
					Status = StatusCodes.Status400BadRequest,
					Title = "Dữ liệu không hợp lệ",
				};
				context.Result = new BadRequestObjectResult(problemDetails);
			}
		}

		public void OnActionExecuted(ActionExecutedContext context)
		{
		}
	}
}


