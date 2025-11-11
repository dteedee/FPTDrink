using AutoMapper;
using FPTDrink.API.DTOs.Admin.ProductCategory;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class ProductCategoryProfile : Profile
	{
		public ProductCategoryProfile()
		{
			CreateMap<ProductCategory, ProductCategoryDto>();
			CreateMap<ProductCategoryCreateRequest, ProductCategory>();
			CreateMap(ProductCategoryUpdateRequest, ProductCategory>()
				.ForMember(d => d.MaLoaiSanPham, o => o.MapFrom(s => s.MaLoaiSanPham));
		}
	}
}


