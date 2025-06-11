using Core.Entities;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace Core.Utilites.Helpers
{
    //public class FileProcessingHelper<TEntity> 
    //{
    //    public static string ComputeFileHash(IFormFile file)
    //    {
    //        using (var ms = new MemoryStream())
    //        {
    //            file.CopyTo(ms);
    //            byte[] fileBytes = ms.ToArray();

    //            using (SHA256 sha256 = SHA256.Create())
    //            {
    //                byte[] hashBytes = sha256.ComputeHash(fileBytes);
    //                StringBuilder sb = new StringBuilder();
    //                foreach (byte b in hashBytes)
    //                {
    //                    sb.Append(b.ToString("X2"));
    //                }
    //                return sb.ToString().ToLower();
    //            }
    //        }
    //    }

    //    public static List<T> ProcessExcelFile<T>(IFormFile file)
    //    {
    //        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

    //        List<T> newRecords = new List<T>();

    //        using (var stream = new MemoryStream())
    //        {
    //            file.CopyTo(stream);
    //            stream.Position = 0;
    //            using (var reader = ExcelReaderFactory.CreateReader(stream))
    //            {
    //                while (reader.Read())
    //                {
    //                    try
    //                    {
    //                        // Okuma işlemleri burada devam eder
    //                        // ...
    //                    }
    //                    catch (Exception ex)
    //                    {
    //                        Console.WriteLine(ex.Message);
    //                    }
    //                }
    //            }
    //        }

    //        return newRecords;
    //    }

    //    public static bool IsDuplicateFile(string hashString)
    //    {
    //        var existingFile = DbContext.UploadedFiles.FirstOrDefault(f => f.ContentHash == hashString);
    //        return existingFile != null;
    //    }
    //    public static async Task<string> ComputeFileHashAsync(IFormFile file)
    //    {
    //        using (var ms = new MemoryStream())
    //        {
    //            await file.CopyToAsync(ms);
    //            ms.Position = 0;

    //            using (var sha256 = SHA256.Create())
    //            {
    //                var hashBytes = sha256.ComputeHash(ms);
    //                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
    //            }
    //        }
    //    }

    //    public static async Task<bool> IsDuplicateFileAsync(TContext context,string hashString)
    //    {
    //        return await context.UploadedFiles.AnyAsync(f => f.ContentHash == hashString);
    //    }
    //}
    public class FileProcessingHelper
    {
        public static string ComputeFileHash(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                file.CopyTo(ms);
                byte[] fileBytes = ms.ToArray();

                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] hashBytes = sha256.ComputeHash(fileBytes);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("X2"));
                    }
                    return sb.ToString().ToLower();
                }
            }
        }

       


        //public static bool IsDuplicateFile(TContext context, string hashString)
        //{
        //    var existingFile = context.Set<T>().FirstOrDefault(f =>f.ContentHash == hashString);
        //    return existingFile != null;
        //}

        public static async Task<string> ComputeFileHashAsync(IFormFile file)
        {
            using (var ms = new MemoryStream())
            {
                await file.CopyToAsync(ms);
                ms.Position = 0;

                using (var sha256 = SHA256.Create())
                {
                    var hashBytes = sha256.ComputeHash(ms);
                    return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                }
            }
        }

        //public static async Task<bool> IsDuplicateFileAsync(TContext context, string hashString)
        //{
        //    return await context.Set<T>().AnyAsync(f => f.ContentHash == hashString);
        //}
    }
}
