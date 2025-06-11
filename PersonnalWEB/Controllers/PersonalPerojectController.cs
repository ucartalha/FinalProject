// -----------------------------------------------------------------------------
// PersonnalTrackingController (.NET 6 uyumlu)
//
// NOT: Aşağıdaki BLL sınıflarını Program.cs içinde DI’ye kaydetmeniz gerekir:
//      builder.Services.AddScoped<VpnEmployeeManagerBLL>();
//      builder.Services.AddScoped<PersonalManagerBLL>();
//      builder.Services.AddScoped<EmployeeRecordManagerBLL>();
//      builder.Services.AddScoped<EmployeeManagerBLL>();
//      builder.Services.AddScoped<UserBLL>();
//      builder.Services.AddScoped<ReportsUserBLL>();
// -----------------------------------------------------------------------------

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using MetropolCard.Reports.Library.BLL.PersonnalTracking;
using MetropolCard.Reports.Library.BLL;
using MetropolCard.Reports.Library.BLL.MReportsUser;
using MetropolCard.Reports.Library.DTO;
using MetropolCard.Reports.Library.DTO.PersonnalTracking;
using MetropolCard.Report.WebIU.Filters;
using MetropolCard.Report.WebIU.Models.ViewModel.PersonnalTracking;
using MetropolCard.Reports.Library.Helper;
using MetropolCard.Reports.Library.Helper.PersonnalTracking;
using Core.Extensions;
using Business.Abstract;

namespace MetropolCard.Report.WebIU.Controllers
{
    [PersonnalTrackingAuthFilter(158, 14, 664, 69, 658, 621, 601, 297)]
    public class PersonnalTrackingController : Controller
    {
        // ---------- BLL bağımlılıkları (ctor-inject) ----------
        private readonly IRemoteWorkEmployeeService _vpn;
        private readonly IPersonalService _personal;
        private readonly EmployeeRecordManagerBLL _record;
        private readonly EmployeeManagerBLL _employee;
        private readonly UserBLL _user;
        private readonly ReportsUserBLL _report;

        public PersonnalTrackingController(
            VpnEmployeeManagerBLL vpn,
            PersonalManagerBLL personal,
            EmployeeRecordManagerBLL record,
            EmployeeManagerBLL employee,
            UserBLL user,
            ReportsUserBLL report)
        {
            _vpn = vpn;
            _personal = personal;
            _record = record;
            _employee = employee;
            _user = user;
            _report = report;
        }

        // ---------- Ortak JSON serializasyon yardımcısı ----------
        private ContentResult JsonResponse(object data)
        {
            var json = JsonConvert.SerializeObject(
                data,
                new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    DateFormatString = "yyyy-MM-ddTHH:mm:ss"
                });
            return Content(json, "application/json");
        }

        // ######################################################################
        // ###########################   VIEW AKSİYONLARI   #####################
        // ######################################################################
        [HttpGet] public IActionResult PerformanceByDepartment() => View();
        [HttpGet] public IActionResult AddFile() => View();
        [HttpGet] public IActionResult OverTime() => View();
        [HttpGet] public IActionResult Percentage() => View();
        [HttpGet] public IActionResult UpdateGuestCard() => View();
        [HttpGet] public IActionResult PersonalRecords() => View();
        [HttpGet] public IActionResult WeeklyMailInformation() => View();

        [HttpGet]
        public IActionResult PerformanceByEmployee(
            DateTime? startdate, DateTime? enddate, int? employeeId)
        {
            ViewBag.StartDate = startdate ?? DateTime.Now.AddDays(-7);
            ViewBag.EndDate = enddate ?? DateTime.Now;
            ViewBag.EmployeeId = employeeId;
            ViewBag.Model = _personal.GetAllEmployees();
            return View();
        }

