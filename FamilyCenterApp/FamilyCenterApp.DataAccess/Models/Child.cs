using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyCenterApp.DataAccess.Models
{
    /// <summary>
    /// Модель данных для таблицы children
    /// </summary>
    public class Child
    {
        public int Id { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Patronymic { get; set; }
        public DateTime BirthDate { get; set; }
        public string LegalStatus { get; set; }
        public DateTime AdmissionDate { get; set; }
        public string Notes { get; set; }

        // Свойство для отображения полного имени
        public string FullName => $"{LastName} {FirstName} {Patronymic}".Trim();

        // Свойство для отображения возраста
        public int Age
        {
            get
            {
                var today = DateTime.Today;
                var age = today.Year - BirthDate.Year;
                if (BirthDate.Date > today.AddYears(-age)) age--;
                return age;
            }
        }
    }
}