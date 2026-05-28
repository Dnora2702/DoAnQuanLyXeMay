using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using QLXeMay.Models;

namespace QLXeMay.ViewModels.DanhMuc
{
    public class NhaCungCapViewModel : BaseViewModel
    {
        private ObservableCollection<NhaCungCap> _listNhaCungCap;
        public ObservableCollection<NhaCungCap> ListNhaCungCap { get => _listNhaCungCap; set { _listNhaCungCap = value; OnPropertyChanged(); } }

        private string _tenNCC;
        public string TenNCC { get => _tenNCC; set { _tenNCC = value; OnPropertyChanged(); } }

        private string _sdt;
        public string SDT { get => _sdt; set { _sdt = value; OnPropertyChanged(); } }

        private string _diaChi;
        public string DiaChi { get => _diaChi; set { _diaChi = value; OnPropertyChanged(); } }

        private NhaCungCap _selectedItem;
        public NhaCungCap SelectedItem
        {
            get => _selectedItem;
            set
            {
                _selectedItem = value;
                OnPropertyChanged();
                if (SelectedItem != null)
                {
                    TenNCC = SelectedItem.TenNCC;
                    SDT = SelectedItem.SDT;
                    DiaChi = SelectedItem.DiaChi;
                }
            }
        }

        public RelayCommand<object> AddCommand { get; set; }
        public RelayCommand<object> UpdateCommand { get; set; }
        public RelayCommand<object> DeleteCommand { get; set; }

        public NhaCungCapViewModel()
        {
            LoadData();

            AddCommand = new RelayCommand<object>((p) => !string.IsNullOrEmpty(TenNCC), (p) => {
                using (var db = new QLXeMayEntities())
                {
                    db.NhaCungCaps.Add(new NhaCungCap() { 
                        TenNCC = TenNCC,
                        SDT = SDT,
                        DiaChi = DiaChi
                    });
                    db.SaveChanges();
                    Refresh();
                }
            });

            UpdateCommand = new RelayCommand<object>((p) => SelectedItem != null && !string.IsNullOrEmpty(TenNCC), (p) => {
                using (var db = new QLXeMayEntities())
                {
                    var editItem = db.NhaCungCaps.Find(SelectedItem.MaNCC);
                    if (editItem != null)
                    {
                        editItem.TenNCC = TenNCC;
                        editItem.SDT = SDT;
                        editItem.DiaChi = DiaChi;
                        db.SaveChanges();
                        Refresh();
                    }
                }
            });

            DeleteCommand = new RelayCommand<object>((p) => SelectedItem != null, (p) => {
                if (MessageBox.Show("Bạn có chắc chắn muốn xóa Nhà cung cấp này?", "Xác nhận", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                {
                    using (var db = new QLXeMayEntities())
                    {
                        var deleteItem = db.NhaCungCaps.Find(SelectedItem.MaNCC);
                        if (deleteItem != null)
                        {
                            db.NhaCungCaps.Remove(deleteItem);
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
                ListNhaCungCap = new ObservableCollection<NhaCungCap>(db.NhaCungCaps.ToList());
            }
        }

        void Refresh() 
        { 
            TenNCC = ""; 
            SDT = ""; 
            DiaChi = ""; 
            SelectedItem = null; 
            LoadData(); 
        }
    }
}
