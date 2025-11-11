namespace FPTDrink.Core.Interfaces.Services
{
	public interface IVisitorsOnlineTracker
	{
		void Touch(string key, TimeSpan? ttl = null);
		int GetOnlineCount(TimeSpan window);
	}
}


