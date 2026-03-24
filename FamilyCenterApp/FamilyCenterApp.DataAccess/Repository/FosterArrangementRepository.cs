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
    /// Репозиторий для работы с устройствами детей в приёмные семьи
    /// </summary>
    public class FosterArrangementRepository
    {
        /// <summary>
        /// Получить все активные устройства
        /// </summary>
        public List<FosterArrangement> GetAllActive()
        {
            var arrangements = new List<FosterArrangement>();
            string sql = @"SELECT fa.*, 
                                  CONCAT(c.last_name, ' ', c.first_name, ' ', c.patronymic) AS child_name,
                                  CONCAT(fp.last_name, ' ', fp.first_name, ' ', fp.patronymic) AS foster_parent_name
                           FROM foster_arrangements fa
                           INNER JOIN children c ON fa.child_id = c.id
                           INNER JOIN foster_parents fp ON fa.foster_parent_id = fp.id
                           WHERE fa.status = 'действует'
                           ORDER BY fa.start_date DESC";

            using (var reader = DatabaseHelper.ExecuteReader(sql))
            {
                while (reader.Read())
                {
                    arrangements.Add(MapToArrangement(reader));
                }
            }
            return arrangements;
        }

        /// <summary>
        /// Получить устройства конкретного ребёнка
        /// </summary>
        public List<FosterArrangement> GetByChildId(int childId)
        {
            var arrangements = new List<FosterArrangement>();
            string sql = @"SELECT fa.*, 
                                  CONCAT(c.last_name, ' ', c.first_name, ' ', c.patronymic) AS child_name,
                                  CONCAT(fp.last_name, ' ', fp.first_name, ' ', fp.patronymic) AS foster_parent_name
                           FROM foster_arrangements fa
                           INNER JOIN children c ON fa.child_id = c.id
                           INNER JOIN foster_parents fp ON fa.foster_parent_id = fp.id
                           WHERE fa.child_id = @childId
                           ORDER BY fa.start_date DESC";

            var parameters = new[] { new MySqlParameter("@childId", childId) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    arrangements.Add(MapToArrangement(reader));
                }
            }
            return arrangements;
        }

        /// <summary>
        /// Получить детей в конкретной приёмной семье
        /// </summary>
        public List<FosterArrangement> GetByFosterParentId(int fosterParentId)
        {
            var arrangements = new List<FosterArrangement>();
            string sql = @"SELECT fa.*, 
                                  CONCAT(c.last_name, ' ', c.first_name, ' ', c.patronymic) AS child_name,
                                  CONCAT(fp.last_name, ' ', fp.first_name, ' ', fp.patronymic) AS foster_parent_name
                           FROM foster_arrangements fa
                           INNER JOIN children c ON fa.child_id = c.id
                           INNER JOIN foster_parents fp ON fa.foster_parent_id = fp.id
                           WHERE fa.foster_parent_id = @fosterParentId
                           ORDER BY fa.start_date DESC";

            var parameters = new[] { new MySqlParameter("@fosterParentId", fosterParentId) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    arrangements.Add(MapToArrangement(reader));
                }
            }
            return arrangements;
        }

        /// <summary>
        /// Добавить устройство ребёнка в приёмную семью
        /// </summary>
        public bool Add(FosterArrangement arrangement)
        {
            string sql = @"INSERT INTO foster_arrangements 
                          (child_id, foster_parent_id, start_date, end_date, arrangement_type, status) 
                          VALUES (@childId, @fosterParentId, @startDate, @endDate, @arrangementType, @status)";

            var parameters = new[]
            {
                new MySqlParameter("@childId", arrangement.ChildId),
                new MySqlParameter("@fosterParentId", arrangement.FosterParentId),
                new MySqlParameter("@startDate", arrangement.StartDate),
                new MySqlParameter("@endDate", arrangement.EndDate ?? (object)DBNull.Value),
                new MySqlParameter("@arrangementType", arrangement.ArrangementType),
                new MySqlParameter("@status", arrangement.Status)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Завершить устройство (установить дату окончания и статус)
        /// </summary>
        public bool EndArrangement(int id, DateTime endDate, string status = "завершено")
        {
            string sql = "UPDATE foster_arrangements SET end_date = @endDate, status = @status WHERE id = @id";
            var parameters = new[]
            {
                new MySqlParameter("@id", id),
                new MySqlParameter("@endDate", endDate),
                new MySqlParameter("@status", status)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Удалить устройство
        /// </summary>
        public bool Delete(int id)
        {
            string sql = "DELETE FROM foster_arrangements WHERE id = @id";
            var parameters = new[] { new MySqlParameter("@id", id) };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        private FosterArrangement MapToArrangement(MySqlDataReader reader)
        {
            return new FosterArrangement
            {
                Id = reader.GetInt32("id"),
                ChildId = reader.GetInt32("child_id"),
                FosterParentId = reader.GetInt32("foster_parent_id"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.IsDBNull(reader.GetOrdinal("end_date")) ? (DateTime?)null : reader.GetDateTime("end_date"),
                ArrangementType = reader.GetString("arrangement_type"),
                Status = reader.GetString("status"),
                ChildName = reader.IsDBNull(reader.GetOrdinal("child_name")) ? null : reader.GetString("child_name"),
                FosterParentName = reader.IsDBNull(reader.GetOrdinal("foster_parent_name")) ? null : reader.GetString("foster_parent_name")
            };
        }
    }
}