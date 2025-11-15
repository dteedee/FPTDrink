using AutoMapper;
using FPTDrink.API.DTOs.Public.CustomerAuth;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class CustomerProfile : Profile
	{
		public CustomerProfile()
		{
			CreateMap<KhachHang, CustomerProfileDto>();
		}
	}
}

