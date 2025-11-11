using System.Collections.Concurrent;
using FPTDrink.Core.Interfaces.Services;

namespace FPTDrink.Infrastructure.Services
{
	public class VisitorsOnlineTracker : IVisitorsOnlineTracker
	{
		private readonly ConcurrentDictionary<string, DateTime> _lastSeen = new();

		public void Touch(string key, TimeSpan? ttl = null)
		{
			_lastSeen[key] = DateTime.UtcNow;
			// cleanup occasionally
			if (_lastSeen.Count % 100 == 0)
			{
				var threshold = DateTime.UtcNow - (ttl ?? TimeSpan.FromMinutes(5));
				foreach (var kv in _lastSeen)
				{
					if (kv.Value < threshold)
					{
						_lastSeen.TryRemove(kv.Key, out _);
					}
				}
			}
		}

		public int GetOnlineCount(TimeSpan window)
		{
			var threshold = DateTime.UtcNow - window;
			return _lastSeen.Values.Count(v => v >= threshold);
		}
	}
}


