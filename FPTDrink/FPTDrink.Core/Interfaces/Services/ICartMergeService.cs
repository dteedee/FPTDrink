using System.Collections.Generic;
using FPTDrink.Core.Models;

namespace FPTDrink.Core.Interfaces.Services
{
	public interface ICartMergeService
	{
		Task<IReadOnlyList<GioHang>> GetCustomerCartAsync(string customerId, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> AddItemAsync(string customerId, string productId, int quantity, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> UpdateItemQuantityAsync(string customerId, string productId, int quantity, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> RemoveItemAsync(string customerId, string productId, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> ClearAsync(string customerId, CancellationToken cancellationToken = default);
		Task<(bool success, string? error)> MergeGuestCartAsync(string customerId, string guestCartId, CancellationToken cancellationToken = default);
	}
}

