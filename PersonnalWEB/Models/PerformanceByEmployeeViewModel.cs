using Entities.DTOs;

namespace PersonnalWEB.Models
{
    public class PerformanceByEmployeeViewModel
    {
        public DateTime StartDate { get; set; } = DateTime.Now.AddMonths(-1);
        public DateTime EndDate { get; set; } = DateTime.Now;
        public int? SelectedEmployeeId { get; set; }
        public int? DepartmentId { get; set; }

        public List<RemoteEmployeeDto> Employees { get; set; } = new();
        public List<DepartmentDto> Departments { get; set; } = new();
        public string DepartmentName { get; set; }
        public bool IsAdmin { get; set; } = false;
    }
}
