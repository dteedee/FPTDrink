using Microsoft.AspNetCore.Authorization;

namespace FPTDrink.API.Authorization
{
	public class PermissionRequirement : IAuthorizationRequirement
	{
		public string FeatureCode { get; }
		public string[] AllowedRoles { get; }

		public PermissionRequirement(string featureCode, string[] allowedRoles)
		{
			FeatureCode = featureCode;
			AllowedRoles = allowedRoles ?? Array.Empty<string>();
		}
	}
}


