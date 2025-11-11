using FPTDrink.Core.Interfaces.Services;
using FPTDrink.Core.Models.Reports;
using FPTDrink.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace FPTDrink.Infrastructure.Services
{
	public class VisitorStatsService : IVisitorStatsService
	{
		private readonly FptdrinkContext _db;
		private readonly IVisitorsOnlineTracker _tracker;

		public VisitorStatsService(FptdrinkContext db, IVisitorsOnlineTracker tracker)
		{
			_db = db;
			_tracker = tracker;
		}

		public async Task<VisitorStats> GetVisitorStatsAsync(CancellationToken cancellationToken = default)
		{
			// Dựa trên bảng ThongKe hiện có
			var today = DateTime.Today;
			var yesterday = today.AddDays(-1);
			var startOfWeek = today.AddDays(-(int)today.DayOfWeek + (int)DayOfWeek.Monday);
			var startOfPrevWeek = startOfWeek.AddDays(-7);
			var startOfMonth = new DateTime(today.Year, today.Month, 1);
			var startOfPrevMonth = startOfMonth.AddMonths(-1);
			var endOfPrevMonth = startOfMonth.AddDays(-1);

			// Đếm theo bảng ThongKe (giả định có cột ThoiGian)
			int homNay = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date == today, cancellationToken);
			int homQua = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date == yesterday, cancellationToken);
			int tuanNay = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date >= startOfWeek && x.ThoiGian.Date <= today, cancellationToken);
			int tuanTruoc = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date >= startOfPrevWeek && x.ThoiGian.Date < startOfWeek, cancellationToken);
			int thangNay = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date >= startOfMonth && x.ThoiGian.Date <= today, cancellationToken);
			int thangTruoc = await _db.ThongKes.CountAsync(x => x.ThoiGian.Date >= startOfPrevMonth && x.ThoiGian.Date <= endOfPrevMonth, cancellationToken);
			int tatCa = await _db.ThongKes.CountAsync(cancellationToken);

			var stats = new VisitorStats
			{
				HomNay = homNay.ToString(),
				HomQua = homQua.ToString(),
				TuanNay = tuanNay.ToString(),
				TuanTruoc = tuanTruoc.ToString(),
				ThangNay = thangNay.ToString(),
				ThangTruoc = thangTruoc.ToString(),
				TatCa = tatCa.ToString(),
				VisitorsOnline = _tracker.GetOnlineCount(TimeSpan.FromMinutes(5))
			};
			return stats;
		}
	}
}


