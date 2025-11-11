using System.ComponentModel.DataAnnotations;
using System.Globalization;

namespace FPTDrink.API.DTOs.Admin.Statistics
{
	public class DateRangeRequest : IValidatableObject
	{
		[Required]
		[RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Định dạng ngày phải dd/MM/yyyy")]
		public string FromDay { get; set; } = string.Empty;

		[Required]
		[RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Định dạng ngày phải dd/MM/yyyy")]
		public string ToDay { get; set; } = string.Empty;

		public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
		{
			if (DateTime.TryParseExact(FromDay, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var from) &&
				DateTime.TryParseExact(ToDay, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var to))
			{
				if (from > to)
				{
					yield return new ValidationResult("FromDay phải nhỏ hơn hoặc bằng ToDay", new[] { nameof(FromDay), nameof(ToDay) });
				}
			}
		}
	}

	public class SingleDateRequest
	{
		[Required]
		[RegularExpression(@"^\d{2}/\d{2}/\d{4}$", ErrorMessage = "Định dạng ngày phải dd/MM/yyyy")]
		public string Date { get; set; } = string.Empty;
	}
}


