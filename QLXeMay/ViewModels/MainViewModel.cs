using System.Windows.Input;
using System.Linq;
using System.IO;
using QLXeMay.ViewModels.DanhMuc;
using QLXeMay.ViewModels.NghiepVu;
using QLXeMay.ViewModels.HeThong;
using QLXeMay.Models;
using System;

namespace QLXeMay.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _currentUserDisplayName;
        public string CurrentUserDisplayName
        {
            get => _currentUserDisplayName;
            set { _currentUserDisplayName = value; OnPropertyChanged(); }
        }

        private string _currentUserAvatar;
        public string CurrentUserAvatar
        {
            get => _currentUserAvatar;
            set { _currentUserAvatar = value; OnPropertyChanged(); }
        }
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        // Khai báo Command để chuyển sang màn hình Hãng Xe
        public ICommand ShowHangXeCommand { get; set; }
        public ICommand ShowNhanVienCommand { get; set; }
        public ICommand ShowXeMayCommand { get; set; }
        public ICommand ShowKhachHangCommand { get; set; }
        public ICommand ShowHoaDonCommand { get; set; }
        public ICommand ShowNhaCungCapCommand { get; set; }
        public ICommand ShowHeThongCommand { get; set; }
        public ICommand ShowTrangChuCommand { get; set; }
        public ICommand ShowProfileCommand { get; set; }

        public MainViewModel()
        {
            LoadUserInfo();
            App.Current.Properties["ForceRefreshName"] = false;
            // Timer to check for name refresh or just load on init
            var dispatcherTimer = new System.Windows.Threading.DispatcherTimer();
            dispatcherTimer.Tick += (s, e) => {
                if (App.Current.Properties["ForceRefreshName"] != null && (bool)App.Current.Properties["ForceRefreshName"])
                {
                    LoadUserInfo();
                    App.Current.Properties["ForceRefreshName"] = false;
                }
            };
            dispatcherTimer.Interval = TimeSpan.FromSeconds(1);
            dispatcherTimer.Start();
   
             CurrentView = new TrangChuViewModel();
            ShowTrangChuCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new TrangChuViewModel();
            });

            ShowHangXeCommand = new RelayCommand<object>((p) => true, (p) =>
            {
                 CurrentView = new HangXeViewModel();
            });
            ShowNhanVienCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new NhanVienViewModel();
            });
            ShowXeMayCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new XeMayViewModel();
            });
            ShowKhachHangCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new KhachHangViewModel();
            });
            ShowHoaDonCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new HoaDonViewModel();
            });
            ShowNhaCungCapCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new NhaCungCapViewModel();
            });
            ShowHeThongCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new BackupRestoreViewModel();
            });

            ShowProfileCommand = new RelayCommand<object>((p) => true, (p) => {
                CurrentView = new ThongTinCaNhanViewModel();
            });
        }

        private void LoadUserInfo()
        {
            using (var db = new QLXeMayEntities())
            {
                var nv = db.NhanViens.FirstOrDefault(x => x.TenDangNhap == App.CurrentUser);
                if (nv != null)
                {
                    CurrentUserDisplayName = nv.TenNV;
                }
                else
                {
                    CurrentUserDisplayName = App.CurrentUser; // fallback
                }
            }

            // Load Avatar
            string targetFolder = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Images", "Avatars");
            string[] extensions = { ".jpg", ".png", ".jpeg" };
            bool found = false;
            foreach (var ext in extensions)
            {
                string possiblePath = Path.Combine(targetFolder, App.CurrentUser + ext);
                if (File.Exists(possiblePath))
                {
                    CurrentUserAvatar = possiblePath + "?v=" + DateTime.Now.Ticks;
                    found = true;
                    break;
                }
            }
            if (!found) CurrentUserAvatar = "/Images/Icons/user.png";
        }
    }
}