using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MySql.Data.MySqlClient;
using FamilyCenterApp.DataAccess.Models;

namespace FamilyCenterApp.DataAccess.Repositories
{
    /// <summary>
    /// Репозиторий для работы с детьми (таблица children)
    /// </summary>
    public class ChildRepository : IRepository<Child>
    {
        /// <summary>Получить всех детей</summary>
        public List<Child> GetAll()
        {
            var children = new List<Child>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  legal_status, admission_date, notes 
                           FROM children ORDER BY last_name, first_name";

            using (var reader = DatabaseHelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    children.Add(MapToChild(reader));
                }
            }
            return children;
        }

        /// <summary>Получить ребёнка по ID</summary>
        public Child GetById(int id)
        {
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  legal_status, admission_date, notes 
                           FROM children WHERE id = @id";

            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                if (reader.Read())
                {
                    return MapToChild(reader);
                }
            }
            return null;
        }

        /// <summary>Добавить нового ребёнка</summary>
        public int Add(Child child)
        {
            string sql = @"INSERT INTO children 
                          (last_name, first_name, patronymic, birth_date, legal_status, admission_date, notes) 
                          VALUES (@last_name, @first_name, @patronymic, @birth_date, @legal_status, @admission_date, @notes);
                          SELECT LAST_INSERT_ID();";

            var parameters = new[]
            {
                new MySqlParameter("@last_name", child.LastName),
                new MySqlParameter("@first_name", child.FirstName),
                new MySqlParameter("@patronymic", child.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", child.BirthDate),
                new MySqlParameter("@legal_status", child.LegalStatus),
                new MySqlParameter("@admission_date", child.AdmissionDate),
                new MySqlParameter("@notes", child.Notes ?? (object)DBNull.Value)
            };

            var result = DatabaseHelper.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        /// <summary>Обновить данные ребёнка</summary>
        public bool Update(Child child)
        {
            string sql = @"UPDATE children SET 
                          last_name = @last_name,
                          first_name = @first_name,
                          patronymic = @patronymic,
                          birth_date = @birth_date,
                          legal_status = @legal_status,
                          admission_date = @admission_date,
                          notes = @notes
                          WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@id", child.Id),
                new MySqlParameter("@last_name", child.LastName),
                new MySqlParameter("@first_name", child.FirstName),
                new MySqlParameter("@patronymic", child.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", child.BirthDate),
                new MySqlParameter("@legal_status", child.LegalStatus),
                new MySqlParameter("@admission_date", child.AdmissionDate),
                new MySqlParameter("@notes", child.Notes ?? (object)DBNull.Value)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>Удалить ребёнка</summary>
        public bool Delete(int id)
        {
            string sql = "DELETE FROM children WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>Поиск детей по ФИО или примечаниям</summary>
        public List<Child> Search(string searchText)
        {
            var children = new List<Child>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  legal_status, admission_date, notes 
                           FROM children 
                           WHERE CONCAT(last_name, ' ', first_name, ' ', patronymic) LIKE @search
                              OR notes LIKE @search
                           ORDER BY last_name, first_name";

            var parameters = new[] { new MySqlParameter("@search", $"%{searchText}%") };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    children.Add(MapToChild(reader));
                }
            }
            return children;
        }

        /// <summary>Преобразование из DataReader в объект Child</summary>
        private Child MapToChild(MySqlDataReader reader)
        {
            return new Child
            {
                Id = reader.GetInt32("id"),
                LastName = reader.GetString("last_name"),
                FirstName = reader.GetString("first_name"),
                Patronymic = reader.GetString("patronymic"),
                BirthDate = reader.GetDateTime("birth_date"),
                LegalStatus = reader.GetString("legal_status"),
                AdmissionDate = reader.GetDateTime("admission_date"),
                Notes = reader.GetString("notes")
            };
        }
    }
}