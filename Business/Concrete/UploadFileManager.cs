using Business.Abstract;
using Business.Constants;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text; 
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class UploadFileManager : IUploadFileService
    {
        IUploadedFilesDal _uploadDal;
        public UploadFileManager(IUploadedFilesDal uploadedFilesDal)
        {
            _uploadDal = uploadedFilesDal;
        }
        public void AddUploadedFile(IFormFile formFile)
        {
            if (formFile == null || formFile.Length == 0)
            { 
                throw new ArgumentNullException("No file uploaded.");
            }

            // Dosya adını hashleyerek kaydetme
            string fileName = GenerateHashedFileName(formFile.FileName);

            // Dosya yolu
            string filePath = Path.Combine(PathToExcel.ExcelPath, fileName);

            try
            {
                // Dosyayı sunucuya kaydetme
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    formFile.CopyTo(stream);
                }

                // UploadedFile nesnesini oluşturma
                var uploadedFile = new UploadedFile
                {
                    FileName = formFile.FileName,
                    FileSize = formFile.Length,
                    UploadTime = DateTime.Now,
                    ContentHash = ComputeFileHash(formFile)
                };

                // Diğer işlemleri gerçekleştirme (örn. veritabanına ekleme)
                //using (var dbContext = new InputContext()) // YourDbContext, kullanılan veritabanı teknolojisine göre değişir
                //{
                //    // Ekleme işlemi
                //    dbContext.UploadedFiles.Add(uploadedFile);
                //    dbContext.SaveChanges();
                //}
                _uploadDal.Add(uploadedFile);
                // Ekleme işlemi başarılı olduğunda dönüş yapma
                return;

            }
            catch (Exception ex)
            {
                // Hata durumunda ilgili işlemleri gerçekleştirin (örn. hata loglama)

                throw new Exception($"An error occurred while adding the file: {ex.Message}");
            }
        }

        private string GenerateHashedFileName(string fileName)
        {
            // Dosya adını hashleyerek yeni bir dosya adı oluşturun
            using (var algorithm = SHA256.Create())
            {
                byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileName);
                byte[] hashBytes = algorithm.ComputeHash(fileNameBytes);
                string hashedFileName = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return hashedFileName;
            }
        }

        private string ComputeFileHash(IFormFile formFile)
        {
            // Dosyanın hash değerini hesaplayın
            using (var algorithm = SHA256.Create())
            {
                using (var stream = formFile.OpenReadStream())
                {
                    byte[] hashBytes = algorithm.ComputeHash(stream);
                    string hash = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                    return hash;
                }
            }
        }
    }
}

