using AutoMapper;
using FPTDrink.API.DTOs.Admin.NhanVien;
using FPTDrink.Core.Models;

namespace FPTDrink.API.Mapping
{
	public class NhanVienProfile : Profile
	{
		public NhanVienProfile()
		{
			CreateMap<NhanVien, NhanVienDto>()
				.ForMember(d => d.TenChucVu, o => o.MapFrom(s => s.IdChucVuNavigation != null ? s.IdChucVuNavigation.TenChucVu : null))
				.ForMember(d => d.IdChucVu, o => o.MapFrom(s => s.IdChucVu));
			CreateMap<NhanVienCreateRequest, NhanVien>();
			CreateMap<NhanVienUpdateRequest, NhanVien>();
		}
	}
}


