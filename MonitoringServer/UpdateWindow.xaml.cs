using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
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
    /// Логика взаимодействия для UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        string NameInsertTable, RequestFillComboBox1, RequestFillComboBox2, RequestLoadingWithDB,
            RequestUpdate, AdditionalRequest, RequestCreateSensor = null, RequestTriger = null, DropTrigger, ID;
        DBConnection Connection; // = new DBConnection("localhost", "3308", "root", "123456", "test");
        MainWindow mainWindow;
        DataTable Table;
        bool UpdateSolution;
        string tb1, tb2, tb3, tb4, tb5, cb1, cb2, cb3, cb6, OldSensorNameTrigger;

        public UpdateWindow(string name_table, string id, MainWindow window, DBConnection connection)
        {
            InitializeComponent();

            NameInsertTable = name_table;
            mainWindow = window;
            ID = id;
            Connection = connection;
            ChouseTable();
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            switch (NameInsertTable)
            {
                case "sensors":
                    tb1 = TextBox1.Text.ToLower();
                    tb2 = TextBox2.Text;
                    tb3 = TextBox3.Text;
                    tb4 = TextBox4.Text;
                    tb5 = TextBox5.Text;
                    cb6 = ComboBox6.Text;

                    if (TextBox1.Text != "" && TextBox2.Text != "" && TextBox3.Text != "" && TextBox4.Text != "" && ComboBox6.Text != "")
                    {
                        if (IsDigit(TextBox2.Text) && IsDigit(TextBox3.Text))
                        {
                            if (Convert.ToInt32(TextBox2.Text) > Convert.ToInt32(TextBox3.Text))
                            {
                                UpdateSolution = true;

                                RequestUpdate = string.Format("UPDATE `sensors` SET `name_sensor` = '{0}', " +
                                "`max_value`= '{1}', `min_value` = '{2}', `unit_sensor` = '{3}', `description_sensor` = '{4}', ",
                                tb1, tb2, tb3, tb4, tb5);
                                AdditionalRequest = string.Format("select `id_facility` from facilities where `name_facility` like '{0}';", cb6);

                                if (OldSensorNameTrigger != tb1 && OldSensorNameTrigger != tb1.ToLower())
                                {
                                    RequestCreateSensor = string.Format("RENAME TABLE  `{0}` TO  `{1}`", OldSensorNameTrigger.ToLower(), tb1);

                                    DropTrigger = string.Format("DROP TRIGGER IF EXISTS `test`.`{0}_after_insert`;", OldSensorNameTrigger.ToLower());

                                    RequestTriger = string.Format("DELIMITER // " +
                                        "CREATE DEFINER=`root`@`localhost` TRIGGER `{0}_after_insert` " +
                                        " AFTER INSERT ON `{0}` FOR EACH ROW BEGIN " +
                                        " set @name_sensor = \"{0}\"; " +
                                        " set @max_value = (SELECT max_value FROM sensors where name_sensor = @name_sensor); " +
                                        " set @min_value = (SELECT min_value FROM sensors where name_sensor = @name_sensor); " +
                                        " if new.`value` > @max_value or new.`value` < @min_value then " +
                                        " INSERT INTO `test`.`violation_reports` (`id_report`, `id_sensor`, `datetime`, `value`) " +
                                        " VALUES(null, (select id_sensor from sensors where name_sensor = @name_sensor), new.`datetime`, new.`value`);" +
                                        " end if;" +
                                        " END" +
                                        " //", tb1);
                                }
                            }
                            else
                            {
                                UpdateSolution = false;
                                MessageBox.Show("Максимальное значение не может быть больше Минимального значения.", "Ошибка ввода!");
                            }
                        }
                        else
                        {
                            UpdateSolution = false;
                            MessageBox.Show("Некорректно заполнены числовые поля (Максимальное значение, Минимальное значение).", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        UpdateSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }

                    try
                    {
                        RequestUpdate += " `id_facility` = '" + Connection.RequestOneObject(AdditionalRequest) + "' ";
                        RequestUpdate += string.Format(" WHERE (`id_sensor` = '{0}'); ", ID);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.ToString(), "Ошибка!");
                    }

                    break;

                case "users":
                    if (TextBox1.Text != "" && TextBox2.Text != "")
                    {
                        if (TextBox1.Text.Length > 3 && TextBox1.Text.Length < 17)
                        {
                            UpdateSolution = true;
                            RequestUpdate = string.Format("UPDATE `users` SET `login` = '{0}', `pass` = '{1}' " +
                                "where id = '{2}';",
                            TextBox1.Text, TextBox2.Text, ID);
                        }
                        else
                        {
                            UpdateSolution = false;
                            MessageBox.Show("Логин может содержать только от 4 до 30 символов.", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        UpdateSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }
                    break;

                case "manualinput":
                    if (TextBox1.Text != "" && TextBox2.Text != "" && TextBox3.Text != "" && TextBox4.Text != "")
                    {
                        UpdateSolution = true;

                        RequestUpdate = string.Format("UPDATE `manualinput` SET `id_sensor` = '{0}', `id_user` = '{1}', " +
                            "`datetime` = '{2}', `value` = '{3}' where `id_input` = '{4}' ",
                            TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text, ID);
                    }
                    else
                    {
                        UpdateSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }

                    break;

                case "facilities":
                    if (TextBox1.Text != "" && TextBox2.Text != "")
                    {
                        if (TextBox2.Text.Length > 3 && TextBox2.Text.Length < 50 && TextBox1.Text.Length > 3 && TextBox1.Text.Length < 30)
                        {
                            UpdateSolution = true;
                            RequestUpdate = string.Format("UPDATE `facilities` SET `id_facility` = '{0}', `name_facility` = '{1}', " +
                                "`description_facility` = '{2}' where `id_facility` = '{3}';",
                                TextBox1.Text, TextBox2.Text, TextBox5.Text, ID);
                        }
                        else
                        {
                            UpdateSolution = false;
                            MessageBox.Show("Шифр установки может содержать только от 4 до 30 символов.\n Наименование установки может содержать только от 4 до 50 символов.", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        UpdateSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }
                    break;

                case "authority":
                    if (ComboBox1.Text != "" && ComboBox2.Text != "")
                    {
                        UpdateSolution = true;
                        RequestUpdate = string.Format("UPDATE `authority` " +
                            "SET `id_user` = (select id from users where login = '{0}'), " +
                            "`id_sensor` = (select id_sensor from sensors where name_sensor = '{1}')  " +
                            "where `id_authority` = '{2}';",
                            ComboBox2.Text, ComboBox1.Text, ID);
                    }
                    else
                    {
                        UpdateSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }
                    break;
            }

            if (UpdateSolution)
            {
                try
                {
                    Connection.ActionRequest(RequestUpdate);

                    if (RequestTriger != null && RequestCreateSensor != null)
                    {
                        Connection.ActionRequest(RequestCreateSensor);
                        Connection.ActionRequest(DropTrigger);
                        Connection.ScriptRequest(RequestTriger);
                    }

                    RequestTriger = null;
                    RequestCreateSensor = null;

                    mainWindow.SelectTable();

                    MessageBox.Show("Редактирование прошло успешно", "Редактирование");

                    ClearField();
                }
                catch (MySql.Data.MySqlClient.MySqlException ex) when (ex.Number == 1062)
                {
                    MessageBox.Show("Нельзя добавить дублирующую запись.", "Ошибка!");
                    if (Connection.Connection.State != ConnectionState.Closed)
                    {
                        Connection.Connection.Close();
                    }
                }
                catch
                {
                    MessageBox.Show("Неизвестная ошибка.", "Ошибка!");
                    if (Connection.Connection.State != ConnectionState.Closed)
                    {
                        Connection.Connection.Close();
                    }
                }
            }
        }

        private void ClearField()
        {
            TextBox1.Clear();
            TextBox2.Clear();
            TextBox3.Clear();
            TextBox4.Clear();
            TextBox5.Clear();
            ComboBox1.Text = "";
            ComboBox2.Text = "";
            ComboBox6.Text = "";
        }

        private void ChouseTable()
        {
            switch (NameInsertTable)
            {
                case "sensors":
                    Height = 344.777;
                    Width = 419.318;

                    Label1.Content = "Наименование сенсора:";
                    Label2.Content = "Максимальное значение:";
                    Label3.Content = "Минимальное значение:";
                    Label4.Content = "Единица измерения:";
                    Label5.Content = "Описание:";
                    Label6.Content = "Установка:";

                    Label1.Visibility = Visibility.Visible;
                    Label2.Visibility = Visibility.Visible;
                    Label3.Visibility = Visibility.Visible;
                    Label4.Visibility = Visibility.Visible;
                    Label5.Visibility = Visibility.Visible;
                    Label6.Visibility = Visibility.Visible;

                    TextBox1.Visibility = Visibility.Visible;
                    TextBox2.Visibility = Visibility.Visible;
                    TextBox3.Visibility = Visibility.Visible;
                    TextBox4.Visibility = Visibility.Visible;
                    TextBox5.Visibility = Visibility.Visible;
                    ComboBox6.Visibility = Visibility.Visible;

                    RequestFillComboBox1 = "SELECT `name_facility` FROM `facilities`";
                    RequestLoadingWithDB = "SELECT name_sensor, max_value, min_value, unit_sensor, name_facility, description_sensor " +
                        "FROM sensors, facilities where sensors.id_facility = facilities.id_facility and id_sensor = " + ID;

                    FillComboBox(RequestFillComboBox1, ComboBox6);

                    Table = Connection.LoadTable(RequestLoadingWithDB);

                    TextBox1.Text = Table.Rows[0][0].ToString();
                    TextBox2.Text = Table.Rows[0][1].ToString();
                    TextBox3.Text = Table.Rows[0][2].ToString();
                    TextBox4.Text = Table.Rows[0][3].ToString();
                    TextBox5.Text = Table.Rows[0][5].ToString();
                    ComboBox6.Text = Table.Rows[0][4].ToString();
                    OldSensorNameTrigger = Table.Rows[0][0].ToString();
                    break;
                case "users":
                    Height = 318.377;
                    Width = 216.118;

                    Label1.Content = "Логин пользователя:";
                    Label2.Content = "Пароль пользователя:";

                    Label1.Visibility = Visibility.Visible;
                    Label2.Visibility = Visibility.Visible;

                    TextBox1.Visibility = Visibility.Visible;
                    TextBox2.Visibility = Visibility.Visible;

                    RequestLoadingWithDB = "SELECT login, pass FROM users where id = " + ID;

                    Table = Connection.LoadTable(RequestLoadingWithDB);

                    TextBox1.Text = Table.Rows[0][0].ToString();
                    TextBox2.Text = Table.Rows[0][1].ToString();

                    break;
                case "manualinput":
                    Height = 390.377;
                    Width = 216.118;

                    Label1.Content = "Датчик:";
                    Label2.Content = "Пользователь:";
                    Label3.Content = "Дата и время:";
                    Label4.Content = "Значение:";

                    Label1.Visibility = Visibility.Visible;
                    Label2.Visibility = Visibility.Visible;
                    Label3.Visibility = Visibility.Visible;
                    Label4.Visibility = Visibility.Visible;

                    TextBox1.Visibility = Visibility.Visible;
                    TextBox2.Visibility = Visibility.Visible;
                    TextBox3.Visibility = Visibility.Visible;
                    TextBox4.Visibility = Visibility.Visible;

                    RequestLoadingWithDB = "SELECT id_sensor, id_user, `datetime`, `value` FROM manualinput where id_input = " + ID;

                    Table = Connection.LoadTable(RequestLoadingWithDB);

                    TextBox1.Text = Table.Rows[0][0].ToString();
                    TextBox2.Text = Table.Rows[0][1].ToString();
                    TextBox3.Text = Table.Rows[0][2].ToString();
                    TextBox4.Text = Table.Rows[0][3].ToString();
                    break;
                case "facilities":
                    Height = 242.377;
                    Width = 419.318;

                    Label1.Content = "Шифр установки:";
                    Label2.Content = "Наименование:";
                    Label5.Content = "Описание:";

                    Label1.Visibility = Visibility.Visible;
                    Label2.Visibility = Visibility.Visible;
                    Label5.Visibility = Visibility.Visible;

                    TextBox1.Visibility = Visibility.Visible;
                    TextBox2.Visibility = Visibility.Visible;
                    TextBox5.Visibility = Visibility.Visible;

                    RequestLoadingWithDB = "SELECT id_facility, name_facility, description_facility FROM facilities where id_facility = '" + ID + "';";

                    Table = Connection.LoadTable(RequestLoadingWithDB);

                    TextBox1.Text = Table.Rows[0][0].ToString();
                    TextBox2.Text = Table.Rows[0][1].ToString();
                    TextBox5.Text = Table.Rows[0][2].ToString();

                    break;
                case "authority":
                    Height = 242.377;
                    Width = 216.118;

                    Label1.Content = "Датчик:";
                    Label2.Content = "Пользователь:";

                    Label1.Visibility = Visibility.Visible;
                    Label2.Visibility = Visibility.Visible;

                    ComboBox1.Visibility = Visibility.Visible;
                    ComboBox2.Visibility = Visibility.Visible;

                    RequestFillComboBox1 = "SELECT `name_sensor` FROM `sensors`";
                    RequestFillComboBox2 = "SELECT `login` FROM `users` ";

                    FillComboBox(RequestFillComboBox1, ComboBox1);
                    FillComboBox(RequestFillComboBox2, ComboBox2);

                    RequestLoadingWithDB = "SELECT name_sensor, login FROM authority, users, sensors " +
                        "where authority.id_sensor = sensors.id_sensor and authority.id_user = users.id and id_authority = " + ID;

                    Table = Connection.LoadTable(RequestLoadingWithDB);

                    ComboBox1.Text = Table.Rows[0][0].ToString();
                    ComboBox2.Text = Table.Rows[0][1].ToString();

                    break;
            }
        }

        private void FillComboBox(string Request, ComboBox comboBox)
        {
            List<string> ComboBoxList = Connection.LoadReaderOne(Request);

            foreach (string c in ComboBoxList)
            {
                comboBox.Items.Add(c);
            }
        }

        private bool IsDigit(string str)
        {
            return str.Length == str.Where(c => char.IsDigit(c)).Count();
        }
    }
}
