using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Microsoft.Win32;
using QLXeMay.Models;
using System.Data.Entity;
using System.Data.SqlClient;
using System.IO;

namespace QLXeMay.ViewModels.HeThong
{
    public class BackupRestoreViewModel : BaseViewModel
    {
        public ICommand BackupCommand { get; set; }
        public ICommand RestoreCommand { get; set; }

        public BackupRestoreViewModel()
        {
            BackupCommand = new RelayCommand<object>((p) => true, async (p) => await BackupDatabaseAsync());
            RestoreCommand = new RelayCommand<object>((p) => true, async (p) => await RestoreDatabaseAsync());
        }

        private async Task BackupDatabaseAsync()
        {
            try
            {
                var dialog = new SaveFileDialog
                {
                    Filter = "Backup files (*.bak)|*.bak",
                    FileName = "QLXeMay_Backup_" + DateTime.Now.ToString("ddMMyyyy_HHmmss") + ".bak",
                    Title = "Chọn nơi lưu bản sao lưu"
                };

                if (dialog.ShowDialog() == true)
                {
                    using (var db = new QLXeMayEntities())
                    {
                        string dbName = db.Database.Connection.Database;
                        string backupPath = dialog.FileName;

                        string sql = $@"BACKUP DATABASE [{dbName}] TO DISK = '{backupPath}' WITH FORMAT, MEDIANAME = 'QLXeMayBackup', NAME = 'Full Backup of QLXeMay';";
                        
                        await db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, sql);
                    }

                    DialogService.ShowSuccess("Đã sao lưu cơ sở dữ liệu thành công!", "Sao lưu hoàn tất");
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowError("Lỗi khi sao lưu dữ liệu: " + ex.Message, "Lỗi Sao Lưu");
            }
        }

        private async Task RestoreDatabaseAsync()
        {
            try
            {
                var dialog = new OpenFileDialog
                {
                    Filter = "Backup files (*.bak)|*.bak",
                    Title = "Chọn file để phục hồi"
                };

                if (dialog.ShowDialog() == true)
                {
                    string backupPath = dialog.FileName;

                    using (var db = new QLXeMayEntities())
                    {
                        string dbName = db.Database.Connection.Database;

                        // Đối với LocalDB, phục hồi trực tiếp có thể gây lỗi "File in use". 
                        // Cần chuyển sang SINGLE_USER, phục hồi, và đưa về MULTI_USER.
                        string sql = $@"
                            USE master;
                            ALTER DATABASE [{dbName}] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
                            RESTORE DATABASE [{dbName}] FROM DISK = '{backupPath}' WITH REPLACE;
                            ALTER DATABASE [{dbName}] SET MULTI_USER;
                        ";

                        // Ngắt tất cả các kết nối hiện hành trong connection pool để không bị block
                        SqlConnection.ClearAllPools();

                        await db.Database.ExecuteSqlCommandAsync(TransactionalBehavior.DoNotEnsureTransaction, sql);
                    }

                    DialogService.ShowSuccess("Đã phục hồi cơ sở dữ liệu thành công! Ứng dụng cần khởi động lại để áp dụng.", "Phục hồi hoàn tất");
                    System.Windows.Application.Current.Shutdown();
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowError("Lỗi khi phục hồi dữ liệu: " + ex.Message, "Lỗi Phục Hồi");
            }
        }
    }
}
