using Entities.DTOs;

namespace PersonnalWEB.Models
{
    public class EmployeeDepartmentUpdateVM
    {
        public List<EmployeeDto> EmployeesDTOs { get; set; }
        public List<DepartmentDto> departmentDtos { get; set; }
        public string PageName { get; set; }
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int UserId { get; set; }
        public int? DepartmentId { get; set; }
    }
}
