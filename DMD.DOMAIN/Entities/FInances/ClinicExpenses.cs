using DMD.DOMAIN.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DMD.DOMAIN.Entities.FInances
{
    public class ClinicExpenses : BaseEntity<int>
    {
        public int ClinicProfileId { get; set; }
        public string Remarks { get; set; }
        public ExpenseCategory Category { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }

    }
}
