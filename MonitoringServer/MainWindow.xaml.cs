using Microsoft.Win32;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MonitoringServer
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        string UserName;
        Server server;

        ThreadStart tm;
        Thread myThread;

        Settings manager = new Settings(AppDomain.CurrentDomain.BaseDirectory + "MonitoringSetting.ini");

        DBConnection Connection, ConnectionTest;
        string hostname, port, user, password, database, charset, ip_server, port_server, login, pass;
        public DataTable Table;
        List<string> ListTable;
        Dictionary<string, string> ArrayTableNames;

        public string SelectionTable = "", RequestDeleteRow, RequestDeleteTable;
        string cellID = "", cellName = "";

        public MainWindow(string login)
        {
            InitializeComponent();
            server = new Server();

            UserName = login;

            Title += UserName;

            FillArrayTableNames();
            FillSettingDB();
            FillSettingServer();
            FillSettingLoginOptions();

            ConnectionDataBase();
        }
        // \/
        private void ServerStartStopButton_Click(object sender, RoutedEventArgs e)
        {
            ServerStartStop();
        }

        private void ServerStartStop()
        {
            if (server.ServerStatus == false)
            {
                tm = new ThreadStart(ServerStart);
                myThread = new Thread(tm);
                myThread.Start();

                ServerStartStopButton.Content = "Остановить";
                ServerStatusLabel.Content = "Запущен";
                ServerIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 240, 30));

                MenuItem_server_start.IsEnabled = false;
                MenuItem_server_stop.IsEnabled = true;
            }
            else if (server.ServerStatus == true)
            {
                server.ServerStop();
                ServerStartStopButton.Content = "Запустить";
                ServerStatusLabel.Content = "Остановлен";
                ServerIndicator.Fill = new SolidColorBrush(Color.FromRgb(230, 0, 0));

                MenuItem_server_start.IsEnabled = true;
                MenuItem_server_stop.IsEnabled = false;
            }
        }
        // \/
        private void ServerStart()
        {
            ip_server = manager.GetPrivateString("SERVER", "ip_server");
            port_server = manager.GetPrivateString("SERVER", "port");

            server.ServerStart(IPAddress.Parse(ip_server), Convert.ToInt32(port_server));
        }
        // \/
        private void ConnectionDataBase()
        {
            Connection = new DBConnection(hostname, port, user, password, database, charset);
            if (Connection.ConnectionCheck())
            {
                FillTablesListBox(Connection.RequestTables);
            }
            else
            {
                MessageBox.Show("Сервер отклонил запрос на подключение. Пожалуйста, проверьте правильность введенных данных", "Ошибка подключения!");
            }
        }
        // \/
        private void FillTablesListBox(string request)
        {
            TablesListBox.Items.Clear();

            ListTable = Connection.LoadReaderOne(Connection.RequestTables);

            foreach (string c in ListTable)
            {
                if (ArrayTableNames.ContainsKey(c))
                    TablesListBox.Items.Add(ArrayTableNames[c]);
                else
                    TablesListBox.Items.Add(c);
            }
        }
        // \/
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (server.ServerStatus == true)
            {
                server.ServerStop();
            }
        }
        // \/
        private void TableListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectTable();
        }

        public void SelectTable()
        {
            try
            {
                SelectionTable = FoundNameTable(ArrayTableNames, TablesListBox.SelectedItem.ToString());
                if (SelectionTable == "sensors" || SelectionTable == "users" || SelectionTable == "authority")
                {
                    Connection.QueryBuilding(SelectionTable);
                    Table = Connection.LoadTable(Connection.RequestSelectTable);
                    TableDataGrid.DataContext = Table;
                    TableDataGrid.Columns[0].Visibility = Visibility.Hidden;

                    InsertButton.Visibility = Visibility.Visible;
                    UpdateButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Visible;
                }
                else if (SelectionTable == "facilities")
                {
                    Connection.QueryBuilding(SelectionTable);
                    Table = Connection.LoadTable(Connection.RequestSelectTable);
                    TableDataGrid.DataContext = Table;

                    InsertButton.Visibility = Visibility.Visible;
                    UpdateButton.Visibility = Visibility.Visible;
                    DeleteButton.Visibility = Visibility.Visible;
                }
                else if (SelectionTable == "violation_reports" || SelectionTable == "manualinput")
                {
                    Connection.QueryBuilding(SelectionTable);
                    Table = Connection.LoadTable(Connection.RequestSelectTable);
                    TableDataGrid.DataContext = Table;
                    TableDataGrid.Columns[0].Visibility = Visibility.Hidden;

                    InsertButton.Visibility = Visibility.Hidden;
                    UpdateButton.Visibility = Visibility.Hidden;
                    DeleteButton.Visibility = Visibility.Hidden;
                }
                else
                {
                    SelectionTable = TablesListBox.SelectedItem.ToString();
                    Connection.QueryBuilding(SelectionTable);
                    Table = Connection.LoadTable(Connection.RequestSelectTable);
                    TableDataGrid.DataContext = Table;

                    InsertButton.Visibility = Visibility.Hidden;
                    UpdateButton.Visibility = Visibility.Hidden;
                    DeleteButton.Visibility = Visibility.Hidden;
                }
            }
            catch
            {
                SelectionTable = null;
            }
        }
        // \/
        private void TableDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                DataRowView row = (DataRowView)TableDataGrid.SelectedItems[0];
                cellID = row.Row.ItemArray[0].ToString();
                cellName = row.Row.ItemArray[1].ToString();
                Debug.WriteLine(cellID);
            }
            catch
            {
                cellID = null;
                cellName = null;
            }
        }
        // \/
        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow IW = new InsertWindow(SelectionTable, this, Connection);
            IW.ShowDialog();
        }
        // \/
        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            if (cellID != null)
            {
                UpdateWindow IW = new UpdateWindow(SelectionTable, cellID, this, Connection);
                IW.ShowDialog();
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите редактируемую строку.", "Ошибка");
            }
        }
        // \/
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (cellID != null)
            {
                MessageBoxResult result = MessageBox.Show(this, "Удалить выделенную строку?", "Удаление", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        if (SelectionTable == "sensors")
                        {
                            RequestDeleteRow = Connection.DeleteQueryBuilding(SelectionTable, cellID);
                            RequestDeleteTable = string.Format("DROP TABLE if exists `{0}`;", cellName);
                            Connection.ActionRequest(RequestDeleteRow);
                            Connection.ActionRequest(RequestDeleteTable);

                            Connection.QueryBuilding(SelectionTable);
                            Table = Connection.LoadTable(Connection.RequestSelectTable);
                            TableDataGrid.DataContext = Table;
                            TableDataGrid.Columns[0].Visibility = Visibility.Hidden;

                            MessageBox.Show(this, "Информация была удалена.", "Удаление");
                        }
                        else
                        {
                            RequestDeleteRow = Connection.DeleteQueryBuilding(SelectionTable, cellID);
                            Connection.ActionRequest(RequestDeleteRow);

                            Connection.QueryBuilding(SelectionTable);
                            Table = Connection.LoadTable(Connection.RequestSelectTable);
                            TableDataGrid.DataContext = Table;
                            TableDataGrid.Columns[0].Visibility = Visibility.Hidden;

                            MessageBox.Show(this, "Информация была удалена.", "Удаление");

                        }
                    }
                    catch
                    {
                        MessageBox.Show("Невозможно удалить.", "Ошибка");
                    }
                }
            }
            else
            {
                MessageBox.Show("Пожалуйста, выберите удаляемую строку.", "Ошибка");
            }
        }
        // \/
        private void SaveChangeSettingDBButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Вы уверены что хотите сохранить изменение?", "Сохранение", MessageBoxButton.YesNoCancel);

            if (result == MessageBoxResult.Yes)
            {
                ConnectionTest = new DBConnection(IPSettingTextBox.Text, PortSettingTextBox.Text, UserSettingTextBox.Text,
                        PassSettingTextBox.Text, NameDBSettingTextBox.Text, CharsetSettingTextBox.Text);

                if (ConnectionTest.ConnectionCheck())
                {
                    manager.WritePrivateString("CONNECTION", "ip_server", IPSettingTextBox.Text);
                    manager.WritePrivateString("CONNECTION", "port", PortSettingTextBox.Text);
                    manager.WritePrivateString("CONNECTION", "user", UserSettingTextBox.Text);
                    manager.WritePrivateString("CONNECTION", "password", PassSettingTextBox.Text);
                    manager.WritePrivateString("DATABASE_INFO", "database", NameDBSettingTextBox.Text);
                    manager.WritePrivateString("DATABASE_INFO", "charset", CharsetSettingTextBox.Text);

                    FillTablesListBox(Connection.RequestTables);
                    Connection = ConnectionTest;
                }
                else
                {
                    MessageBox.Show("Сервер отклонил запрос на подключение. Пожалуйста, проверьте правильность введенных данных", "Ошибка подключения!");
                    FillSettingDB();
                }
            }

            if (result == MessageBoxResult.No)
            {
                FillSettingServer();
            }
        }
        // \/
        private void SaveChangeSettingServerButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Вы уверены что хотите сохранить изменение? Внимание сервер будет остановлен.", "Сохранение", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                bool temp_status = server.ServerStatus;
                if (temp_status == true)
                    server.ServerStop();
                manager.WritePrivateString("SERVER", "ip_server", IPSettingServerTextBox.Text);
                manager.WritePrivateString("SERVER", "port", PortSettingServerTextBox.Text);
            }
            if (result == MessageBoxResult.No)
            {
                FillSettingServer();
            }
        }

        private void SaveChangeLoginOptionsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show(this, "Вы уверены что хотите сохранить изменение? Внимание сервер будет перезагружен.", "Сохранение", MessageBoxButton.YesNoCancel);
            if (result == MessageBoxResult.Yes)
            {
                manager.WritePrivateString("LOGIN_OPTIONS", "login", LoginLoginOptionsTextBox.Text);
                manager.WritePrivateString("LOGIN_OPTIONS", "password", PassLoginOptionsTextBox.Text);
            }
            if (result == MessageBoxResult.No)
            {
                FillSettingLoginOptions();
            }

        }
        // \/
        private void FillSettingDB()
        {
            hostname = manager.GetPrivateString("CONNECTION", "ip_server");
            port = manager.GetPrivateString("CONNECTION", "port");
            user = manager.GetPrivateString("CONNECTION", "user");
            password = manager.GetPrivateString("CONNECTION", "password");
            database = manager.GetPrivateString("DATABASE_INFO", "database");
            charset = manager.GetPrivateString("DATABASE_INFO", "charset");

            IPSettingTextBox.Text = hostname;
            PortSettingTextBox.Text = port;
            UserSettingTextBox.Text = user;
            PassSettingTextBox.Text = password;
            NameDBSettingTextBox.Text = database;
            CharsetSettingTextBox.Text = charset;

            IPLabel.Content = hostname;
            PortLabel.Content = port;
            UsernameLabel.Content = user;
        }
        // \/
        private void FillSettingServer()
        {
            ip_server = manager.GetPrivateString("SERVER", "ip_server");
            port_server = manager.GetPrivateString("SERVER", "port");

            IPSettingServerTextBox.Text = ip_server;
            PortSettingServerTextBox.Text = port_server;
        }
        // \/
        private void FillSettingLoginOptions()
        {
            login = manager.GetPrivateString("LOGIN_OPTIONS", "login");
            pass = manager.GetPrivateString("LOGIN_OPTIONS", "password");

            LoginLoginOptionsTextBox.Text = login;
            PassLoginOptionsTextBox.Text = pass;

        }
        // \/
        private void MenuItem_choice_user_Click(object sender, RoutedEventArgs e)
        {
            Autorization A = new Autorization();
            A.Show();
            this.Close();
        }
        // \/
        private void MenuItem_Quit_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        // \/
        private void RefreshTableButton_Click(object sender, RoutedEventArgs e)
        {
            string temp_table = TablesListBox.SelectedItem.ToString();
            TableDataGrid.SelectedIndex = -1;
            TablesListBox.SelectedIndex = -1;
            TablesListBox.Items.Clear();
            ConnectionDataBase();
            TablesListBox.SelectedItem = temp_table;
        }
        // \/
        private void MenuItem_copy_db_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Резервная копия БД (.sql)|*.sql";
            dlg.FileName = "backup";
            dlg.Title = "Сохранить";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        try
                        {
                            cmd.Connection = Connection.Connection;
                            Connection.Connection.Open();
                            mb.ExportToFile(filename);
                            Connection.Connection.Close();
                            MessageBox.Show("Резервная копия успешно сохранена.", "Сохранение");
                        }
                        catch
                        {
                            MessageBox.Show("Неудалось сохранить файл. Попробуйте снова.", "Ошибка сохранения");
                        }
                        finally
                        {
                            if (Connection.Connection.State != ConnectionState.Closed)
                            {
                                Connection.Connection.Close();
                            }
                        }
                    }
                }
            }
        }
        // \/
        private void MenuItem_paste_db_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Резервная копия БД (.sql)|*.sql";
            dlg.Title = "Открыть";
            if (dlg.ShowDialog() == true)
            {
                string filename = dlg.FileName;

                using (MySqlCommand cmd = new MySqlCommand())
                {
                    using (MySqlBackup mb = new MySqlBackup(cmd))
                    {
                        try
                        {
                            cmd.Connection = Connection.Connection;
                            Connection.Connection.Open();
                            mb.ImportFromFile(filename);
                            Connection.Connection.Close();
                            MessageBox.Show("Восстановление прошло успешно.", "Восстановление");
                        }
                        catch
                        {
                            MessageBox.Show("Неудалось востанновить БД. Попробуйте снова.", "Ошибка восстановления");
                        }
                        finally
                        {
                            if (Connection.Connection.State != ConnectionState.Closed)
                            {
                                Connection.Connection.Close();
                            }
                        }
                    }
                }
            }
        }
        // \/
        private void MenuItem_server_start_Click(object sender, RoutedEventArgs e)
        {
            if (server.ServerStatus == false)
            {
                tm = new ThreadStart(ServerStart);
                myThread = new Thread(tm);
                myThread.Start();

                ServerStartStopButton.Content = "Остановить";
                ServerStatusLabel.Content = "Запущен";
                ServerIndicator.Fill = new SolidColorBrush(Color.FromRgb(0, 240, 30));

                MenuItem_server_start.IsEnabled = false;
                MenuItem_server_stop.IsEnabled = true;
            }
        }
        // \/
        private void MenuItem_server_stop_Click(object sender, RoutedEventArgs e)
        {
            if (server.ServerStatus == true)
            {
                server.ServerStop();
                ServerStartStopButton.Content = "Запустить";
                ServerStatusLabel.Content = "Остановлен";
                ServerIndicator.Fill = new SolidColorBrush(Color.FromRgb(230, 0, 0));

                MenuItem_server_start.IsEnabled = true;
                MenuItem_server_stop.IsEnabled = false;
            }
        }
        // \/
        private void MenuItem_server_settings_Click(object sender, RoutedEventArgs e)
        {
            SettingsTabItem.IsSelected = true;
        }
        // \/
        private void MenuItem_server_table_Click(object sender, RoutedEventArgs e)
        {
            TableTabItem.IsSelected = true;
        }
        // \/
        private void MenuItem_add_upd_user_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow IW = new InsertWindow("users", this, Connection);
            IW.ShowDialog();
        }
        // \/
        private void MenuItem_add_upd_autority_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow IW = new InsertWindow("authority", this, Connection);
            IW.ShowDialog();
        }
        // \/
        private void MenuItem_add_upd_falicity_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow IW = new InsertWindow("facilities", this, Connection);
            IW.ShowDialog();
        }
        // \/
        private void MenuItem_add_upd_sensor_Click(object sender, RoutedEventArgs e)
        {
            InsertWindow IW = new InsertWindow("sensors", this, Connection);
            IW.ShowDialog();
        }
        // \/

        private void FillArrayTableNames()
        {
            ArrayTableNames = new Dictionary<string, string>();
            ArrayTableNames["sensors"] = "Датчики";
            ArrayTableNames["users"] = "Пользователи";
            ArrayTableNames["manualinput"] = "Ручной ввод";
            ArrayTableNames["facilities"] = "Установки";
            ArrayTableNames["authority"] = "Полномочия";
            ArrayTableNames["violation_reports"] = "Отчеты о нарушениях";
        }

        private string FoundNameTable(Dictionary<string, string> array, string value)
        {
            return array.FirstOrDefault(x => x.Value == value).Key;
        }
    }
}
