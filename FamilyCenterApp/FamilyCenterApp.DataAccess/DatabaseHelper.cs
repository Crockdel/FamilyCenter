using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using System.Configuration;

namespace FamilyCenterApp.DataAccess
{
    /// <summary>
    /// Вспомогательный класс для работы с базой данных MySQL
    /// </summary>
    public static class DatabaseHelper
    {
        // Строка подключения из app.config
        private static readonly string ConnectionString = "Server=127.0.0.1; Database=family_center_db;UserId=root;Password=vertrigo;Port=3306;Charset=utf8 ";

        static DatabaseHelper()
        {
            // Получаем строку подключения из конфигурационного файла
            ConnectionString = ConfigurationManager.ConnectionStrings["FamilyCenterDB"].ConnectionString;
        }

        /// <summary>
        /// Получить новое подключение к базе данных
        /// </summary>
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(ConnectionString);
        }

        /// <summary>
        /// Выполнить SQL запрос без возврата данных (INSERT, UPDATE, DELETE)
        /// </summary>
        public static int ExecuteNonQuery(string sql, MySqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteNonQuery();
                }
            }
        }

        /// <summary>
        /// Выполнить SQL запрос и вернуть DataReader
        /// </summary>
        public static MySqlDataReader ExecuteReader(string sql, MySqlParameter[] parameters = null)
        {
            var connection = GetConnection();
            connection.Open();
            var command = new MySqlCommand(sql, connection);

            if (parameters != null)
                command.Parameters.AddRange(parameters);

            // CommandBehavior.CloseConnection автоматически закроет соединение при закрытии DataReader
            return command.ExecuteReader(System.Data.CommandBehavior.CloseConnection);
        }

        /// <summary>
        /// Выполнить SQL запрос и вернуть скалярное значение (например, COUNT)
        /// </summary>
        public static object ExecuteScalar(string sql, MySqlParameter[] parameters = null)
        {
            using (var connection = GetConnection())
            {
                connection.Open();
                using (var command = new MySqlCommand(sql, connection))
                {
                    if (parameters != null)
                        command.Parameters.AddRange(parameters);

                    return command.ExecuteScalar();
                }
            }
        }
    }
}