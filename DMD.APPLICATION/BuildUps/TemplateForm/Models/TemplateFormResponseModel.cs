namespace DMD.APPLICATION.BuildUps.TemplateForm.Models
{
    public class TemplateFormResponseModel
    {
        public List<TemplateFormModel> Items { get; set; } = new();
        public int TotalCount { get; set; }
    }
}
