using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FamilyCenterApp.DataAccess.Models
{
    /// <summary>
    /// Модель связи ребёнка с кровным родителем
    /// </summary>
    public class ChildParent
    {
        public int Id { get; set; }
        public int ChildId { get; set; }
        public int ParentId { get; set; }
        public string RelationshipType { get; set; } // мать, отец, опекун (временный), иной родственник
        public bool IsActive { get; set; } // активна ли связь

        // Навигационные свойства (для удобства отображения)
        public string ChildName { get; set; }
        public string ParentName { get; set; }
    }
}