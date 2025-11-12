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
				return; // Không có role, từ chối
			}

			// BƯỚC 1: Kiểm tra role có trong danh sách AllowedRoles không
			// Nếu có AllowedRoles và role không nằm trong đó thì từ chối ngay
			if (requirement.AllowedRoles.Length > 0 &&
				!requirement.AllowedRoles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
			{
				return; // Role không được phép, từ chối
			}

			// BƯỚC 2: Nếu không có FeatureCode, chỉ cho phép nếu role nằm trong AllowedRoles
			// (đã kiểm tra ở trên và pass)
			if (string.IsNullOrWhiteSpace(requirement.FeatureCode))
			{
				// Chỉ succeed nếu role đã được kiểm tra và nằm trong AllowedRoles
				if (requirement.AllowedRoles.Length > 0 &&
					requirement.AllowedRoles.Any(r => string.Equals(r, roleName, StringComparison.OrdinalIgnoreCase)))
				{
					context.Succeed(requirement);
				}
				return;
			}

			// BƯỚC 3: Kiểm tra permission
			// "Quản lý" mặc định có full quyền (không cần kiểm tra DB)
			// Các role khác ("Kế toán", "Thu ngân") phải kiểm tra permission trong DB
			if (string.Equals(roleName, "Quản lý", StringComparison.OrdinalIgnoreCase))
			{
				// "Quản lý" có full quyền, luôn cho phép
				context.Succeed(requirement);
				return;
			}

			// Các role khác phải kiểm tra permission trong database
			using var scope = _serviceProvider.CreateScope();
			var rolesRepo = scope.ServiceProvider.GetRequiredService<IChucVuRepository>();
			var permRepo = scope.ServiceProvider.GetRequiredService<IPhanQuyenRepository>();

			var role = rolesRepo.Query().FirstOrDefault(r => r.TenChucVu == roleName);
			if (role == null)
			{
				return; // Không tìm thấy role, từ chối
			}

			// Kiểm tra permission trong database - BẮT BUỘC cho các role khác "Quản lý"
			var hasPerm = await permRepo.Query()
				.Where(p => p.IdchucVu == role.Id && p.MaChucNang == requirement.FeatureCode)
				.AnyAsync();

			if (hasPerm)
			{
				context.Succeed(requirement);
			}
			// Nếu không có permission, không gọi context.Succeed() -> authorization sẽ fail
		}
	}
}


