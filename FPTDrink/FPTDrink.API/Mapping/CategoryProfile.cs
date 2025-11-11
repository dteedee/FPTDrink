using AutoMapper;
using FPTDrink.API.DTOs.Admin.Category;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class CategoryProfile : Profile
	{
		public CategoryProfile()
		{
			CreateMap<Category, CategoryDto>();
			CreateMap<CategoryCreateRequest, Category>();
			CreateMap<CategoryUpdateRequest, Category>();
		}
	}
}


