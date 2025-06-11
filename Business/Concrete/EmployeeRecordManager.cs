using Business.Abstract;
using Business.Constants;
using Core.Entities;
using Core.Utilites.Helpers;
using Core.Utilites.Results;
using Core.Utilities.Helpers.FileHelper;
using Core.Extensions;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Entities.DTOs;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml.FormulaParsing.Excel.Functions.DateTime;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace Business.Concrete
{
    public class EmployeeRecordManager : IEmployeeRecordService
    {
        IEmployeeRecordDal _employeeDal;
        IExcelRepository<EmployeeRecord> _excelRepository;
        IFileHelper _fileHelper;
        private readonly InputContext _dbContext;
        IUploadedFilesDal _uploadedFilesDal;
        public EmployeeRecordManager(IEmployeeRecordDal employeeDal, IExcelRepository<EmployeeRecord> excelRepository, IFileHelper fileHelper, InputContext dbContext, IUploadedFilesDal uploadedFilesDal)
        {
            _employeeDal = employeeDal;
            _excelRepository = excelRepository;
            _fileHelper = fileHelper;
            _uploadedFilesDal = uploadedFilesDal;
            _dbContext = dbContext;
        }
        public IResult Add(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentNullException("No file uploaded.");
            }

            // Dosya adını hashleyerek kaydetme
            string fileName = HashHelper.ComputeFileHash(file);

            // Dosya yolu
            //string filePath = Path.Combine(PathToExcel.ExcelPath, fileName);


            if (file != null && file.Length > 0)
            {
                using (var stream = new MemoryStream())
                {
                    file.CopyTo(stream);
                    stream.Position = 0;
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                    var configuration = new ExcelReaderConfiguration
                    {
                        // Karakter kodlamasını burada belirleyin
                        FallbackEncoding = Encoding.GetEncoding("UTF-8"), // Örnek olarak UTF-8 karakter kodlaması kullanılıyor
                                                                          // Diğer yapılandırma seçeneklerini de burada belirleyebilirsiniz
                    };

                    using (var reader = ExcelReaderFactory.CreateReader(stream, configuration))
                    {
                        var records = new List<EmployeeRecord>();

                        while (reader.Read())
                        {
                            try
                            {

                                var cardIdValue = reader.GetValue(1);
                                if (cardIdValue == null || string.IsNullOrEmpty(cardIdValue.ToString()))
                                {
                                    continue;
                                }
                                // Verileri Excel'den okuyarak nesnelere dönüştürme

                                var cardID = reader.GetValue(1) != null ? Convert.ToInt32(reader.GetValue(1)) : 0;
                                var name = reader.GetValue(2) != null ? TransoformTurkishChars.ConvertTurkishChars(reader.GetString(2)) : string.Empty;
                                var surName = reader.GetValue(3) != null ? TransoformTurkishChars.ConvertTurkishChars(reader.GetString(3)) : string.Empty;
                                var sirket = reader.GetValue(4) != null ? reader.GetString(4) : string.Empty;
                                var department = reader.GetValue(5) != null ? reader.GetString(5) : string.Empty;
                                var blok = reader.GetValue(6) != null ? reader.GetString(6) : string.Empty;

                                var date = reader.GetValue(7) != null ? Convert.ToDateTime(reader.GetValue(7)) : DateTime.MinValue;

                                var firstRecord = reader.GetValue(8) != null ? Convert.ToDateTime(reader.GetValue(8)) : DateTime.MinValue;
                                var lastRecord = reader.GetValue(9) != null ? Convert.ToDateTime(reader.GetValue(9)) : DateTime.MinValue;
                                //var workingHour = reader.GetValue(10) != null ? TimeSpan.Parse(Convert.ToString(reader.GetValue(10))) : TimeSpan.Zero;
                                var workingHour = reader.GetValue(10) != null ? TimeSpan.Parse(Convert.ToDateTime(reader.GetValue(10)).ToString("HH:mm:ss")) : TimeSpan.Zero;
                                //var x = reader.GetValue(10);
                                //var y = Convert.ToDateTime(x);
                                //var hh = y.ToString("HH:mm:ss");
                                //var workingHour = TimeSpan.Parse(hh);

                                //TimeSpan workingHour = TimeSpan.Zero;
                                if (name.Contains(" "))
                                {
                                    var nameParts = name.Split(' ');
                                    var nameFirst = nameParts[0];
                                    name = nameFirst; // İlk ismi tekrar name'e atıyoruz
                                    // Diğer işlemleri burada yapabilirsiniz
                                }
                                var matchingRemoteId = GetByFullName(name, surName);
                                if (IsValidEmployeeRecord(lastRecord).Success)
                                {
                                    records.Add(new EmployeeRecord
                                    {
                                        CardId = cardID, 
                                        Name = name,
                                        SurName = surName,
                                        Sirket = sirket,
                                        Department = department,
                                        blok = blok,
                                        Date = date,
                                        FirstRecord = firstRecord,
                                        LastRecord = lastRecord,
                                        WorkingHour = workingHour,
                                        RemoteEmployeeId = matchingRemoteId.Id
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                new ErrorResult("hata burada" + ex);
                            }
                        }
                        var existingRecords = new List<EmployeeRecord>();
                        existingRecords = _employeeDal.GetAll();
                        var distinctRecords = records
                            .GroupBy(r => new { r.CardId, r.Name, r.SurName, r.WorkingHour, r.Department, r.FirstRecord, r.LastRecord, r.Date, r.RemoteEmployeeId })
                            .Select(g => g.First())
                            .ToList();

                        var recordsToAdd = distinctRecords
                            .Where(r => !existingRecords.Any(er =>
                                er.CardId == r.CardId &&
                                er.Name == r.Name &&
                                er.SurName == r.SurName &&
                                er.WorkingHour == r.WorkingHour &&
                                er.Department == r.Department &&
                                er.FirstRecord == r.FirstRecord &&
                                er.LastRecord == r.LastRecord &&
                                er.Date == r.Date &&
                                er.RemoteEmployeeId == r.RemoteEmployeeId
                            ))
                            .ToList();
                        foreach (var empRecord in recordsToAdd)
                        {
                            _employeeDal.Add(empRecord);
                        }

                        // records listesini kullanarak işlemleri gerçekleştirin (örn. veritabanına ekleme)
                        var uploadedFile = new UploadedFile
                        {
                            FileName = file.FileName,
                            FileSize = file.Length,
                            UploadTime = DateTime.Now,
                            ContentHash = HashHelper.ComputeFileHash(file)
                        };

                        // Dosya daha önce yüklenmiş mi kontrolü
                        var existingFile = _dbContext.UploadedFiles.FirstOrDefault(f => f.ContentHash == uploadedFile.ContentHash);
                        if (existingFile != null)
                        {
                            return new ErrorResult("Dosya zaten var.");
                        }
                        if (existingFile!=null)
                        {
                            _dbContext.UploadedFiles.Add(uploadedFile);
                            _dbContext.SaveChanges();
                        }
                        else
                        {
                            return new ErrorResult("Dosya zaten var.");
                        }
                        

                        return new SuccessResult("Excel file has been processed successfully.");
                    }
                }
            }

            return new ErrorResult("No file or empty file is provided.");
        }
        private string GetFilePathFromHash(string hash)
        {
            var uploadedFile = _dbContext.UploadedFiles.FirstOrDefault(f => f.ContentHash == hash);
            if (uploadedFile != null)
            {
                return uploadedFile.ContentHash;
            }
            else
            {
                // Hash değeriyle eşleşen bir dosya bulunamadıysa, gerekli işlemleri yapabilirsiniz.
                // Örneğin, hata döndürebilir veya başka bir varsayılan dosya yolunu döndürebilirsiniz.
                // Bu duruma göre işlemleri burada gerçekleştirebilirsiniz.
                return string.Empty; // veya throw new Exception("Dosya bulunamadı");
            }
        }

        private IResult IsValidEmployeeRecord(DateTime lastRecord)
        {
            var result = _employeeDal.GetAll(c => c.LastRecord == lastRecord);
            if (!result.Any(c => c.LastRecord.Month == lastRecord.Month || c.LastRecord.Year == lastRecord.Year))
            {
                return new SuccessResult("Employee record not found."); // Hata durumunu döndür
            }
            else
            {
                // Geçerli çalışan kaydı bulunduğunu işaretlemek için başka bir işlem yapabilirsiniz
                return new ErrorResult("Employee record is valid."); // Başarılı durumu döndür
            }
        }

        public IResult Delete(int id)
        {
            throw new NotImplementedException();
        }

        public IDataResult<List<EmployeeRecord>> GetAll()
        {
            return new SuccessDataResult<List<EmployeeRecord>>(_employeeDal.GetAll());
        }

        public IDataResult<List<EmployeeRecord>> GetAllByWorkingHour(TimeSpan min, TimeSpan max)
        {
            List<EmployeeRecord> records = _employeeDal.GetAll();
            List<EmployeeRecord> filteredRecords = records.Where(r => r.WorkingHour >= min && r.WorkingHour <= max)
                .OrderBy(r => r.WorkingHour)
                .ToList();


            if (filteredRecords.Count > 0)
            {
                return new SuccessDataResult<List<EmployeeRecord>>(filteredRecords, "Çalışma saatine göre kayıtlar başarıyla getirildi.");
            }
            else
            {
                return new ErrorDataResult<List<EmployeeRecord>>("Belirtilen çalışma saatine uygun kayıt bulunamadı.");
            }
        }

        public IDataResult<List<EmployeeRecord>> GetByCardId(int cardId)
        {
            return new SuccessDataResult<List<EmployeeRecord>>(_employeeDal.GetAll(r => r.CardId == cardId));
        }

        public IDataResult<List<PersonalEmployeeDto>> GetPersonalDetails(int Id)
        {
            return new SuccessDataResult<List<PersonalEmployeeDto>>(_employeeDal.GetEmployeeDetail(Id));
        }

        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1);
            var deleteResult = _employeeDal.DeleteByDateRange(startDate, endDate);
            //date: ay gün yıl olarak geliyor onu düzelt!
            if (deleteResult.Success)
            {
                return new SuccessResult("Belirtilen tarih aralığındaki veriler başarıyla silindi.");
            }
            else
            {
                return new ErrorResult("Belirtilen tarih aralığındaki verileri silerken bir hata oluştu.");
            }

            //DateTime startOfMonth = new DateTime(startDate.Year, startDate.Month,1);
            //DateTime endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

            //try
            //{

            //    var deleteResult = _employeeDal.DeleteByDateRange(startOfMonth, endOfMonth);

            //    if (deleteResult.Success)
            //    {
            //        return new SuccessResult("Ayın verileri başarıyla silindi.");
            //    }
            //    else
            //    {
            //        return new ErrorResult("Ayın verilerini silerken bir hata oluştu.");
            //    }
            //}
            //    catch (Exception ex)
            //    {

            //        return new ErrorResult($"Ayın verilerini silerken bir hata oluştu: {ex.Message}");
            //    }
        }

        public IResult GetAverageHour(string name, double averageHour)
        {
            if (string.IsNullOrEmpty(name))
            {
                return new ErrorResult("İsim alanı boş olamaz.");
            }

            var matchingRecords = _employeeDal.GetAll(r => r.Name == name).ToList();
            var recordCount = matchingRecords.Count();
            if (recordCount > 0)
            {
                var totalWorkingHour = matchingRecords.Sum(r => r.WorkingHour.TotalHours);
                averageHour = totalWorkingHour / recordCount;

                return new SuccessResult(averageHour.ToString());
            }
            else
            {
                return new ErrorResult($"'{name}' adına sahip kayıt bulunamadı.");
            }

        }

        public IDataResult<List<EmployeeRecord>> GetByName(int Id)
        {
            return new SuccessDataResult<List<EmployeeRecord>>(_employeeDal.GetAll(e => e.RemoteEmployeeId == Id));
        }
        //string.Equals(e.FirstName, firstName, StringComparison.OrdinalIgnoreCase) && string.Equals(e.LastName, lastName, StringComparison.OrdinalIgnoreCase) || 
        private RemoteEmployee GetByFullName(string firstName, string lastName)
        {
            var result = _dbContext.EmployeeDtos.FirstOrDefault(e =>
             e.FirstName.Contains(firstName) && e.LastName.Contains(lastName));

            if (result == null)
            {
                // Eğer veri bulunamazsa, yeni bir RemoteEmployee nesnesi oluşturup kaydet
                var newEmployee = new RemoteEmployee
                {
                    FirstName = firstName,
                    LastName = lastName,
                    // Diğer özellikleri de gerekirse burada tanımlayabilirsiniz
                };

                _dbContext.EmployeeDtos.Add(newEmployee);
                _dbContext.SaveChanges(); // Değişiklikleri veritabanına kaydet

                return newEmployee;
            }

            // Eğer veri bulunursa mevcut RemoteEmployee nesnesini döndür
            return result;
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLates(int month, int week,int year)
        {
            var result = _employeeDal.GetLates(month, week, year);
            
            if (result != null && result.Count > 0)
            {
                foreach (var group in result)
                {
                    string message = GetMessageForProcessTemp(group.ProcessTemp);
                    group.Message = message;
                }
                return new SuccessDataResult<List<LateEmployeeGroupDto>>(result, "Geç Kalanlar Listelendi");
            }
            return new ErrorDataResult<List<LateEmployeeGroupDto>>("Geç kalan veya 11.30 saatten az çalışan çalışan yok");
        }

        private string GetMessageForProcessTemp(int processTemp)
        {
            // ProcessTemp değerine göre uygun mesajı döndüren bir fonksiyon ekleyin
            switch (processTemp)
            {
                case 1:
                    return "geç kaldı fakat tam çalıştı";
                case 2:
                    return "geç kaldı ve 9:30 saatten az çalıştı";
                case 3:
                    return "geç kalmadı ama 9:30 saatten az çalıştı";
                case 4:
                    return "geç kalmadı ve tam çalıştı";
                default:
                    return "Bilinmeyen ProcessTemp";
            }
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLatesByMonth(int month, int year)
        {
            var result = _employeeDal.GetLatesByMonth(month, year);

            if (result != null && result.Count > 0)
            {
                foreach (var group in result)
                {
                    string message = GetMessageForProcessTemp(group.ProcessTemp);
                    group.Message = message;
                }
                return new SuccessDataResult<List<LateEmployeeGroupDto>>(result, "Geç Kalanlar Listelendi");
            }
            return new ErrorDataResult<List<LateEmployeeGroupDto>>("Geç kalan veya 11.30 saatten az çalışan çalışan yok");
        }
    }
}
