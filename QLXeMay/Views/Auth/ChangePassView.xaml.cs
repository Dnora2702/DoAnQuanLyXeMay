using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QLXeMay.ViewModels.Auth;

namespace QLXeMay.Views.Auth
{
    /// <summary>
    /// Interaction logic for ChangePassView.xaml
    /// </summary>
    public partial class ChangePassView : Window
    {
        public ChangePassView()
        {
            InitializeComponent();
        }
        private void BtnConfirm_Click(object sender, RoutedEventArgs e)
        {
            var vm = DataContext as ChangePassViewModel;
            vm?.ChangePass(txtOldPass, txtNewPass, txtConfirmPass);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);
            DragMove();
        }
    }
}
