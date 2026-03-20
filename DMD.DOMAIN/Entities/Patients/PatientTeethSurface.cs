using DMD.DOMAIN.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMD.DOMAIN.Entities.Patients
{
    public class PatientTeethSurface : BaseEntity<int>
    {
        public int PatientTeethId { get; set; }
        public TeethSurface Surface { get; set; } // Mesial, Distal, Occlusal, Buccal, Lingual
        public string TeethSurfaceName { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public PatientTeeth? PatientTeeth { get; set; }
    }
}
