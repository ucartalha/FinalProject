using Business.Abstract;
using Microsoft.AspNetCore.Mvc;
using System.Net;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UploadedFileController : ControllerBase
    {
        private readonly IUploadFileService _uploadedFileService;

        public UploadedFileController(IUploadFileService uploadedFileService)
        {
            _uploadedFileService = uploadedFileService;
        }

        [HttpPost]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UploadFile(IFormFile file)
        {
            try
            {
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => true;
                _uploadedFileService.AddUploadedFile(file);
                return Ok("File uploaded successfully.");
            }
            catch (ArgumentNullException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                // Hata durumunda ilgili işlemleri gerçekleştirin (örn. hata loglama)
                return StatusCode(StatusCodes.Status500InternalServerError, $"An error occurred while uploading the file: {ex.Message}");
            }
        }
    }
}
