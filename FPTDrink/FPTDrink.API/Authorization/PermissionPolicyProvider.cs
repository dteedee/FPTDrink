using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace FPTDrink.API.Authorization
{
	public class PermissionPolicyProvider : DefaultAuthorizationPolicyProvider
	{
		public PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : base(options) { }

		public override Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
		{
			if (policyName.StartsWith("perm:", StringComparison.OrdinalIgnoreCase))
			{
				// perm:{feature}|{role1,role2}
				var rule = policyName.Substring("perm:".Length);
				var parts = rule.Split('|');
				var feature = parts.Length > 0 ? parts[0] : string.Empty;
				var rolesPart = parts.Length > 1 ? parts[1] : string.Empty;
				var roles = string.IsNullOrWhiteSpace(rolesPart) ? Array.Empty<string>() : rolesPart.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

				var policy = new AuthorizationPolicyBuilder()
					.AddRequirements(new PermissionRequirement(feature, roles))
					.Build();
				return Task.FromResult<AuthorizationPolicy?>(policy);
			}
			return base.GetPolicyAsync(policyName);
		}
	}
}


