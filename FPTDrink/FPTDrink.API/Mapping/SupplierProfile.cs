using AutoMapper;
using FPTDrink.API.DTOs.Admin.Suppliers;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class SupplierProfile : Profile
	{
		public SupplierProfile()
		{
			CreateMap<NhaCungCap, SupplierDto>();
			CreateMap<SupplierCreateRequest, NhaCungCap>();
			CreateMap<SupplierUpdateRequest, NhaCungCap>()
				.ForMember(d => d.MaNhaCungCap, o => o.MapFrom(s => s.MaNhaCungCap));
		}
	}
}


