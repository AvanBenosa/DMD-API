using DMD.DOMAIN.Enums;

namespace DMD.APPLICATION.PatientsModule.PatientDentalChart.Models
{
    public class PatientDentalChartSurfaceModel
    {
        public string Id { get; set; } = string.Empty;
        public string PatientTeethId { get; set; } = string.Empty;
        public TeethSurface? Surface { get; set; }
        public string TeethSurfaceName { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
    }
}
