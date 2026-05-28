using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLXeMay.Models;

namespace QLXeMay.ViewModels.DanhMuc
{
    public class KhachHangViewModel : BaseViewModel
    {
        private ObservableCollection<KhachHang> _listKhachHang;
        public ObservableCollection<KhachHang> ListKhachHang { get => _listKhachHang; set { _listKhachHang = value; OnPropertyChanged(); } }

        private string _tenKH; public string TenKH { get => _tenKH; set { _tenKH = value; OnPropertyChanged(); } }
        private string _sdt; public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(); } }
        private string _diaChi; public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        private KhachHang _selectedItem;
        public KhachHang SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenKH = SelectedItem.TenKH;
                    SDT = SelectedItem.SDT;
                    DiaChi = SelectedItem.DiaChi;
                }
            }
        }

        public RelayCommand<object> AddCommand { get; set; }
        public RelayCommand<object> UpdateCommand { get; set; }
        public RelayCommand<object> DeleteCommand { get; set; }

        public KhachHangViewModel()
        {
            LoadData();

            // THÊM MỚI
            AddCommand = new RelayCommand<object>((p) => !string.IsNullOrWhiteSpace(TenKH), (p) => {
                using (var db = new QLXeMayEntities())
                {
                    db.KhachHangs.Add(new KhachHang() { TenKH = TenKH, SDT = SDT, DiaChi = DiaChi });
                    db.SaveChanges();
                    Refresh();
                }
            });

            // SỬA / CẬP NHẬT
            UpdateCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {
                using (var db = new QLXeMayEntities())
                {
                    var editItem = db.KhachHangs.Find(SelectedItem.MaKH);
                    if (editItem != null)
                    {
                        editItem.TenKH = TenKH;
                        editItem.SDT = SDT;
                        editItem.DiaChi = DiaChi;
                        db.SaveChanges();
                        Refresh();
                        MessageBox.Show("Cập nhật thông tin thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            });

            // XÓA KHÁCH HÀNG (Đã được cập nhật để lấy từ SelectedItem)
            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa khách hàng này?", "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    using (var db = new QLXeMayEntities())
                    {
                        // Tìm dựa trên đối tượng đang được chọn trên bảng
                        var deleteItem = db.KhachHangs.Find(SelectedItem.MaKH);
                        if (deleteItem != null)
                        {
                            db.KhachHangs.Remove(deleteItem);
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
                ListKhachHang = new ObservableCollection<KhachHang>(db.KhachHangs.ToList());
            }
        }

        void Refresh() { TenKH = ""; SDT = ""; DiaChi = ""; SelectedItem = null; LoadData(); }
    }
}