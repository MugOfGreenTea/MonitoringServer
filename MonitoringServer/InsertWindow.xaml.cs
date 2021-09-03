using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
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
    /// Логика взаимодействия для DBEditingWindow.xaml
    /// </summary>
    public partial class InsertWindow : Window
    {
        string NameInsertTable, RequestFillComboBox1, RequestFillComboBox2, RequestInsert, RequestCreateSensor = null, RequestTriger = null;
        DBConnection Connection;
        MainWindow mainWindow;
        bool InsertSolution;

        public InsertWindow(string name_table, MainWindow window, DBConnection connection)
        {
            InitializeComponent();

            NameInsertTable = name_table;
            mainWindow = window;
            Connection = connection;

            ChouseTable();
        }

        private void InsertButton_Click(object sender, RoutedEventArgs e)
        {
            switch (NameInsertTable)
            {
                case "sensors":
                    RequestInsert = string.Format("INSERT INTO `sensors` (`id_sensor`, `name_sensor`, `max_value`, `min_value`, " +
                        "`unit_sensor`, `id_facility`, `description_sensor`) VALUES " +
                        "(null, '{0}', '{1}', '{2}', '{3}', (select id_facility from facilities where name_facility = '{4}'), '{5}');",
                        TextBox1.Text.ToLower(), TextBox2.Text, TextBox3.Text, TextBox4.Text, ComboBox6.Text, TextBox5.Text);

                    RequestCreateSensor = string.Format("CREATE TABLE `{0}` ( " +
                        " `datetime` DATETIME NULL DEFAULT NULL, `value` DOUBLE NULL DEFAULT NULL ) " +
                        " COLLATE = 'utf8_unicode_ci' ENGINE = InnoDB;", TextBox1.Text.ToLower());

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
                        " //", TextBox1.Text.ToLower());

                    if (TextBox1.Text != "" && TextBox2.Text != "" && TextBox3.Text != "" && TextBox4.Text != "" && ComboBox6.Text != "")
                    {
                        if (IsDigit(TextBox2.Text) && IsDigit(TextBox3.Text))
                        {
                            if( Convert.ToInt32(TextBox2.Text) > Convert.ToInt32(TextBox3.Text) )
                                InsertSolution = true;
                            else
                            {
                                InsertSolution = false;
                                MessageBox.Show("Максимальное значение не может быть больше Минимального значения.", "Ошибка ввода!");
                            }
                        }
                        else
                        {
                            InsertSolution = false;
                            MessageBox.Show("Некорректно заполнены числовые поля (Максимальное значение, Минимальное значение).", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        InsertSolution = false;
                        MessageBox.Show("Не все поля заполнены.", "Ошибка!");
                    }
                    break;
                case "users":
                    RequestInsert = string.Format("INSERT INTO `users` (`id`, `login`, `pass`) " +
                    "VALUES (null, '{0}', '{1}');",
                    TextBox1.Text, TextBox2.Text);
                    if (TextBox1.Text != "" && TextBox2.Text != "")
                    {
                        if (TextBox1.Text.Length > 3 && TextBox1.Text.Length < 17)
                            InsertSolution = true;
                        else
                        {
                            InsertSolution = false;
                            MessageBox.Show("Логин может содержать только от 4 до 16 символов.", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        InsertSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }
                    break;
                case "manualinput":
                    RequestInsert = string.Format("INSERT INTO `manualinput` (`id_input`, `id_sensor`, `id_user`, `datetime`, `value`) " +
                        "VALUES (null, '{0}', '{1}', '{2}', '{3}');",
                        TextBox1.Text, TextBox2.Text, TextBox3.Text, TextBox4.Text);
                    if (TextBox1.Text != "" && TextBox2.Text != "" && TextBox3.Text != "" && TextBox4.Text != "")
                    {
                        InsertSolution = true;
                    }
                    else
                    {
                        InsertSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }

                    break;
                case "facilities":
                    RequestInsert = string.Format("INSERT INTO `facilities` (`id_facility`, `name_facility`, `description_facility`) " +
                        "VALUES ('{0}', '{1}', '{2}');",
                        TextBox1.Text, TextBox2.Text, TextBox5.Text);
                    if (TextBox1.Text != "" && TextBox2.Text != "")
                    {
                        if (TextBox2.Text.Length > 3 && TextBox2.Text.Length < 50 && TextBox1.Text.Length > 3 && TextBox1.Text.Length < 30)
                            InsertSolution = true;
                        else
                        {
                            InsertSolution = false;
                            MessageBox.Show("Шифр установки может содержать только от 4 до 30 символов.\n Наименование установки может содержать только от 4 до 50 символов.", "Ошибка ввода!");
                        }
                    }
                    else
                    {
                        InsertSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }

                    break;
                case "authority":
                    RequestInsert = string.Format("INSERT INTO `authority` (`id_authority`, `id_user`, `id_sensor`)  " +
                        "VALUES (null, (select id from users where login = '{0}'), (select id_sensor from sensors where name_sensor = '{1}'));",
                        ComboBox2.Text, ComboBox1.Text);
                    if (ComboBox1.Text != "" && ComboBox2.Text != "")
                    {
                        InsertSolution = true;
                    }
                    else
                    {
                        InsertSolution = false;
                        MessageBox.Show("Не все поля заполнены", "Ошибка!");
                    }
                    break;
            }

            if (InsertSolution)
            {
                try
                {
                    Connection.ActionRequest(RequestInsert);

                    if (RequestTriger != null && RequestCreateSensor != null)
                    {
                        Connection.ActionRequest(RequestCreateSensor);
                        Connection.ScriptRequest(RequestTriger);
                    }

                    RequestTriger = null;
                    RequestCreateSensor = null;

                    mainWindow.SelectTable();

                    MessageBox.Show("Добавление прошло успешно", "Добавление");

                    ClearField();
                }
                catch (MySqlException ex) when (ex.Number == 1062)
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

                    FillComboBox(RequestFillComboBox1, ComboBox6);

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
