using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace QLXeMay.Helpers
{
    public class ImagePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string path = value as string;
            if (string.IsNullOrWhiteSpace(path))
                return null;

            // Xóa phần cache-busting query string nếu có
            if (path.Contains("?v="))
            {
                path = path.Substring(0, path.IndexOf("?v="));
            }

            // Nếu là đường dẫn tuyệt đối (người dùng vừa chọn xong từ OpenFileDialog)
            if (Path.IsPathRooted(path) && File.Exists(path))
            {
                return CreateImage(path);
            }

            // Lấy thư mục chứa file chạy (.exe)
            string baseDir = AppDomain.CurrentDomain.BaseDirectory;
            string fullPath = "";

            // Nếu DB lưu dạng /Images/XeMay/guid.jpg
            if (path.StartsWith("/") || path.StartsWith("\\"))
            {
                fullPath = Path.Combine(baseDir, path.TrimStart('/', '\\').Replace('/', '\\'));
            }
            else
            {
                // Nếu DB lưu dạng tên file (vd: wave-alpha.jpg)
                fullPath = Path.Combine(baseDir, "Images", "XeMay", path);
            }

            if (File.Exists(fullPath))
                return CreateImage(fullPath);

            return null; // Trả về null nếu không tìm thấy để hiển thị trống
        }

        private BitmapImage CreateImage(string filePath)
        {
            try
            {
                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // Đảm bảo đọc ngay lập tức, không lock file
                image.CreateOptions = BitmapCreateOptions.IgnoreImageCache; // Không dùng cache cũ của WPF
                image.UriSource = new Uri(filePath, UriKind.Absolute);
                image.EndInit();
                image.Freeze(); // Cho phép dùng an toàn trên nhiều thread
                return image;
            }
            catch
            {
                return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
