﻿using Microsoft.AspNetCore.Mvc;

namespace PersonnalWEB.Controllers
{
    public class ErrorController : Controller
    {
        [Route("Error/404")]
        public IActionResult Error404() => View("NotFound");
    }
}
