using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLXeMay.Models;
namespace QLXeMay.ViewModels.DanhMuc
{
    public class HangXeViewModel : BaseViewModel
    {
        private ObservableCollection<HangXe> _listHangXe;
        public ObservableCollection<HangXe> ListHangXe { get => _listHangXe; set { _listHangXe = value; OnPropertyChanged(); } }

        private string _tenHang;
        public string TenHang { get => _tenHang; set { _tenHang = value; OnPropertyChanged(); } }

        private HangXe _selectedItem;
        public HangXe SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null) TenHang = SelectedItem.TenHang;
            }
        }

        public RelayCommand<object> AddCommand { get; set; }
        public RelayCommand<object> UpdateCommand { get; set; }
        public RelayCommand<object> DeleteCommand { get; set; }

        public HangXeViewModel()
        {
            LoadData();

            // Thêm mới
            AddCommand = new RelayCommand<object>((p) => !string.IsNullOrEmpty(TenHang), (p) => {
                using (var db = new QLXeMayEntities())
                {
                    db.HangXes.Add(new HangXe() { TenHang = TenHang });
                    db.SaveChanges(); // SQL tự sinh mã nhờ IDENTITY(1,1)
                    Refresh();
                }
            });

            // Cập nhật
            UpdateCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {
                using (var db = new QLXeMayEntities())
                {
                    var editItem = db.HangXes.Find(SelectedItem.MaHang);
                    editItem.TenHang = TenHang;
                    db.SaveChanges();
                    Refresh();
                }
            });

            // Xóa
            DeleteCommand = new RelayCommand<object>((p) => true, (p) => {
                if (MessageBox.Show("Xóa hãng này sẽ ảnh hưởng dữ liệu xe. Bạn chắc chắn?", "Cảnh báo", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    var item = p as HangXe;
                    using (var db = new QLXeMayEntities())
                    {
                        var deleteItem = db.HangXes.Find(item.MaHang);
                        db.HangXes.Remove(deleteItem);
                        db.SaveChanges();
                        Refresh();
                    }
                }
            });
        }

        void LoadData()
        {
            using (var db = new QLXeMayEntities())
            {
                ListHangXe = new ObservableCollection<HangXe>(db.HangXes.ToList());
            }
        }

        void Refresh() { TenHang = ""; SelectedItem = null; LoadData(); }
    }
}
