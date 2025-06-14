using Business.Abstract;
using Core.Extensions;
using Entities.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using PersonnalWEB.Models;

public class PersonnalTrackingController : Controller
{
    private readonly IVpnEmployeeService _vpnEmployeeService;
    private readonly IPersonalService _personalService;
    private readonly IEmployeeRecordService _employeeRecordService;
    private readonly IEmployeeService _employeeService;



    public PersonnalTrackingController(
        IVpnEmployeeService vpnEmployeeService,
        IPersonalService personalService,
        IEmployeeRecordService employeeRecordService,
        IEmployeeService employeeService
        )
    {
        _vpnEmployeeService = vpnEmployeeService;
        _personalService = personalService;
        _employeeRecordService = employeeRecordService;
        _employeeService = employeeService;
       
    }

    [HttpGet]
    public ActionResult PerformanceByDepartment() => View();

    [HttpGet]
    public ContentResult GetAllEmployeesWithParams(DateTime startDate, DateTime endDate, int? departmentId, int? Id)
    {
        var result = _vpnEmployeeService.GetAllWithParams(startDate, endDate, departmentId, Id);
        return JsonCamel(result);
    }

    [HttpGet]
    public ContentResult GetOfficeAndVpnDates(DateTime startDate, DateTime endDate, int? departmentId)
    {
        var result = _personalService.GetOfficeAndVpnDates(startDate, endDate, departmentId);
        return JsonCamel(result);
    }

    [HttpGet]
    public ContentResult GetLatesWithDepartment(DateTime startDate, DateTime endDate, int year, string[] department)
    {
        var result = _employeeRecordService.GetLatesWithDepartment(startDate, endDate, year, department);
        return JsonCamel(result);
    }

    [HttpGet]
    public ActionResult AddFile() => View();

    [HttpPost]
    public ActionResult AddFileVPN(IFormFile file)
    {
        var result = _vpnEmployeeService.Add(file);
        return RedirectToAction("AddFile");
    }

    [HttpPost]
    public ActionResult AddFileOffice(IFormFile file)
    {
        var result = _employeeRecordService.Add(file);
        return RedirectToAction("AddFile");
    }

    [HttpGet]
    public ActionResult PerformanceByEmployee(DateTime? startdate, DateTime? enddate, int? employeeId)
    {
        ViewBag.StartDate = startdate ?? DateTime.MinValue;
        ViewBag.EndDate = enddate ?? DateTime.Now;
        ViewBag.EmployeeId = employeeId;
        ViewBag.Model = GetAllEmployee();
        return View();
    }

    [HttpGet]
    public ContentResult GetAllEmployee()
    {
        var result = _personalService.GetAllEmployees();
        return JsonCamel(result);
    }

    [HttpGet]
    public ContentResult GetAllDetails(DateTime startDate, DateTime endDate, int empId)
    {
        var result = _vpnEmployeeService.GetAllDetails(startDate, endDate, empId);
        return JsonCamel(result);
    }

    [HttpGet]
    public ContentResult ProcessMonthlyAverage(int Id, int month, int year)
    {
        var result = _personalService.ProcessMonthlyAverage(Id, month, year);
        return JsonCamel(result);
    }

    [HttpGet]
    public ActionResult OverTime() => View();

    [HttpGet]
    public ContentResult GetOverTime(int month, int year)
    {
        var result = _vpnEmployeeService.GetOverTime(month, year);
        return JsonCamel(result);
    }

    [HttpGet]
    public ActionResult Percentage() => View();

    [HttpGet]
    public ContentResult Percentages(int month, int year, int? departmentId)
    {
        var result = _vpnEmployeeService.Percentages(month, year, departmentId);
        return JsonCamel(result);
    }

    [HttpGet]
    public ActionResult PersonalRecords() => View();

    [HttpPost]
    public ActionResult UpdateDepartman(EmployeeDepartmentUpdateVM updateVM)
    {
        var dto = new EmployeeDto
        {
            Id = updateVM.Id,
            DepartmentId = updateVM.DepartmentId
        };

        bool success = _employeeService.UpdateDepartment(dto);
        TempData[success ? "Message" : "ErrorMessage"] = success ? "Güncelleme Başarılı" : "Değişiklik yapılmadı, güncelleme başarısız";
        return RedirectToAction("EmployeeDepartmantUpdate");
    }

    [HttpGet]
    public ActionResult EmployeeDepartmantUpdate()
    {
        var vm = new EmployeeDepartmentUpdateVM
        {
            EmployeesDTOs = _employeeService.DepartmantUpdate(),
            departmentDtos = _employeeService.GetDepartments()
        };
        return View(vm);
    }

    [HttpGet]
    public JsonResult GetDepartmantEmployee(int id)
    {
        var dto = _employeeService.GetEmployeeById(id).Data;
        var vm = new EmployeeDepartmentUpdateVM
        {
            Id = dto.Id,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            DepartmentId = dto.DepartmentId,
            departmentDtos = _employeeService.GetDepartments()
        };
        return Json(vm);
    }

    [HttpGet]
    public ActionResult AllEmployeeDepartmantUpdate()
    {
        var vm = new EmployeeDepartmentUpdateVM
        {
            EmployeesDTOs = _employeeService.GetAllDepartmentUpdates().Data,
            departmentDtos = _employeeService.GetDepartments()
        };
        return View(vm);
    }

    [HttpPost]
    public ActionResult AllUpdateDepartman(EmployeeDepartmentUpdateVM updateVM)
    {
        var dto = new EmployeeDto
        {
            Id = updateVM.Id,
            DepartmentId = updateVM.DepartmentId
        };

        bool success = _employeeService.UpdateDepartment(dto);
        TempData[success ? "Message" : "ErrorMessage"] = success ? "Güncelleme Başarılı" : "Değişiklik yapılmadı, güncelleme başarısız";
        return RedirectToAction("AllEmployeeDepartmantUpdate");
    }

    [HttpGet]
    public ContentResult BestPersonalMonth(int month, int year, int departmentId)
    {
        var result = _personalService.ProcessMonthlyAverageBestPersonal(month, year, departmentId);
        return JsonCamel(result);
    }

    [HttpGet]
    public ContentResult SelectedBestPersonalMonth(int month, int year, int[] ids)
    {
        var result = _personalService.ProcessMonthlyAverageSelectedPersonal(month, year, ids);
        return JsonCamel(result);
    }


    



 

    //[HttpPost]
    //public JsonResult CheckUsernameExists(string username)
    //{
    //    bool exists = _reportsUserService.CheckIfUsernameExists(username);
    //    return Json(new { exists });
    //}

    [HttpGet]
    public JsonResult GetEmployeesWithDepartmentId(int departmentId)
    {
        var response = _personalService.GetEmployeesWithDepartmentId(departmentId);

        if (response.Success)
        {
            var employeeList = response.Data as List<RemoteEmployeeDto>;
            var result = employeeList.Select(e => new
            {
                id = e.Id,
                firstName = e.FirstName,
                lastName = e.LastName
            }).ToList();

            return Json(new { data = result });
        }

        return Json(new { data = new List<object>() });
    }

    private ContentResult JsonCamel(object result)
    {
        var serialized = JsonConvert.SerializeObject(result, new JsonSerializerSettings
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            DateFormatString = "yyyy-MM-ddTHH:mm:ss"
        });

        return Content(serialized, "application/json");
    }
}
