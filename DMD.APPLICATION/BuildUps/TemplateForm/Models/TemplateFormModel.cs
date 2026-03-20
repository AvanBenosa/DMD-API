namespace DMD.APPLICATION.BuildUps.TemplateForm.Models
{
    public class TemplateFormModel
    {
        public string Id { get; set; } = string.Empty;
        public string ClinicProfileId { get; set; } = string.Empty;
        public string TemplateName { get; set; } = string.Empty;
        public string TemplateContent { get; set; } = string.Empty;
        public DateTime? Date { get; set; }
    }
}
