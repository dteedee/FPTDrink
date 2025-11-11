using System.ComponentModel.DataAnnotations;

namespace FPTDrink.API.Validators
{
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false)]
	public sealed class AgeRangeAttribute : ValidationAttribute
	{
		public int Min { get; }
		public int Max { get; }

		public AgeRangeAttribute(int min, int max)
		{
			Min = min;
			Max = max;
			ErrorMessage = $"Độ tuổi phải nằm trong khoảng từ {min} đến {max}.";
		}

		public override bool IsValid(object? value)
		{
			if (value is not DateTime date) return true; // allow null/unspecified, use [Required] if needed
			var today = DateTime.Today;
			int age = today.Year - date.Year;
			if (date.Date > today.AddYears(-age)) age--;
			return age >= Min && age <= Max;
		}
	}
}


