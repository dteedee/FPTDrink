using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace FPTDrink.Web.Services
{
	public class ApiClient
	{
		private readonly HttpClient _httpClient;
		private readonly ILogger<ApiClient>? _logger;

		public ApiClient(IHttpClientFactory httpClientFactory, ILogger<ApiClient>? logger = null)
		{
			_httpClient = httpClientFactory.CreateClient("FPTDrinkApi");
			_logger = logger;
		}

		public async Task<T?> GetAsync<T>(string url, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.GetAsync(url, cancellationToken);
				
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger?.LogError("API trả về lỗi {StatusCode}: {Url}. Nội dung: {ErrorContent}", 
						response.StatusCode, url, errorContent);
					return default(T);
				}
				
				return await response.Content.ReadFromJsonAsync<T>(cancellationToken: cancellationToken);
			}
			catch (HttpRequestException ex)
			{
				_logger?.LogError(ex, "Lỗi kết nối API: {Url}. Đảm bảo API đang chạy tại {BaseAddress}", url, _httpClient.BaseAddress);
				return default(T);
			}
			catch (TaskCanceledException ex)
			{
				_logger?.LogError(ex, "Timeout khi gọi API: {Url}", url);
				return default(T);
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Lỗi không xác định khi gọi API: {Url}", url);
				return default(T);
			}
		}

		public async Task<HttpResponseMessage> PostAsync<T>(string url, T payload, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.PostAsJsonAsync(url, payload, cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger?.LogError("API POST trả về lỗi {StatusCode}: {Url}. Nội dung: {ErrorContent}", 
						response.StatusCode, url, errorContent);
				}
				return response;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Lỗi khi POST API: {Url}", url);
				throw;
			}
		}

		public async Task<HttpResponseMessage> PutAsync<T>(string url, T payload, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.PutAsJsonAsync(url, payload, cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger?.LogError("API PUT trả về lỗi {StatusCode}: {Url}. Nội dung: {ErrorContent}", 
						response.StatusCode, url, errorContent);
				}
				return response;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Lỗi khi PUT API: {Url}", url);
				throw;
			}
		}

		public async Task<HttpResponseMessage> DeleteAsync(string url, CancellationToken cancellationToken = default)
		{
			try
			{
				var response = await _httpClient.DeleteAsync(url, cancellationToken);
				if (!response.IsSuccessStatusCode)
				{
					var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
					_logger?.LogError("API DELETE trả về lỗi {StatusCode}: {Url}. Nội dung: {ErrorContent}", 
						response.StatusCode, url, errorContent);
				}
				return response;
			}
			catch (Exception ex)
			{
				_logger?.LogError(ex, "Lỗi khi DELETE API: {Url}", url);
				throw;
			}
		}
	}
}
