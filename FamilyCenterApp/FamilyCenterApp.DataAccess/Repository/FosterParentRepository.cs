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
    /// Репозиторий для работы с приёмными родителями (таблица foster_parents)
    /// </summary>
    public class FosterParentRepository : IRepository<FosterParent>
    {
        public List<FosterParent> GetAll()
        {
            var fosterParents = new List<FosterParent>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, phone, status 
                           FROM foster_parents ORDER BY last_name, first_name";

            using (var reader = DatabaseHelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    fosterParents.Add(MapToFosterParent(reader));
                }
            }
            return fosterParents;
        }

        public FosterParent GetById(int id)
        {
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, phone, status 
                           FROM foster_parents WHERE id = @id";

            var parameters = new[] { new MySqlParameter("@id", id) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                if (reader.Read())
                {
                    return MapToFosterParent(reader);
                }
            }
            return null;
        }

        public int Add(FosterParent fosterParent)
        {
            string sql = @"INSERT INTO foster_parents 
                          (last_name, first_name, patronymic, birth_date, phone, status) 
                          VALUES (@last_name, @first_name, @patronymic, @birth_date, @phone, @status);
                          SELECT LAST_INSERT_ID();";

            var parameters = new[]
            {
                new MySqlParameter("@last_name", fosterParent.LastName),
                new MySqlParameter("@first_name", fosterParent.FirstName),
                new MySqlParameter("@patronymic", fosterParent.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", fosterParent.BirthDate ?? (object)DBNull.Value),
                new MySqlParameter("@phone", fosterParent.Phone ?? (object)DBNull.Value),
                new MySqlParameter("@status", fosterParent.Status)
            };

            var result = DatabaseHelper.ExecuteScalar(sql, parameters);
            return Convert.ToInt32(result);
        }

        public bool Update(FosterParent fosterParent)
        {
            string sql = @"UPDATE foster_parents SET 
                          last_name = @last_name,
                          first_name = @first_name,
                          patronymic = @patronymic,
                          birth_date = @birth_date,
                          phone = @phone,
                          status = @status
                          WHERE id = @id";

            var parameters = new[]
            {
                new MySqlParameter("@id", fosterParent.Id),
                new MySqlParameter("@last_name", fosterParent.LastName),
                new MySqlParameter("@first_name", fosterParent.FirstName),
                new MySqlParameter("@patronymic", fosterParent.Patronymic ?? (object)DBNull.Value),
                new MySqlParameter("@birth_date", fosterParent.BirthDate ?? (object)DBNull.Value),
                new MySqlParameter("@phone", fosterParent.Phone ?? (object)DBNull.Value),
                new MySqlParameter("@status", fosterParent.Status)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public bool Delete(int id)
        {
            string sql = "DELETE FROM foster_parents WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        public List<FosterParent> Search(string searchText)
        {
            var fosterParents = new List<FosterParent>();
            string sql = @"SELECT id, last_name, first_name, patronymic, birth_date, phone, status 
                           FROM foster_parents 
                           WHERE CONCAT(last_name, ' ', first_name, ' ', patronymic) LIKE @search
                              OR phone LIKE @search
                              OR status LIKE @search
                           ORDER BY last_name, first_name";

            var parameters = new[] { new MySqlParameter("@search", $"%{searchText}%") };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    fosterParents.Add(MapToFosterParent(reader));
                }
            }
            return fosterParents;
        }

        private FosterParent MapToFosterParent(MySqlDataReader reader)
        {
            return new FosterParent
            {
                Id = reader.GetInt32("id"),
                LastName = reader.GetString("last_name"),
                FirstName = reader.GetString("first_name"),
                Patronymic = reader.GetString("patronymic"),
                BirthDate = reader.GetDateTime("birth_date"),
                Phone = reader.GetString("phone"),
                Status = reader.GetString("status")
            };
        }
    }
}