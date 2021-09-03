using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MonitoringServer
{
    class Server
    {
        TcpListener listener;
        bool ServerOperation = false;
        public bool ServerStatus = false;
        static string cmd_request_report;

        public void ServerStart(IPAddress IP, int Port)
        {
            try
            {
                int MaxThreadsCount = Environment.ProcessorCount * 4;
                ThreadPool.SetMaxThreads(MaxThreadsCount, MaxThreadsCount);
                ThreadPool.SetMinThreads(2, 2);

                int counter = 0;
                listener = new TcpListener(IP, Port);

                // Запускаем TcpListener и начинаем слушать клиентов.
                listener.Start();
                ServerOperation = true;
                ServerStatus = true;
                // Принимаем клиентов в бесконечном цикле.
                while (ServerOperation)
                {

                    Console.WriteLine("Waiting for a connection... ");

                    // При появлении клиента добавляем в очередь потоков его обработку.
                    ThreadPool.QueueUserWorkItem(HandlerClient, listener.AcceptTcpClient());
                    // Выводим информацию о подключении.
                    counter++;
                    Console.WriteLine("Connection №" + counter.ToString() + "!");

                }
            }
            catch (SocketException e)
            {
                //В случае ошибки, выводим что это за ошибка.
                Console.WriteLine("SocketException: {0}", e);
            }
        }

        public void ServerStop()
        {
            listener.Stop();
            ServerStatus = false;
            Debug.WriteLine("server stop");
        }

        static void HandlerClient(object client_obj)
        {
            // Буфер для принимаемых данных.
            byte[] Buffer = new byte[4096];
            string ClientStr = "";
            int Count;
            string return_answer;

            TcpClient client = client_obj as TcpClient;

            // Получаем информацию от клиента
            NetworkStream stream = client.GetStream();

            // Принимаем данные от клиента в цикле пока не дойдём до конца.
            while ((Count = stream.Read(Buffer, 0, Buffer.Length)) != 0)
            {
                // Преобразуем данные в ASCII string.
                ClientStr = Encoding.UTF8.GetString(Buffer, 0, Count);

                // Обрабатываем данные в методе
                //data = data.ToUpper();
                return_answer = Procedure(ClientStr); //выполнение запроса

                // Преобразуем полученную строку в массив Байт.
                byte[] msg = Encoding.UTF8.GetBytes(return_answer);

                // Отправляем данные обратно клиенту (ответ).
                stream.Write(msg, 0, msg.Length);

            }

            // Закрываем соединение.
            client.Close();
        }

        static string Procedure(string request)
        {
            string[] str_request = request.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            string id_user = string.Empty, return_answer = str_request[0] + ";";

            MySqlConnection connection = new MySqlConnection("server=localhost;port=3308;user=root;database=test;password=123456;pooling=true;");

            switch (return_answer)
            {
                case "0;":
                    return_answer = "established";
                    break;
                case "1;": //запрос на авторизацию, возвращает список датчиков
                    string cmd_query_auto = "SELECT `id`, `login`, `pass` FROM users WHERE login = '" + str_request[1] + "' AND pass = '" + str_request[2] + "' LIMIT 1;";

                    connection.Open();

                    MySqlCommand cmd1_1 = new MySqlCommand(cmd_query_auto, connection);
                    MySqlDataReader reader_user = cmd1_1.ExecuteReader();

                    if (reader_user.HasRows)
                    {
                        while (reader_user.Read())
                        {
                            id_user = reader_user[0].ToString();
                        }
                    }
                    else
                    {
                        return_answer = "0;error_autorization;";
                        Console.WriteLine("error auto");
                        return return_answer;
                    }
                    reader_user.Close();

                    string cmd_query_authority = "SELECT name_sensor FROM authority, sensors " +
                        "WHERE authority.id_sensor = sensors.id_sensor and id_user = '" + id_user + "'; ";
                    MySqlCommand cmd1_2 = new MySqlCommand(cmd_query_authority, connection);
                    MySqlDataReader reader_authority = cmd1_2.ExecuteReader();

                    while (reader_authority.Read())
                    {
                        return_answer += reader_authority[0] + ";";
                    }
                    reader_authority.Close();

                    connection.Close();
                    break;

                case "2;"://запрос последних 60 значений, возвращает 60 значений
                    string cmd_request_info_sensor = "SELECT `name_facility`, `unit_sensor`, `max_value`, `min_value` FROM sensors, facilities WHERE sensors.id_facility = facilities.id_facility AND name_sensor = '" + str_request[1] + "';";
                    string cmd_request_last60 = "SELECT `value` FROM " + str_request[1] + " ORDER BY datetime DESC LIMIT 60";

                    connection.Open();

                    MySqlCommand cmd2_1 = new MySqlCommand(cmd_request_info_sensor, connection);
                    MySqlDataReader reader_info_sensor = cmd2_1.ExecuteReader();


                    if (reader_info_sensor.HasRows)
                    {
                        while (reader_info_sensor.Read())
                        {
                            return_answer += reader_info_sensor[0] + ";" + reader_info_sensor[1] + ";" +
                                reader_info_sensor[2] + ";" + reader_info_sensor[3] + ";";
                        }
                    }
                    else
                    {
                        return_answer = "0;error_select_info_sensor;";
                        Console.WriteLine("error select info sensor");
                        return return_answer;
                    }
                    reader_info_sensor.Close();

                    MySqlCommand cmd2_2 = new MySqlCommand(cmd_request_last60, connection);
                    MySqlDataReader reader_last60 = cmd2_2.ExecuteReader();

                    if (reader_last60.HasRows)
                    {
                        while (reader_last60.Read())
                        {
                            return_answer += reader_last60[0] + ";";
                        }
                    }
                    else
                    {
                        return_answer += "no_value;";
                    }
                    reader_last60.Close();

                    connection.Close();
                    break;

                case "3;"://запрос последнего значения, возвращает последнее значение
                    string cmd_request_last = "SELECT `value` FROM " + str_request[1] + " ORDER BY datetime DESC LIMIT 1";

                    connection.Open();

                    MySqlCommand cmd3 = new MySqlCommand(cmd_request_last, connection);
                    MySqlDataReader reader_last = cmd3.ExecuteReader();

                    if (reader_last.HasRows)
                    {
                        while (reader_last.Read())
                        {
                            return_answer += reader_last[0] + ";";
                        }
                    }
                    else
                    {
                        return_answer = "0;error_select_last;";
                        Console.WriteLine("error select last");
                        return return_answer;
                    }
                    reader_last.Close();
                                                                                                                                                                                                 {
                        Random random = new Random();
                        string cmd1_query = "insert into pid_1_1_a values (Now(), " + random.Next(20, 80) + ");";
                        string cmd2_query = "insert into pit_1_1_b values (Now(), " + random.Next(400, 450) + ");";
                        string cmd3_query = "insert into pit_1_2_a values (Now(), " + random.Next(120, 200) + ");";
                        MySqlCommand cmdq = new MySqlCommand(cmd1_query, connection);
                        MySqlCommand cmdw = new MySqlCommand(cmd2_query, connection);
                        MySqlCommand cmde = new MySqlCommand(cmd3_query, connection);
                        cmdq.ExecuteNonQuery();
                        cmdw.ExecuteNonQuery();
                        cmde.ExecuteNonQuery();
                    }
                    connection.Close();

                    break;

                case "4;"://запрос списка датчиков, возвращает все информацию по запращиваемым датчикам
                    string cmd_query_array_sensors = "SELECT `name_sensor`, `unit_sensor`, `max_value`, `min_value` " +
                        "FROM sensors where name_sensor = '" + str_request[1] + "';";
                    string cmd_request_last_array_sensors = "SELECT `value` FROM " + str_request[1] + " ORDER BY datetime DESC LIMIT 1;";

                    connection.Open();

                    MySqlCommand cmd4_1 = new MySqlCommand(cmd_query_array_sensors, connection);
                    MySqlDataReader reader_query_array_sensors = cmd4_1.ExecuteReader();

                    if (reader_query_array_sensors.HasRows)
                    {
                        while (reader_query_array_sensors.Read())
                        {
                            return_answer += reader_query_array_sensors[0] + ";" + reader_query_array_sensors[1] + ";" +
                                reader_query_array_sensors[2] + ";" + reader_query_array_sensors[3] + ";";
                        }
                    }
                    else
                    {
                        return_answer = "0;error_no_sensors_attached;";
                        Console.WriteLine("error no sensors attached");
                        return return_answer;
                    }
                    reader_query_array_sensors.Close();

                    MySqlCommand cmd4_2 = new MySqlCommand(cmd_request_last_array_sensors, connection);
                    MySqlDataReader reader_request_last_array_sensors = cmd4_2.ExecuteReader();

                    try
                    {
                        if (reader_request_last_array_sensors.HasRows)
                        {
                            while (reader_request_last_array_sensors.Read())
                            {
                                return_answer += reader_request_last_array_sensors[0] + ";";
                            }
                        }
                        else
                        {
                            return_answer += "-;";
                        }
                        reader_request_last_array_sensors.Close();
                    }
                    catch
                    {
                        return_answer += "-;";
                        reader_request_last_array_sensors.Close();
                    }

                    connection.Close();
                    break;

                case "5;"://запрос последних значений всех датчиков
                    connection.Open();

                    for (int i = 1; i < str_request.Length; i++)
                    {
                        string cmd_request_last_values = "SELECT `value` FROM " + str_request[i] + " ORDER BY datetime DESC LIMIT 1";
                        Debug.WriteLine(cmd_request_last_values);

                        MySqlCommand cmd5 = new MySqlCommand(cmd_request_last_values, connection);
                        MySqlDataReader reader_last_values = cmd5.ExecuteReader();

                        if (reader_last_values.HasRows)
                        {
                            while (reader_last_values.Read())
                            {
                                return_answer += reader_last_values[0] + ";";
                            }
                        }
                        else
                        {
                            return_answer += "-;";
                        }
                        reader_last_values.Close();
                    }

                    connection.Close();
                    break;

                case "6;":// запрос отчета
                    cmd_request_report = "SELECT DATE_FORMAT(violation_reports.`datetime`, '%d.%m.%Y'), " +
                            "sensors.id_facility, facilities.name_facility, violation_reports.id_sensor, " +
                            "sensors.name_sensor, count(violation_reports.id_sensor) FROM sensors, facilities, violation_reports " +
                            "where facilities.id_facility = sensors.id_facility and sensors.id_sensor = violation_reports.id_sensor " +
                            "and violation_reports.`datetime` between '" + str_request[1] + "' and '" + str_request[2] +
                            "' and sensors.name_sensor in (";

                    for (int i = 3; i < str_request.Length; i++)
                    {
                        if (i < str_request.Length - 1)
                            cmd_request_report += "'" + str_request[i] + "', ";
                        else
                            cmd_request_report += "'" + str_request[i] + "' ";

                    }

                    cmd_request_report += ") group by date(violation_reports.`datetime`), violation_reports.id_sensor " +
                        "order by date(violation_reports.`datetime`); ";

                    connection.Open();

                    MySqlCommand cmd6 = new MySqlCommand(cmd_request_report, connection);
                    MySqlDataReader reader_last_report = cmd6.ExecuteReader();

                    if (reader_last_report.HasRows)
                    {
                        while (reader_last_report.Read())
                        {
                            return_answer += reader_last_report[0] + ";" + reader_last_report[1] + ";"
                                + reader_last_report[2] + ";" + reader_last_report[3] + ";" + reader_last_report[4]
                                + ";" + reader_last_report[5] + ";";
                        }
                    }
                    else
                    {
                        return_answer = "0;error_no_results_for_the_selected_period;";
                        Console.WriteLine("error no results for the selected period");
                        return return_answer;
                    }
                    reader_last_report.Close();

                    connection.Close();
                    break;

                case "7;":
                    string cmd_request_manual_input = "INSERT INTO `test`.`manualinput` (`id_input`, `id_sensor`, `id_user`, `datetime`, `value`) " +
                        "VALUES(null, (select id_sensor from sensors where name_sensor = '" + str_request[1] + "'), " +
                        "(select id from users where login = '" + str_request[2] + "'), '" + str_request[3] + "', '" + str_request[4] + "');";

                    connection.Open();

                    MySqlCommand cmd7 = new MySqlCommand(cmd_request_manual_input, connection);
                    int reader_manual_input = cmd7.ExecuteNonQuery();

                    if (reader_manual_input >= 1)
                    {
                        return_answer += "input_made;";
                    }
                    else if (reader_manual_input <= 0)
                    {
                        return_answer = "0;error_no_insert_manual_input;";
                    }

                    connection.Close();
                    break;
            }
            return return_answer;
        }

    }
}
