namespace DMD.APPLICATION.PatientsModule.Patient.Models
{
    public class PatientUploadResultModel
    {
        public int TotalRows { get; set; }
        public int ImportedCount { get; set; }
        public int SkippedCount { get; set; }
        public List<string> Errors { get; set; } = new();
    }
}
