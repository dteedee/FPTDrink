using System.Text.RegularExpressions;

namespace FPTDrink.Web.Helpers
{
    public static class ImagePathHelper
    {
        /// <summary>
        /// Normalize image path để đảm bảo hiển thị đúng từ wwwroot
        /// Hỗ trợ các format:
        /// - /Uploads/files/... (từ database)
        /// - /Uploads/images/ProductCategory/... (từ database)
        /// - ~/Uploads/... (đã normalize)
        /// - Uploads/... (relative path)
        /// - http://... hoặc https://... (external URL)
        /// </summary>
        /// <param name="imagePath">Đường dẫn ảnh từ database</param>
        /// <returns>Đường dẫn đã được normalize (luôn bắt đầu bằng ~/)</returns>
        public static string NormalizeImagePath(string? imagePath)
        {
            // Nếu null hoặc empty, trả về placeholder
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return "~/images/placeholder.png";
            }

            // Trim whitespace
            imagePath = imagePath.Trim();

            // Nếu đã bắt đầu bằng ~/, giữ nguyên (đã normalize)
            if (imagePath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // Nếu là external URL (http:// hoặc https://), giữ nguyên
            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // Nếu bắt đầu bằng /, thêm ~ để map tới wwwroot
            // Ví dụ: /Uploads/files/image.jpg → ~/Uploads/files/image.jpg
            if (imagePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                // Đảm bảo không có double slash
                string normalized = "~" + imagePath;
                normalized = Regex.Replace(normalized, @"(?<!:)/{2,}", "/"); // Replace multiple slashes (except after :)
                return normalized;
            }

            // Nếu không có prefix và không phải URL, thêm ~/Uploads/
            // Ví dụ: files/image.jpg → ~/Uploads/files/image.jpg
            // Hoặc: Uploads/files/image.jpg → ~/Uploads/files/image.jpg
            string trimmed = imagePath.TrimStart('/');
            
            // Nếu đã có Uploads/ ở đầu, chỉ cần thêm ~/
            if (trimmed.StartsWith("Uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return "~/" + trimmed;
            }

            // Nếu không có Uploads/, thêm ~/Uploads/
            return "~/Uploads/" + trimmed;
        }

        /// <summary>
        /// Normalize image path và trả về URL trực tiếp (không cần Url.Content)
        /// Sử dụng khi path đã bắt đầu bằng / (absolute path)
        /// </summary>
        public static string GetImageUrl(string? imagePath)
        {
            if (string.IsNullOrWhiteSpace(imagePath))
            {
                return "/images/placeholder.png";
            }

            imagePath = imagePath.Trim();

            // Nếu là external URL, giữ nguyên
            if (imagePath.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                imagePath.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // Nếu bắt đầu bằng /, giữ nguyên (đã là absolute path)
            if (imagePath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath;
            }

            // Nếu bắt đầu bằng ~/, convert thành /
            if (imagePath.StartsWith("~/", StringComparison.OrdinalIgnoreCase))
            {
                return imagePath.Substring(1); // Bỏ ~
            }

            // Nếu không có prefix, thêm /Uploads/
            string trimmed = imagePath.TrimStart('/');
            if (trimmed.StartsWith("Uploads/", StringComparison.OrdinalIgnoreCase))
            {
                return "/" + trimmed;
            }

            return "/Uploads/" + trimmed;
        }

        /// <summary>
        /// Kiểm tra xem path có phải là external URL không
        /// </summary>
        public static bool IsExternalUrl(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            return path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                   path.StartsWith("https://", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Kiểm tra xem path có hợp lệ không (không chứa ký tự nguy hiểm)
        /// </summary>
        public static bool IsValidPath(string? path)
        {
            if (string.IsNullOrWhiteSpace(path))
                return false;

            // Không cho phép các ký tự nguy hiểm
            if (path.Contains("..") || path.Contains("//") || path.Contains("\\"))
                return false;

            // Nếu là external URL, luôn hợp lệ
            if (IsExternalUrl(path))
                return true;

            return true;
        }

        /// <summary>
        /// Lấy đường dẫn placeholder mặc định
        /// </summary>
        public static string GetPlaceholderPath()
        {
            return "~/images/placeholder.png";
        }

        /// <summary>
        /// Debug: Log image path để kiểm tra
        /// </summary>
        public static string DebugImagePath(string? imagePath)
        {
            var normalized = NormalizeImagePath(imagePath);
            System.Diagnostics.Debug.WriteLine($"[ImagePathHelper] Original: '{imagePath}' → Normalized: '{normalized}'");
            return normalized;
        }
    }
}
