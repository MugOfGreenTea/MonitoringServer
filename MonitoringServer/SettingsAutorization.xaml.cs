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

namespace MonitoringServer
{
    /// <summary>
    /// Логика взаимодействия для SettingsAutorization.xaml
    /// </summary>
    public partial class SettingsAutorization : Window
    {
        Settings manager = new Settings(AppDomain.CurrentDomain.BaseDirectory + "MonitoringSetting.ini");
        string hostname, port, user, password, database, charset;

        public SettingsAutorization()
        {
            InitializeComponent();

            hostname = manager.GetPrivateString("CONNECTION", "ip_server");
            port = manager.GetPrivateString("CONNECTION", "port");
            user = manager.GetPrivateString("CONNECTION", "user");
            password = manager.GetPrivateString("CONNECTION", "password");
            database = manager.GetPrivateString("DATABASE_INFO", "database");
            charset = manager.GetPrivateString("DATABASE_INFO", "charset");

            FillSettings();
        }

        private void SaveSettingButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Вы уверены что хотите сохранить изменение?", "Сохранение", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                manager.WritePrivateString("CONNECTION", "ip_server", IPSettingTextBox.Text);
                manager.WritePrivateString("CONNECTION", "port", PortSettingTextBox.Text);
                manager.WritePrivateString("CONNECTION", "user", UserSettingTextBox.Text);
                manager.WritePrivateString("CONNECTION", "password", PassSettingTextBox.Text);
                manager.WritePrivateString("DATABASE_INFO", "database", NameDBSettingTextBox.Text);
                manager.WritePrivateString("DATABASE_INFO", "charset", CharsetSettingTextBox.Text);
                Close();
            }
            if (result == MessageBoxResult.No)
            {
                FillSettings();
            }
        }

        private void FillSettings()
        {
            IPSettingTextBox.Text = hostname;
            PortSettingTextBox.Text = port;
            UserSettingTextBox.Text = user;
            PassSettingTextBox.Text = password;
            NameDBSettingTextBox.Text = database;
            CharsetSettingTextBox.Text = charset;
        }
    }
}
