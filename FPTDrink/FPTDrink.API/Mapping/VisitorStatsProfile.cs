using AutoMapper;
using FPTDrink.API.DTOs.Public.Home;
using FPTDrink.Core.Models.Reports;

namespace FPTDrink.API.Mapping
{
	public class VisitorStatsProfile : Profile
	{
		public VisitorStatsProfile()
		{
			CreateMap<VisitorStats, VisitorStatsDto>();
		}
	}
}


