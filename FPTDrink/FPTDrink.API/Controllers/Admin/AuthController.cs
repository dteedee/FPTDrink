using FPTDrink.API.DTOs.Admin.Auth;
using FPTDrink.Core.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FPTDrink.API.Controllers.Admin
{
	[ApiController]
	[Route("api/admin/[controller]")]
	public class AuthController : ControllerBase
	{
		private readonly IAuthService _authService;

		public AuthController(IAuthService authService)
		{
			_authService = authService;
		}

		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
		{
			var (success, error, user) = await _authService.LoginAsync(request.Username, request.Password, ct);
			if (!success) return BadRequest(new LoginResponse { Success = false, Error = error });

			var response = new LoginResponse
			{
				Success = true,
				User = new UserDto
				{
					Id = user!.Id,
					TenDangNhap = user.TenDangNhap,
					TenHienThi = user.TenHienThi,
					FullName = user.FullName,
					IsActiveAccount = user.IsActiveAccount
				}
			};
			return Ok(response);
		}

		[HttpPost("logout")]
		public async Task<IActionResult> Logout()
		{
			await _authService.LogoutAsync();
			return Ok(new { success = true });
		}
	}
}


