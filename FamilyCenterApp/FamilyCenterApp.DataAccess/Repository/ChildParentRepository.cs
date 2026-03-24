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
    /// Репозиторий для работы со связями детей и кровных родителей
    /// </summary>
    public class ChildParentRepository
    {
        /// <summary>
        /// Получить всех кровных родителей ребёнка
        /// </summary>
        public List<Parent> GetParentsByChildId(int childId)
        {
            var parents = new List<Parent>();
            string sql = @"SELECT p.id, p.last_name, p.first_name, p.patronymic, p.birth_date, 
                                  p.phone, p.parental_rights_status, cp.relationship_type, cp.is_active
                           FROM parents p
                           INNER JOIN child_parent cp ON p.id = cp.parent_id
                           WHERE cp.child_id = @childId
                           ORDER BY cp.relationship_type DESC, p.last_name";

            var parameters = new[] { new MySqlParameter("@childId", childId) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    parents.Add(MapToParentWithRelation(reader));
                }
            }
            return parents;
        }

        /// <summary>
        /// Получить всех детей кровного родителя
        /// </summary>
        public List<Child> GetChildrenByParentId(int parentId)
        {
            var children = new List<Child>();
            string sql = @"SELECT c.id, c.last_name, c.first_name, c.patronymic, c.birth_date, 
                                  c.legal_status, c.admission_date, c.notes, cp.relationship_type
                           FROM children c
                           INNER JOIN child_parent cp ON c.id = cp.child_id
                           WHERE cp.parent_id = @parentId";

            var parameters = new[] { new MySqlParameter("@parentId", parentId) };

            using (var reader = DatabaseHelper.ExecuteReader(sql, parameters))
            {
                while (reader.Read())
                {
                    children.Add(MapToChild(reader));
                }
            }
            return children;
        }

        /// <summary>
        /// Добавить связь ребёнка с родителем
        /// </summary>
        public bool AddRelation(int childId, int parentId, string relationshipType, bool isActive = true)
        {
            string sql = @"INSERT INTO child_parent (child_id, parent_id, relationship_type, is_active) 
                           VALUES (@childId, @parentId, @relationshipType, @isActive)";

            var parameters = new[]
            {
                new MySqlParameter("@childId", childId),
                new MySqlParameter("@parentId", parentId),
                new MySqlParameter("@relationshipType", relationshipType),
                new MySqlParameter("@isActive", isActive)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Удалить связь ребёнка с родителем
        /// </summary>
        public bool DeleteRelation(int childId, int parentId)
        {
            string sql = "DELETE FROM child_parent WHERE child_id = @childId AND parent_id = @parentId";
            var parameters = new[]
            {
                new MySqlParameter("@childId", childId),
                new MySqlParameter("@parentId", parentId)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        /// <summary>
        /// Обновить тип родства
        /// </summary>
        public bool UpdateRelationType(int childId, int parentId, string relationshipType)
        {
            string sql = "UPDATE child_parent SET relationship_type = @relationshipType WHERE child_id = @childId AND parent_id = @parentId";
            var parameters = new[]
            {
                new MySqlParameter("@childId", childId),
                new MySqlParameter("@parentId", parentId),
                new MySqlParameter("@relationshipType", relationshipType)
            };

            int rowsAffected = DatabaseHelper.ExecuteNonQuery(sql, parameters);
            return rowsAffected > 0;
        }

        private Parent MapToParentWithRelation(MySqlDataReader reader)
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