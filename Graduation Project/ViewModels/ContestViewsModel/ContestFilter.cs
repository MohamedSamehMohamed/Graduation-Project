namespace GraduationProject.ViewModels.ContestViewsModel
{
    public class ContestFilter
    {
        public string ContestTitle { get; set; } = "";
        public string PreparedBy { get; set; } = "";
        public string ContestStatus { get; set; } = "";
        public string ContestType { get; set; } = "";
        public string ContestX { get; set; } = "";
        public string ContestPrivacy { get; set; } = "";
        public string Reset { get; set; } = "";
        public int UserId { get; set; } = -1; 
    }
}
