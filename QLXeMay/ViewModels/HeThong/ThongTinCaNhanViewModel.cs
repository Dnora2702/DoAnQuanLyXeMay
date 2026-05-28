using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using QLXeMay.Models;
using System.Data.Entity;

namespace QLXeMay.ViewModels.HeThong
{
    public class ThongTinCaNhanViewModel : BaseViewModel
    {
        private NhanVien _nhanVienInfo;
        public NhanVien NhanVienInfo
        {
            get => _nhanVienInfo;
            set { _nhanVienInfo = value; OnPropertyChanged(); }
        }

        private string _avatarPath;
        public string AvatarPath
        {
            get => _avatarPath;
            set
            {
                _avatarPath = value;
                OnPropertyChanged();
            }
        }

        public ICommand ChonAnhCommand { get; set; }
        public ICommand SaveCommand { get; set; }

        public ThongTinCaNhanViewModel()
        {
            LoadData();

            ChonAnhCommand = new RelayCommand<object>((p) => true, async (p) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.Filter = "Image files (*.png;*.jpeg;*.jpg)|*.png;*.jpeg;*.jpg";
                if (openFileDialog.ShowDialog() == true)
                {
                    string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars");
                    if (!Directory.Exists(targetFolder)) Directory.CreateDirectory(targetFolder);

                    string fileName = App.CurrentUser + Path.GetExtension(openFileDialog.FileName);
                    string destFile = Path.Combine(targetFolder, fileName);

                    // Copy và ghi đè
                    File.Copy(openFileDialog.FileName, destFile, true);
                    
                    // Thêm cache-buster để ép WPF nhận diện đây là chuỗi mới và gọi lại Converter
                    AvatarPath = destFile + "?v=" + DateTime.Now.Ticks;
                    
                    System.Windows.MessageBox.Show("Cập nhật ảnh đại diện thành công. Bạn có thể cần đăng nhập lại để cập nhật trên toàn hệ thống.", "Thành công", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
                }
            });

            SaveCommand = new RelayCommand<object>((p) => NhanVienInfo != null, async (p) =>
            {
                using (var db = new QLXeMayEntities())
                {
                    var nv = await db.NhanViens.FirstOrDefaultAsync(x => x.TenDangNhap == App.CurrentUser);
                    if (nv != null)
                    {
                        nv.TenNV = NhanVienInfo.TenNV;
                        nv.SDT = NhanVienInfo.SDT;
                        nv.DiaChi = NhanVienInfo.DiaChi;
                        await db.SaveChangesAsync();
                        System.Windows.MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo");
                        
                        // Kích hoạt event để MainWindow load lại Tên NV nếu cần
                        App.Current.Properties["ForceRefreshName"] = true;
                    }
                }
            });
        }

        private void LoadData()
        {
            using (var db = new QLXeMayEntities())
            {
                var nv = db.NhanViens.FirstOrDefault(x => x.TenDangNhap == App.CurrentUser);
                if (nv != null)
                {
                    // Copy data để bind
                    NhanVienInfo = new NhanVien
                    {
                        MaNV = nv.MaNV,
                        TenDangNhap = nv.TenDangNhap,
                        TenNV = nv.TenNV,
                        SDT = nv.SDT,
                        DiaChi = nv.DiaChi
                    };
                }
            }

            // Load Avatar
            LoadAvatar();
        }

        private void LoadAvatar()
        {
            if (string.IsNullOrEmpty(App.CurrentUser)) return;
            string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars");
            string[] extensions = { ".jpg", ".png", ".jpeg" };
            foreach (var ext in extensions)
            {
                string possiblePath = Path.Combine(targetFolder, App.CurrentUser + ext);
                if (File.Exists(possiblePath))
                {
                    AvatarPath = possiblePath;
                    return;
                }
            }
            AvatarPath = "/Images/Icons/user.png"; // Mặc định
        }
    }
}
