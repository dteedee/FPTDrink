using System.Security.Claims;
using FPTDrink.Core.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.API.Authorization
{
	public class PermissionHandler : AuthorizationHandler<PermissionRequirement>
	{
		private readonly IServiceProvider _serviceProvider;

		public PermissionHandler(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
		}

		protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, PermissionRequirement requirement)
		{
			var roleName = context.User.FindFirstValue(ClaimTypes.Role) ?? context.User.FindFirstValue("role") ?? string.Empty;
			if (string.IsNullOrWhiteSpace(roleName))
			{
				return;
			}

			// If allowed roles specified and current role not in set => fail
			if (requirement.AllowedRoles.Length > 0 &&
				!requirement.AllowedRoles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
			{
				return;
			}

			// If feature code is empty, role check above is enough
			if (string.IsNullOrWhiteSpace(requirement.FeatureCode))
			{
				context.Succeed(requirement);
				return;
			}

			// Verify permission in DB
			using var scope = _serviceProvider.CreateScope();
			var rolesRepo = scope.ServiceProvider.GetRequiredService<IChucVuRepository>();
			var permRepo = scope.ServiceProvider.GetRequiredService<IPhanQuyenRepository>();

			var role = rolesRepo.Query().FirstOrDefault(r => r.TenChucVu == roleName);
			if (role == null)
			{
				return;
			}

			var hasPerm = await permRepo.Query()
				.Where(p => p.IdchucVu == role.Id && p.MaChucNang == requirement.FeatureCode)
				.AnyAsync();

			if (hasPerm)
			{
				context.Succeed(requirement);
			}
		}
	}
}


