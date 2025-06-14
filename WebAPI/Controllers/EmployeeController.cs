//using Business.Abstract;
//using Core.Utilites.Helpers;
//using Entities.Concrete;
//using Microsoft.AspNetCore.Mvc;
//using RestSharp;
//using System.Net;

//namespace WebAPI.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class EmployeeController : Controller
//    {
//        IEmployeeRecordService _services;
//        public EmployeeController(IEmployeeRecordService service)
//        {
//            _services = service;
//        }
         

//        [HttpGet("getdetails")] 
//        public IActionResult Get(int Id)
//        {
//            var result=_services.GetPersonalDetails(Id);
//            if (result.Success)
//            {
//                return Ok(result);
//            }
//            return BadRequest(result);
//        }

//        [HttpPost("add")]
//        public IActionResult UploadFile(IFormFile file)
//        {
//            try
//            {
//                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                
//                _services.Add(file);
                
//                return Ok("File uploaded successfully.");
//            }
//            catch (ArgumentNullException ex)
//            {
//                return BadRequest(ex.Message);
//            }
//            catch (Exception ex)
//            {
//                // Hata durumunda ilgili işlemleri gerçekleştirin (örn. hata loglama)
//                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while uploading the file: {ex.Message}");
//            }
//        }
//        private bool DosyaZatenVar(string dosyaAdi)
//        {
            
//            return false; // Dosya yoksa varsayılan olarak false döndürüldü.
//        }

//        [HttpPost("deletebydate")]
//        public IActionResult DeleteByDate(DateTime startDate, DateTime endDate)
//        {
//            if(_services.DeleteByDateRange(startDate, endDate).Success)
//            {
//                return Ok();
//            }
//            return BadRequest();
//        }
//        [HttpGet("getbyaveragehour")]
//        public IActionResult GetAverageHour(string name,double average)
//        {
//            if (_services.GetAverageHour(name,average).Success)
//            {
//                return Ok();
//            }
                
//            return BadRequest();
//        }
//        [HttpGet("getbyname")]
//        public IActionResult GetByName(int Id)
//        {
//            var result = _services.GetByName(Id);
//            if (result.Success)
//            {
//                return Ok(result);
//            }
//            return BadRequest();
//        }
//        [HttpGet("getlates")]
//        public IActionResult GetLates(int month,int week, int year)
//        {
//            var result = _services.GetLates(month, week, year);
//            //if (result != null && result.Data != null && result.Data.Count > 0)
//            //{
//            //    var responseList = new List<object>();

//            //    foreach (var employee in result.Data)
//            //    {
//            //        if (employee.IsLate && !employee.IsFullWork)
//            //        {
//            //            responseList.Add(new
//            //            {
//            //                Id=employee.Id,
//            //                FullName = employee.FullName,
//            //                Date=employee.FirstRecord,
//            //                WorkingHour = employee.WorkingHour,
//            //                Message = $"{employee.FullName} geç kaldı ve 9:30 saatten az çalıştı."
//            //            });
//            //        }
//            //        if (employee.IsFullWork && employee.IsLate)
//            //        {
//            //            responseList.Add(new
//            //            {
//            //                Id = employee.Id,
//            //                FullName = employee.FullName,
//            //                Date = employee.FirstRecord,
//            //                WorkingHour=employee.WorkingHour,
//            //                Message = $"{employee.FullName} Geç kaldı fakat tam çalıştı"
//            //            });
//            //        }
//            //        if (!employee.IsLate && !employee.IsFullWork)
//            //        {
//            //            responseList.Add(new
//            //            {
//            //                Id = employee.Id,
//            //                FullName = employee.FullName,
//            //                Date = employee.FirstRecord,
//            //                WorkingHour = employee.WorkingHour,
//            //                Message = $"{employee.FullName} Zamanında geldi fakat 9:30 saatten az çalıştı"
//            //            });
//            //        }
//            //        // Diğer durumlar için benzer şekilde ekleme yapabilirsiniz
//            //    }

//            //    return Ok(new
//            //    {
//            //        Data = responseList,
//            //        Message = "Geç Kalanlar Listelendi"
//            //    });
//            //}
//            //return NotFound(new
//            //{
//            //    Message = "Geç kalan veya 11:30 saatten az çalışan çalışan yok"
//            //});
//            if (result.Success)
//            {
//                return Ok(new {Message=result.Message, Data=result.Data});
//            }
//            return BadRequest(result);
//        }

//        [HttpGet("getlatesbymonth")]
//        public IActionResult GetLatesByMonth(int month, int year)
//        {
//            var result=_services.GetLatesByMonth(month, year);
//            if (result.Success)
//            {
//                return Ok(new { Message = result.Message, Data = result.Data });
//            }
//            return BadRequest(result);
//        }

//    }
//}

