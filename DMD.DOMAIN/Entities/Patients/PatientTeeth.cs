using DMD.DOMAIN.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMD.DOMAIN.Entities.Patients
{
    public class PatientTeeth : BaseEntity<int>
    {
        public int PatientInfoId { get; set; }

        // Tooth number (1 - 32)
        public int ToothNumber { get; set; }

        public ToothCondition Condition { get; set; }

        // Dentist remarks
        public string Remarks { get; set; } = string.Empty;

        public ICollection<PatientTeethSurface> TeethSurfaces { get; set; } = new List<PatientTeethSurface>();
        public ICollection<PatientTeethImage> TeethImages { get; set; } = new List<PatientTeethImage>();
    }
}
