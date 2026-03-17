using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMD.DOMAIN.Entities.Patients
{
    public class PatientProgressNote : BaseEntity<int>
    {
        public int PatientInfoId { get; set; }
        public DateTime? Date { get; set; }
        public string Procedure { get; set; }
        public string Category { get; set; }
        public string Remarks { get; set; }

        public double Balance { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
        public double Discount { get; set; }
        public double TotalAmountDue { get; set; }
        public double AmountPaid { get; set; }
    }
}
