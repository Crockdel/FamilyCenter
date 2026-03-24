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
    /// Репозиторий для работы с кровными родителями (таблица parents)
    /// </summary>
    public class ParentRepository : IRepository<Parent>
    {
        public List<Parent> GetAll()
        {
            var parents = new List<Parent>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  phone, parental_rights_status 
                           FROM parents ORDER BY last_name, first_name";

            using (var reader = DatabaseHelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    parents.Add(MapToParent(reader));
                }
            }
            return parents;
        }

        public Parent GetById(int id)
        {
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  phone, parental_rights_status 
                           FROM parents WHERE id = @id";

            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                if (reader.Read())
                {
                    return MapToParent(reader);
                }
            }
            return null;
        }

        public int Add(Parent parent)
        {
            string sql = @"INSERT INTO parents 
                          (last_name, first_name, patronymic, birth_date, phone, parental_rights_status) 
                          VALUES (@last_name, @first_name, @patronymic, @birth_date, @phone, @parental_rights_status);
                          SELECT LAST_INSERT_ID();";

            var parameters = new[]
            {
                new MySqlParameter("@last_name", parent.LastName),
                new MySqlParameter("@first_name", parent.FirstName),
                new MySqlParameter("@patronymic", parent.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", parent.BirthDate ?? (object)DBNull.Value),
                new MySqlParameter("@phone", parent.Phone ?? (object)DBNull.Value),
                new MySqlParameter("@parental_rights_status", parent.ParentalRightsStatus)
            };

            var result = DatabaseHelper.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        public bool Update(Parent parent)
        {
            string sql = @"UPDATE parents SET 
                          last_name = @last_name,
                          first_name = @first_name,
                          patronymic = @patronymic,
                          birth_date = @birth_date,
                          phone = @phone,
                          parental_rights_status = @parental_rights_status
                          WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@id", parent.Id),
                new MySqlParameter("@last_name", parent.LastName),
                new MySqlParameter("@first_name", parent.FirstName),
                new MySqlParameter("@patronymic", parent.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", parent.BirthDate ?? (object)DBNull.Value),
                new MySqlParameter("@phone", parent.Phone ?? (object)DBNull.Value),
                new MySqlParameter("@parental_rights_status", parent.ParentalRightsStatus)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            string sql = "DELETE FROM parents WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public List<Parent> Search(string searchText)
        {
            var parents = new List<Parent>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, 
                                  phone, parental_rights_status 
                           FROM parents 
                           WHERE CONCAT(last_name, ' ', first_name, ' ', patronymic) LIKE @search
                              OR phone LIKE @search
                           ORDER BY last_name, first_name";

            var parameters = new[] { new MySqlParameter("@search", $"%{searchText}%") };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    parents.Add(MapToParent(reader));
                }
            }
            return parents;
        }

        private Parent MapToParent(MySqlDataReader reader)
        {
            return new Parent
            {
                Id = reader.GetInt32("id"),
                LastName = reader.GetString("last_name"),
                FirstName = reader.GetString("first_name"),
                Patronymic = reader.IsDBNull(reader.GetOrdinal("patronymic")) ? null : reader.GetString("patronymic"),
                BirthDate = reader.IsDBNull(reader.GetOrdinal("birth_date")) ? (DateTime?)null : reader.GetDateTime("birth_date"),
                Phone = reader.IsDBNull(reader.GetOrdinal("phone")) ? null : reader.GetString("phone"),
                ParentalRightsStatus = reader.GetString("parental_rights_status")
            };
        }
    }
}