        [HttpGet]
        public IActionResult EmployeeDepartmantUpdate()
        {
            var vm = new EmployeeDepartmentUpdateVM
            {
                EmployeesDTOs = _employee.DepartmantUpdate(),
                departmentDtos = _user.GetDepartments()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult AllEmployeeDepartmantUpdate()
        {
            var vm = new EmployeeDepartmentUpdateVM
            {
                EmployeesDTOs = _employee.AllDepartmantUpdate(),
                departmentDtos = _user.GetDepartments()
            };
            return View(vm);
        }

        [HttpGet]
        public IActionResult AddUser()
            => View(new AddUserVM { departmentDtos = _user.GetDepartments() });

        // ######################################################################
        // ############################   JSON GET AKSİYONLARI   #################
        // ######################################################################
        [HttpGet]
        public ContentResult GetAllEmployeesWithParams(
            DateTime startDate, DateTime endDate, int? departmentId, int? id)
            => JsonResponse(_vpn.GetAllWithParams(startDate, endDate, departmentId, id));

        [HttpGet]
        public ContentResult GetOfficeAndVpnDates(
            DateTime startDate, DateTime endDate, int? departmentId)
            => JsonResponse(_personal.GetOfficeAndVpnDates(startDate, endDate, departmentId));

        [HttpGet]
        public ContentResult GetLatesWithDepartment(
            DateTime startDate, DateTime endDate, int year, [FromQuery] string[] department)
            => JsonResponse(_record.GetLatesWithDepartment(startDate, endDate, year, department));

        [HttpGet]
        public ContentResult GetAllEmployee()
            => JsonResponse(_personal.GetAllEmployees());

        [HttpGet]
        public ContentResult GetAllDetails(DateTime startDate, DateTime endDate, int empId)
            => JsonResponse(_vpn.GetAllDetails(startDate, endDate, empId));

        [HttpGet]
        public ContentResult ProcessMonthlyAverage(int id, int month, int year)
            => JsonResponse(_personal.ProcessMonthlyAverage(id, month, year));

        [HttpGet]
        public ContentResult GetAllItCard()
            => JsonResponse(_record.GetAllItCard());

        [HttpPost]
        public ContentResult UpdateGuestData(int id, int empId)
            => JsonResponse(_record.UpdateGuestData(id, empId));

        [HttpGet]
        public ContentResult GetOverTime(int month, int year)
            => JsonResponse(_vpn.GetOverTime(month, year));

        [HttpGet]
        public ContentResult Percentages(int month, int year, int? departmentId)
            => JsonResponse(_vpn.Percentages(month, year, departmentId));

        [HttpGet]
        public ContentResult BestPersonalMonth(int month, int year, int departmentId)
            => JsonResponse(_personal.ProcessMonthlyAverageBestPersonal(month, year, departmentId));

        [HttpGet]
        public ContentResult SelectedBestPersonalMonth(int month, int year, [FromQuery] int[] ids)
            => JsonResponse(_personal.ProcessMonthlyAverageSelectedPersonal(month, year, ids));

        [HttpGet]
        public JsonResult GetDepartmantEmployee(int id)
        {
            var dto = _employee.GetDepartmanEmployeeId(id);
            var vm = new EmployeeDepartmentUpdateVM
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                DepartmentId = dto.DepartmentId,
                departmentDtos = _user.GetDepartments()
            };
            return Json(vm);
        }

        [HttpGet]
        public JsonResult GetEmployeesWithDepartmentId(int departmentId)
        {
            var res = _personal.GetEmployeesWithDepartmentId(departmentId);
            if (!res.Success || res.Data is null) return Json(new { data = Array.Empty<object>() });

            var list = (res.Data as List<RemoteEmployeeDto>)!
                .Select(e => new { id = e.Id, firstName = e.FirstName, lastName = e.LastName });
            return Json(new { data = list });
        }

        // ######################################################################
        // ############################   POST AKSİYONLARI   #####################
        // ######################################################################
        [HttpPost]
        public IActionResult AddFileVPN(IFormFile file)
        {
            var res = _vpn.Add(file);
            TempData[res.Success ? "Message" : "ErrorMessage"] = res.Message;
            return RedirectToAction(nameof(AddFile));
        }

        [HttpPost]
        public IActionResult AddFileOffice(IFormFile file)
        {
            var res = _record.Add(file);
            TempData[res.Success ? "Message" : "ErrorMessage"] = res.Message;
            return RedirectToAction(nameof(AddFile));
        }

        [HttpPost]
        public IActionResult UpdateDepartman(EmployeeDepartmentUpdateVM vm)
        {
            var ok = _employee.UpdateDepartman(
                new EmployeeDto { Id = vm.Id, DepartmentId = vm.DepartmentId });
            TempData[ok ? "Message" : "ErrorMessage"] =
                ok ? "Güncelleme Başarılı" : "Değişlik Yapılmadı, Güncelleme Başarısız";
            return RedirectToAction(nameof(EmployeeDepartmantUpdate));
        }

        [HttpPost]
        public IActionResult AllUpdateDepartman(EmployeeDepartmentUpdateVM vm)
        {
            var ok = _employee.UpdateDepartman(
                new EmployeeDto { Id = vm.Id, DepartmentId = vm.DepartmentId });
            TempData[ok ? "Message" : "ErrorMessage"] =
                ok ? "Güncelleme Başarılı" : "Değişlik Yapılmadı, Güncelleme Başarısız";
            return RedirectToAction(nameof(AllEmployeeDepartmantUpdate));
        }

        [HttpPost]
        public IActionResult WeeklyMailInformation(DateTime startDate, DateTime endDate)
        {
            _personal.WeeklyMailInformation(startDate, endDate);
            return RedirectToAction(nameof(WeeklyMailInformation));
        }

        [HttpPost]
        public IActionResult AddUser(AddUserVM vm)
        {
            if (string.IsNullOrWhiteSpace(vm.Name)) return RedirectToAction(nameof(AddUser));

            var dto = new UserDTO
            {
                Name = vm.Name,
                LastName = vm.LastName,
                Departman = vm.Departman,
                Phone = vm.Phone,
                Email = $"{TransoformTurkishChars.ConvertTurkishChars(vm.Name)}." +
                              $"{TransoformTurkishChars.ConvertTurkishChars(vm.LastName)}@metropolcard.com",
                PermissionType = vm.PermissionType,
                Bolge = vm.Bolge,
                SicilNo = vm.SicilNo
            };
            _employee.AddEmployee(dto);
            return RedirectToAction(nameof(AddUser));
        }

        [HttpPost]
        public JsonResult CheckUsernameExists(string username)
            => Json(new { exists = _report.CheckIfUsernameExists(username) });
    }
}
