using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLXeMay.Models;

namespace QLXeMay.ViewModels.DanhMuc
{
    public class NhanVienViewModel : BaseViewModel
    {
        private ObservableCollection<NhanVien> _listNhanVien;
        public ObservableCollection<NhanVien> ListNhanVien { get => _listNhanVien; set { _listNhanVien = value; OnPropertyChanged(); } }

        // Các thuộc tính bám sát database
        private string _tenNV; public string TenNV { get => _tenNV; set { _tenNV = value; OnPropertyChanged(); } }
        private string _sdt; public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(); } }
        private string _diaChi; public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }
        private string _tenDangNhap; public string TenDangNhap { get => _tenDangNhap; set { _tenDangNhap = value; OnPropertyChanged(); } }
        private string _matKhau; public string MatKhau { get => _matKhau; set { _matKhau = value; OnPropertyChanged(); } }

        private NhanVien _selectedItem;
        public NhanVien SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenNV = SelectedItem.TenNV;
                    SDT = SelectedItem.SDT;
                    DiaChi = SelectedItem.DiaChi;
                    TenDangNhap = SelectedItem.TenDangNhap;
                    MatKhau = ""; // Không hiển thị mật khẩu cũ vì bảo mật
                }
            }
        }

        public RelayCommand<object> AddCommand { get; set; }
        public RelayCommand<object> UpdateCommand { get; set; }
        public RelayCommand<object> DeleteCommand { get; set; }

        public NhanVienViewModel()
        {
            LoadData();

            // Thêm mới
            AddCommand = new RelayCommand<object>((p) => !string.IsNullOrWhiteSpace(TenNV), (p) => {
                using (var db = new QLXeMayEntities())
                {
                    if (!string.IsNullOrWhiteSpace(TenDangNhap))
                    {
                        if (db.TaiKhoans.Any(x => x.TenDangNhap.ToLower() == TenDangNhap.ToLower()))
                        {
                            MessageBox.Show("Tên đăng nhập này đã tồn tại trong hệ thống! Vui lòng chọn tên khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                        
                        string mk = string.IsNullOrWhiteSpace(MatKhau) ? "123456" : MatKhau;
                        db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = TenDangNhap, MatKhau = mk, Quyen = 2 });
                    }

                    db.NhanViens.Add(new NhanVien()
                    {
                        TenNV = TenNV,
                        SDT = SDT,
                        DiaChi = DiaChi,
                        TenDangNhap = string.IsNullOrWhiteSpace(TenDangNhap) ? null : TenDangNhap
                    });
                    db.SaveChanges(); // Tự động sinh MaNV
                    Refresh();
                }
            });

            // Cập nhật
            UpdateCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {
                using (var db = new QLXeMayEntities())
                {
                    var editItem = db.NhanViens.Find(SelectedItem.MaNV);
                    if (editItem != null)
                    {
                        if (!string.IsNullOrWhiteSpace(TenDangNhap))
                        {
                            // Nếu đổi tên đăng nhập khác
                            if (editItem.TenDangNhap != TenDangNhap)
                            {
                                if (db.TaiKhoans.Any(x => x.TenDangNhap.ToLower() == TenDangNhap.ToLower()))
                                {
                                    MessageBox.Show("Tên đăng nhập này đã tồn tại trong hệ thống! Vui lòng chọn tên khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                string mk = string.IsNullOrWhiteSpace(MatKhau) ? "123456" : MatKhau;
                                db.TaiKhoans.Add(new TaiKhoan { TenDangNhap = TenDangNhap, MatKhau = mk, Quyen = 2 });
                            }
                            else 
                            {
                                // Không đổi tên đăng nhập, chỉ đổi mật khẩu nếu có nhập
                                if (!string.IsNullOrWhiteSpace(MatKhau))
                                {
                                    var tk = db.TaiKhoans.Find(TenDangNhap);
                                    if (tk != null)
                                    {
                                        tk.MatKhau = MatKhau;
                                    }
                                }
                            }
                        }

                        editItem.TenNV = TenNV;
                        editItem.SDT = SDT;
                        editItem.DiaChi = DiaChi;
                        editItem.TenDangNhap = string.IsNullOrWhiteSpace(TenDangNhap) ? null : TenDangNhap;
                        db.SaveChanges();
                        Refresh();
                    }
                }
            });

            // Xóa (Đã sửa lại để lấy đối tượng từ SelectedItem)
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {

                // Kiểm tra chặn xóa tài khoản Admin (Tránh lỗi mất quyền đăng nhập hệ thống)
                if (SelectedItem.TenDangNhap == "admin")
                {
                    MessageBox.Show("Không được phép xóa tài khoản Quản trị viên hệ thống!", "Cảnh báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (MessageBox.Show("Bạn có chắc chắn muốn xóa nhân viên này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var db = new QLXeMayEntities())
                    {
                        var deleteItem = db.NhanViens.Find(SelectedItem.MaNV);
                        if (deleteItem != null)
                        {
                            db.NhanViens.Remove(deleteItem);
                            db.SaveChanges();
                            Refresh();
                        }
                    }
                }
            });
        }

        void LoadData()
        {
            using (var db = new QLXeMayEntities())
            {
                ListNhanVien = new ObservableCollection<NhanVien>(db.NhanViens.ToList());
            }
        }

        void Refresh() { TenNV = ""; SDT = ""; DiaChi = ""; TenDangNhap = ""; MatKhau = ""; SelectedItem = null; LoadData(); }
    }
}   