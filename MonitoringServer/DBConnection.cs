using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MonitoringServer
{
    public class DBConnection
    {
        private string value;
        public MySqlConnection Connection;
        public string RequestSelectTable; 
        public string RequestTables = "SHOW TABLES FROM ";

        public DBConnection(string server, string port, string user, string password, string database, string charset)
        {
            Connection = new MySqlConnection("server=" + server + ";port=" + port + ";user=" + user + ";password=" + password + ";database=" + database + ";charset=" + charset + ";pooling=true;");
            RequestTables += database;
        }

        public void ReСonnection(string server, string port, string user, string password, string database, string charset)
        {
            Connection.ConnectionString = "server=" + server + ";port=" + port + ";user=" + user + ";password=" + password + ";database=" + database + ";charset=" + charset + ";pooling=true;";
        }

        public bool ConnectionCheck()
        {
            try
            {
                Connection.Open();
                Connection.Close();
            }
            catch
            {
                return false;
            }
            return true;
        }

        public void QueryBuilding(string NameTable)
        {
            if (NameTable == "sensors")
                RequestSelectTable = "SELECT id_sensor as 'Код записи', name_sensor as 'Наименование', max_value as 'Максимальное значение', " +
                    "min_value as 'Минимальное значение', unit_sensor as 'Единица измерения', facilities.id_facility as 'Шифр установки', " +
                    "description_sensor as 'Описание' FROM sensors, facilities where facilities.id_facility = sensors.id_facility; ";

            else if (NameTable == "users")
                RequestSelectTable = "SELECT id as 'Код записи', login as 'Логин', pass as 'Пароль' FROM users;";

            else if (NameTable == "manualinput")
                RequestSelectTable = "SELECT id_input as 'Код записи', name_sensor as 'Датчик', login as 'Пользователь', " +
                    "DATE_FORMAT(`datetime`, '%d.%m.%Y %T') as 'Дата и время', `value` as 'Значение' FROM manualinput, users, sensors " +
                    "where manualinput.id_user = users.id and manualinput.id_sensor = sensors.id_sensor;";

            else if (NameTable == "facilities")
                RequestSelectTable = "SELECT id_facility as 'Шифр установки', name_facility as 'Название установки', description_facility as 'Описание' " +
                    "FROM facilities;";

            else if (NameTable == "authority")
                RequestSelectTable = "SELECT id_authority as 'Код записи', login as 'Пользователь', name_sensor as 'Датчик' " +
                    " FROM authority, users, sensors where authority.id_user = users.id and authority.id_sensor = sensors.id_sensor;";

            else if (NameTable == "violation_reports")
                RequestSelectTable = "SELECT id_report as 'Код записи', name_sensor as 'Датчик', DATE_FORMAT(`datetime`, '%d.%m.%Y %T') as 'Дата и время', `value` as 'Значение' " +
                    "FROM violation_reports, sensors where violation_reports.id_sensor = sensors.id_sensor ;";

            else
                RequestSelectTable = "SELECT DATE_FORMAT(`datetime`, '%d.%m.%Y %T') as 'Дата и время', `value` as 'Значение' FROM " + NameTable + ";";
        }

        public string DeleteQueryBuilding(string NameTable, string ID)
        {
            if (NameTable == "sensors")
                return string.Format("DELETE FROM `test`.`{0}` WHERE (`id_sensor` = '{1}');", NameTable, ID);
            else if (NameTable == "users")
                return string.Format("DELETE FROM `test`.`{0}` WHERE (`id` = '{1}');", NameTable, ID);
            else if (NameTable == "manualinput")
                return string.Format("DELETE FROM `test`.`{0}` WHERE (`id_input` = '{1}');", NameTable, ID);
            else if (NameTable == "facilities")
                return string.Format("DELETE FROM `test`.`{0}` WHERE (`id_facility` = '{1}');", NameTable, ID);
            else if (NameTable == "authority")
                return string.Format("DELETE FROM `test`.`{0}` WHERE (`id_authority` = '{1}');", NameTable, ID);

            return null;
        }
        /// <summary>
        /// Выполняет запрос просмотра в БД.
        /// </summary>
        /// <returns>
        /// DataTable.
        /// </returns>
        /// <param name="Request">SQL запрос.</param>
        public DataTable LoadTable(string Request)
        {
            try
            {
                Connection.Open();
                MySqlCommand CommandRequest = new MySqlCommand(Request, Connection);
                DataTable Table = new DataTable();
                Table.Load(CommandRequest.ExecuteReader());
                Connection.Close();
                return Table;
            }
            catch (MySqlException ex)
            {
                MessageBox.Show("Кажется что-то не так с подключением к базе данных. Пожалуйста, проверьте настройки или обратитесь к администратору.", "Упс!");
                Debug.WriteLine(ex);
            }
            finally
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }
            return null;
        }

        /// <summary>
        /// Выполняет запрос просмотра в БД.
        /// </summary>
        /// <returns>
        /// List<string>.
        /// </returns>
        /// <param name="Request">SQL запрос.</param>
        public List<string> LoadReaderOne(string Request)
        {
            List<string> list = new List<string>();
            Connection.Open();
            MySqlCommand CommandRequest = new MySqlCommand(Request, Connection);
            MySqlDataReader Reader = CommandRequest.ExecuteReader();

            while (Reader.Read())
            {
                list.Add(Reader[0].ToString());
            }
            Reader.Close();

            Connection.Close();
            return list;
        }

        /// <summary>
        /// Выполняет запрос на добавление, изменение, удалениев в БД. Ничего не возвращает.
        /// </summary>
        /// <param name="Request">SQL запрос.</param>
        public void ActionRequest(string Request)
        {
            Connection.Open();
            MySqlCommand CommandRequest = new MySqlCommand(Request, Connection);
            CommandRequest.ExecuteNonQuery();
            Connection.Close();
        }

        public void ScriptRequest(string Request)
        {
            Connection.Open();
            MySqlScript CommandRequest = new MySqlScript(Connection, Request);
            CommandRequest.Execute();
            Connection.Close();
        }

        /// <summary>
        /// Выполняет запрос в БД.
        /// </summary>
        /// <returns>
        /// Одно string значение.
        /// </returns>
        /// <param name="Request">SQL запрос.</param>
        public string RequestOneObject(string Request)
        {
            try
            {
                Connection.Open();
                MySqlCommand CommandRequest = new MySqlCommand(Request, Connection);
                MySqlDataReader Reader = CommandRequest.ExecuteReader();

                if (Reader.HasRows)
                {
                    while (Reader.Read())
                    {
                        value = Reader[0].ToString();
                    }
                }
                else
                {
                    value = null;
                }
                Reader.Close();

                Connection.Close();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                if (Connection.State != ConnectionState.Closed)
                {
                    Connection.Close();
                }
            }

            return value;
        }
    }
}
