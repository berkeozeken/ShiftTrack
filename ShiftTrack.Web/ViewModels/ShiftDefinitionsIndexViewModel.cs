namespace ShiftTrack.Web.ViewModels
{
    public class ShiftDefinitionsIndexViewModel
    {
        public ShiftDefinitionFormViewModel Form { get; set; } = new ShiftDefinitionFormViewModel();
        public List<ShiftDefinitionViewModel> Items { get; set; } = new List<ShiftDefinitionViewModel>();
    }
}
