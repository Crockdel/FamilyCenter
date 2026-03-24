using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyCenterApp.DataAccess.Models
{
    /// <summary>
    /// Модель данных для таблицы foster_parents (приёмные родители)
    /// </summary>
    public class FosterParent
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Phone { get; set; }
        public string Status { get; set; } // кандидат, приёмный родитель, освобождён от обязанностей

        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();
    }
}