using DMD.DOMAIN.Enums;

namespace DMD.APPLICATION.Finances.ClinicExpenses.Models
{
    public class ClinicExpensesModel
    {
        public string Id { get; set; } = string.Empty;
        public string ClinicProfileId { get; set; } = string.Empty;
        public string Remarks { get; set; } = string.Empty;
        public ExpenseCategory Category { get; set; }
        public DateTime Date { get; set; }
        public double Amount { get; set; }
    }
}
