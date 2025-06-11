using Business.Abstract;
using Business.Concrete;
using Microsoft.AspNetCore.Mvc;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PersonalController : Controller
    {
        IPersonalService _services;
        public PersonalController(IPersonalService personalService)
        {
                _services = personalService;
        }
        [HttpGet("process-monthly-average")]
        public IActionResult ProcessMonthlyAverage(int Id, int month, int year)
        {
            var result = _services.ProcessMonthlyAverage(Id, month, year);

            if (result.Success)
            {
                return Ok(result); // Başarılı ise Personal nesnesini döndür
            }
            else
            {
                return BadRequest(result.Message); // Hata durumunda hata mesajını döndür
            }
        }

        [HttpGet("get-all-employees")]
        public IActionResult GetAllEmployees()
        {
            var result = _services.GetAllEmployees();
            if (result.Success)
            {
                return Ok(result);
            }
            return BadRequest(result.Message);
        }
    }
}
