using MySql.Data.MySqlClient;
using System;
using System.Data;
using System.Windows;
using System.Windows.Media;

namespace MonitoringServer
{
    /// <summary>
    /// Логика взаимодействия для Autorization.xaml
    /// </summary>
    public partial class Autorization : Window
    {
        Settings manager = new Settings(AppDomain.CurrentDomain.BaseDirectory + "MonitoringSetting.ini");

        DBConnection Connection;
        DataTable Table;
        string hostname, port, user, password, database, charset, login, pass;
        bool CheckConn;

        public Autorization()
        {
            InitializeComponent();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            SettingsAutorization SA = new SettingsAutorization();
            SA.ShowDialog();
        }

        public void SetSettings()
        {
            hostname = manager.GetPrivateString("CONNECTION", "ip_server");
            port = manager.GetPrivateString("CONNECTION", "port");
            user = manager.GetPrivateString("CONNECTION", "user");
            password = manager.GetPrivateString("CONNECTION", "password");
            database = manager.GetPrivateString("DATABASE_INFO", "database");
            charset = manager.GetPrivateString("DATABASE_INFO", "charset");
            login = manager.GetPrivateString("LOGIN_OPTIONS", "login");
            pass = manager.GetPrivateString("LOGIN_OPTIONS", "password");
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

            SetSettings();
            Connection = new DBConnection(hostname, port, user, password, database, charset);

            try
            {
                if (TextBoxLogin.Text == login && TextBoxPass.Password == pass && Connection.ConnectionCheck())
                {
                    LabelInfoConn.Foreground = Brushes.Green;
                    LabelInfoConn.Content = "Вход выполнен";
                    MainWindow mw = new MainWindow(TextBoxLogin.Text);
                    mw.Show();
                    Close();
                }
                else if (TextBoxLogin.Text != login || TextBoxPass.Password != pass || (TextBoxLogin.Text != login && TextBoxPass.Password != pass))
                {
                    LabelInfoConn.Foreground = Brushes.Red;
                    LabelInfoConn.Content = "Логин или пароль введены неправильно";
                }
                else if (!Connection.ConnectionCheck())
                {
                    MessageBox.Show("Кажется что-то не так с базой данных. Пожалуйста, проверьте настройки или обратитесь к администратору.", "Упс!");
                }
            }
            catch (NullReferenceException)
            {
                LabelInfoConn.Foreground = Brushes.Red;
                LabelInfoConn.Content = "Логин или пароль введены неправильно";
            }
            catch (MySqlException)
            {
                MessageBox.Show("Кажется что-то не так с базой данных. Пожалуйста, проверьте настройки или обратитесь к администратору.", "Упс!");
            }
            catch
            {

            }
        }
    }
}
