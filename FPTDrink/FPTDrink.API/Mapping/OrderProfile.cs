using AutoMapper;
using FPTDrink.API.DTOs.Admin.Order;
using FPTDrink.Core.Models;
using FPTDrink.Core.Models.Reports;

namespace FPTDrink.API.Mapping
{
	public class OrderProfile : Profile
	{
		public OrderProfile()
		{
			CreateMap<HoaDon, OrderDto>();
			CreateMap<ChiTietHoaDon, OrderItemDto>()
				.ForMember(d => d.ProductName, o => o.MapFrom(s => s.Product != null ? s.Product.Title : null))
				.ForMember(d => d.ProductImage, o => o.MapFrom(s => s.Product != null ? s.Product.Image : null));

			CreateMap<CustomerSummaryInfo, CustomerSummaryDto>();
			CreateMap<CustomerOrderBrief, CustomerOrderBriefDto>();
			CreateMap<CustomerDetailsInfo, CustomerDetailsDto>();
		}
	}
}


