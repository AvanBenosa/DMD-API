using DMD.DOMAIN.Enums;

namespace DMD.APPLICATION.PatientsModule.PatientDentalChart.Models
{
    public class PatientDentalChartModel
    {
        public string PatientInfoId { get; set; } = string.Empty;
        public string Id { get; set; } = string.Empty;
        public int? ToothNumber { get; set; }
        public ToothCondition? Condition { get; set; }
        public string Remarks { get; set; } = string.Empty;
        public List<PatientDentalChartSurfaceModel> Surfaces { get; set; } = new();
        public List<PatientDentalChartImageModel> Images { get; set; } = new();
    }
}
