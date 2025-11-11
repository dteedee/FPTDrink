using Microsoft.AspNetCore.Authorization;

namespace FPTDrink.API.Authorization
{
	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
	public sealed class PermissionAuthorizeAttribute : AuthorizeAttribute
	{
		// Policy name format: perm:{feature}|{role1},{role2}
		public PermissionAuthorizeAttribute(string featureCode, params string[] roles)
		{
			string rolesPart = roles != null && roles.Length > 0
				? string.Join(",", roles)
				: string.Empty;
			Policy = $"perm:{featureCode}|{rolesPart}";
		}
	}
}


