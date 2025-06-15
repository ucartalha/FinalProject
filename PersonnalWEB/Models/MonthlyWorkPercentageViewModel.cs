using Entities.DTOs;

namespace PersonnalWEB.Models
{
    public class MonthlyWorkPercentageViewModel
    {
        public List<DepartmentDto> Departments { get; set; } = new();
        public int? SelectedDepartmentId { get; set; }
        public int CurrentYear { get; set; } = DateTime.Now.Year;
        public int CurrentMonth { get; set; } = DateTime.Now.Month;
    }
}
