using AutoMapper;
using FPTDrink.API.DTOs.Admin.ChucVu;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class ChucVuProfile : Profile
	{
		public ChucVuProfile()
		{
			CreateMap<ChucVu, ChucVuDto>();
			CreateMap<ChucVuCreateRequest, ChucVu>();
			CreateMap<ChucVuUpdateRequest, ChucVu>();
		}
	}
}


