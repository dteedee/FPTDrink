using AutoMapper;
using FPTDrink.API.DTOs.Public.Catalog;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class CatalogProfile : Profile
	{
		public CatalogProfile()
		{
			CreateMap<Product, ProductListItemDto>()
				.ForMember(d => d.GiaNiemYet, o => o.MapFrom(s => s.GiaNiemYet))
				.ForMember(d => d.GiaHienThi, o => o.MapFrom(s => s.GiaBan ?? s.GiaNiemYet))
				.ForMember(d => d.ProductCategoryTitle, o => o.MapFrom(s => s.ProductCategory != null ? s.ProductCategory.Title : null))
				.ForMember(d => d.SupplierTitle, o => o.MapFrom(s => s.Supplier != null ? s.Supplier.Title : null));
		}
	}
}


