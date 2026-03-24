using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyCenterApp.DataAccess.Models
{
    /// <summary>
    /// Модель устройства ребёнка в приёмную семью
    /// </summary>
    public class FosterArrangement
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int FosterParentId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ArrangementType { get; set; } // усыновление/удочерение, опека, приёмная семья, отсевшая семья
        public string Status { get; set; } // действует, завершено, расторгнуто

        // Навигационные свойства
        public string ChildName { get; set; }
        public string FosterParentName { get; set; }
    }
}