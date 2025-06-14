//using Business.Abstract;
//using Microsoft.AspNetCore.Mvc;

//namespace WebAPI.Controllers
//{
//    [ApiController]
//    [Route("[controller]")]
//    public class OverShiftController : Controller
//    {
//        IOverShifService _services;
//        public OverShiftController(IOverShifService service)
//        {
//            _services = service;
//        }
//        [HttpGet("getovershift")]
//        public IActionResult GetOverShift(int Id, int month, int year)
//        {
//            var result = _services.ProcessShiftPrice(Id, month, year);
//            if (result.Success)
//            {
//                return Ok(result);
//            }
//            return BadRequest();
//        }

//        [HttpGet("getallovershift")]
//        public IActionResult GetAllOverShift(int month,int year)
//        {
//            var result = _services.ProcessShiftPriceAllWorkers(month, year);
//            if(result.Success)
//            {
//                return Ok(result);
//            }
//            return BadRequest();
//        }
//    }
//}
