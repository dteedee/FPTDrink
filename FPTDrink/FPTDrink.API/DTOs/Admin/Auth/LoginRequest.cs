using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.DTOs.Admin.Auth
{
	public class LoginRequest
	{
		[Required, MinLength(3)]
		public string Username { get; set; } = string.Empty;
		[Required, MinLength(3)]
		public string Password { get; set; } = string.Empty;
	}
}


