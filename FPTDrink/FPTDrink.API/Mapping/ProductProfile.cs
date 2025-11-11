using AutoMapper;
using FPTDrink.API.DTOs.Admin.Products;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class ProductProfile : Profile
	{
		public ProductProfile()
		{
			CreateMap<Product, ProductDto>()
				.ForMember(d => d.ProductCategoryTitle, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Title : null))
				.ForMember(d => d.SupplierTitle, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Title : null));
			CreateMap<ProductCreateRequest, Product>();
			CreateMap<ProductUpdateRequest, Product>()
				.ForMember(d => d.MaSanPham, o => o.MapFrom(s => s.MaSanPham));
			CreateMap<Product, FPTDrink.API.DTOs.Public.Products.ProductDetailDto>()
				.ForMember(d => d.ProductCategoryTitle, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Title : null))
				.ForMember(d => d.SupplierTitle, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Title : null));
		}
	}
}


