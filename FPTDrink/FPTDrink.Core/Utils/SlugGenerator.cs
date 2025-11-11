using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace FPTDrink.Core.Utils
{
	public static class SlugGenerator
	{
		public static string GenerateSlug(string? input)
		{
			if (string.IsNullOrWhiteSpace(input)) return string.Empty;
			string normalized = input.Normalize(NormalizationForm.FormD);
			var sb = new StringBuilder();
			foreach (var c in normalized)
			{
				var uc = CharUnicodeInfo.GetUnicodeCategory(c);
				if (uc != UnicodeCategory.NonSpacingMark)
				{
					sb.Append(c);
				}
			}
			string cleaned = sb.ToString().Normalize(NormalizationForm.FormC).ToLowerInvariant();
			cleaned = Regex.Replace(cleaned, @"[^a-z0-9\s\-]", "");
			cleaned = Regex.Replace(cleaned, @"\s+", "-");
			cleaned = Regex.Replace(cleaned, @"\-{2,}", "-").Trim('-');
			return cleaned;
		}
	}
}


