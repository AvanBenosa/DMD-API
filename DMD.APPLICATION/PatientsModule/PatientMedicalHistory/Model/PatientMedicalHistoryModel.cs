

namespace DMD.APPLICATION.PatientsModule.PatientMedicalHistory.Model
{
    public class PatientMedicalHistoryModel
    {
        public int Id { get; set; }
        public DateTime? Date { get; set; }
        public int PatientsInfoId { get; set; }
        public bool Q1 { get; set; }
        public bool Q2 { get; set; }
        public bool Q3 { get; set; }
        public bool Q4 { get; set; }
        public bool Q5 { get; set; }
        public bool Q6 { get; set; }
        public bool Q7 { get; set; }
        public bool Q8 { get; set; }
        public bool Q9 { get; set; }
        public bool Q10Nursing { get; set; }
        public bool Q10Pregnant { get; set; }
        public bool Q12 { get; set; }
        public bool Q13 { get; set; }
        public string Others { get; set; }
        public string Remarks { get; set; }
    }
}
