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
        IPersonalDal _personalDal;
        IExcelRepository<EmployeeRecord> _excelRepository;
        IFileHelper _fileHelper;
        private readonly InputContext _dbContext;
        IUploadedFilesDal _uploadedFilesDal;
        public EmployeeRecordManager(IEmployeeRecordDal employeeDal,IPersonalDal personalDal, IExcelRepository<EmployeeRecord> excelRepository, IFileHelper fileHelper, InputContext dbContext, IUploadedFilesDal uploadedFilesDal)
        {
            _employeeDal = employeeDal;
            _excelRepository = excelRepository;
            _fileHelper = fileHelper;
            _uploadedFilesDal = uploadedFilesDal;
            _dbContext = dbContext;
            _personalDal = personalDal;
        }
        public IResult Add(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return new ErrorResult("Dosya bulunamadı veya boş.");

            var fileHash = HashHelper.ComputeFileHash(file);
            var existingFile = _dbContext.UploadedFiles
                .FirstOrDefault(f => f.ContentHash == fileHash);

            if (existingFile != null)
                return new ErrorResult("Bu dosya daha önce yüklenmiş.");

            var records = new List<EmployeeRecord>();

            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;

                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                var config = new ExcelReaderConfiguration
                {
                    FallbackEncoding = Encoding.GetEncoding("UTF-8")
                };

                using (var reader = ExcelReaderFactory.CreateReader(stream, config))
                {
                    while (reader.Read())
                    {
                        try
                        {
                            var cardIdValue = reader.GetValue(1);
                            if (cardIdValue == null || string.IsNullOrWhiteSpace(cardIdValue.ToString()))
                                continue;

                            int cardID = Convert.ToInt32(reader.GetValue(1));
                            string userValue = reader.GetValue(2)?.ToString();
                            userValue = TransoformTurkishChars.ConvertTurkishChars(userValue);

                            string department = TransoformTurkishChars.ConvertTurkishChars(reader.GetValue(3)?.ToString() ?? "");
                            string blok = reader.GetValue(4)?.ToString() ?? "";

                            DateTime date = reader.GetValue(5) != null ? Convert.ToDateTime(reader.GetValue(5)) : DateTime.MinValue;
                            DateTime firstRecord = reader.GetValue(6) != null ? Convert.ToDateTime(reader.GetValue(6)) : DateTime.MinValue;
                            DateTime lastRecord = reader.GetValue(7) != null ? Convert.ToDateTime(reader.GetValue(7)) : DateTime.MinValue;
                            TimeSpan workingHour = reader.GetValue(8) != null
                                ? TimeSpan.Parse(Convert.ToDateTime(reader.GetValue(8)).ToString("HH:mm:ss"))
                                : TimeSpan.Zero;

                            string[] nameParts = userValue.Split('.');
                            string name = nameParts.Length > 0 ? nameParts[0] : "";
                            string surname = nameParts.Length > 1 ? nameParts[1] : "";

                            var remote = GetByFullName(name, surname);
                            var userWithDept = _employeeDal.GetUserWithDepartment(name, surname);

                            if (IsValidEmployeeRecord(lastRecord).Success)
                            {
                                records.Add(new EmployeeRecord
                                {
                                    CardId = cardID,
                                    Name = name,
                                    SurName = surname,
                                    Sirket = " ",
                                    DepartmentId = userWithDept.Data.DepartmentId,
                                    Department = userWithDept.Data.DepartmetName != "Bilinmeyen" ? userWithDept.Data.DepartmetName : department,
                                    blok = blok,
                                    Date = date,
                                    FirstRecord = firstRecord,
                                    LastRecord = lastRecord,
                                    WorkingHour = workingHour,
                                    RemoteEmployeeId = remote.Id
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            return new ErrorResult("Excel verisi işlenirken hata oluştu: " + ex.Message);
                        }
                    }
                }
            }

            // Duplicate kayıt kontrolü
            var existingRecords = _employeeDal.GetAll();
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
                    er.RemoteEmployeeId == r.RemoteEmployeeId))
                .ToList();

            foreach (var empRecord in recordsToAdd)
            {
                _employeeDal.Add(empRecord);
            }

            // Dosya kaydı
           _dbContext.UploadedFiles.Add(new UploadedFile
            {
                FileName = file.FileName,
                FileSize = file.Length,
                UploadTime = DateTime.Now,
                ContentHash = fileHash
            });

            _dbContext.SaveChanges();

            return new SuccessResult("Dosya başarıyla işlendi ve veriler eklendi.");
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
            var result = _employeeDal.GetEmployeeDetail(Id);
            return new SuccessDataResult<List<PersonalEmployeeDto>>(result.Data);
        }

        public IResult DeleteByDateRange(DateTime startDate, DateTime endDate)
        {
            endDate = endDate.AddDays(1);
            var deleteResult = _employeeDal.DeleteByDateRange(startDate, endDate);
            
            if (deleteResult.Success)
            {
                return new SuccessResult("Belirtilen tarih aralığındaki veriler başarıyla silindi.");
            }
            else
            {
                return new ErrorResult("Belirtilen tarih aralığındaki verileri silerken bir hata oluştu.");
            }

         
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
        private Personnal GetByFullName(string firstName, string lastName)
        {
            var result = _dbContext.Personnals.FirstOrDefault(e =>
             e.FirstName.Contains(firstName) && e.LastName.Contains(lastName));

            if (result == null)
            {
                // Eğer veri bulunamazsa, yeni bir RemoteEmployee nesnesi oluşturup kaydet
                var newEmployee = new Personnal
                {
                    FirstName = firstName,
                    LastName = lastName,
                    UserName= firstName +'.'+lastName,
                    DepartmentId=0
                };

                _dbContext.Personnals.Add(newEmployee);
                _dbContext.SaveChanges(); // Değişiklikleri veritabanına kaydet

                return newEmployee;
            }

            // Eğer veri bulunursa mevcut RemoteEmployee nesnesini döndür
            return result;
        }

        public IDataResult<List<LateEmployeeGroupDto>> GetLates(DateTime startDate, DateTime endDate,int year)
        {
            var result = _employeeDal.GetLates(startDate, endDate, year);
            
            if (result != null && result.Data.Count > 0)
            {
                foreach (var group in result.Data)
                {
                    string message = GetMessageForProcessTemp(group.ProcessTemp);
                    group.Message = message;
                }
                return new SuccessDataResult<List<LateEmployeeGroupDto>>(result.Data, "Geç Kalanlar Listelendi");
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

            if (result != null && result.Data.Count > 0)
            {
                foreach (var group in result.Data)
                {
                    string message = GetMessageForProcessTemp(group.ProcessTemp);
                    group.Message = message;
                }
                return new SuccessDataResult<List<LateEmployeeGroupDto>>(result.Data, "Geç Kalanlar Listelendi");
            }
            return new ErrorDataResult<List<LateEmployeeGroupDto>>("Geç kalan veya 11.30 saatten az çalışan çalışan yok");
        }


        public IDataResult<List<LateEmployeeGroupDto>> GetLatesWithDepartment(DateTime startDate, DateTime endDate, int year, string[] Department)
        {
            var result = _employeeDal.GetLatesWithDepartment(startDate, endDate, year, Department);

            if (result != null && result.Success && result.Data != null && result.Data.Count > 0)
            {
                foreach (var group in result.Data)
                {
                    string message = GetMessageForProcessTemp(group.ProcessTemp);
                    group.Message = message;
                }

                return new SuccessDataResult<List<LateEmployeeGroupDto>>(result.Data, "Geç Kalanlar Departmana göre Listelendi");
            }

            if (Department.Contains("0"))
            {
                var allResult = _employeeDal.GetLates(startDate, endDate, year);

                if (allResult != null && allResult.Success && allResult.Data != null && allResult.Data.Count > 0)
                {
                    foreach (var group in allResult.Data)
                    {
                        string message = GetMessageForProcessTemp(group.ProcessTemp);
                        group.Message = message;
                    }

                    return new SuccessDataResult<List<LateEmployeeGroupDto>>(allResult.Data, "Tüm Geç Kalanlar Listelendi");
                }
            }

            return new ErrorDataResult<List<LateEmployeeGroupDto>>("İlgili departmana ait çalışan bulunamadı");
        }

        public IDataResult<List<PersonalNameWithDepartmentDto>> GetAllDepartmentEmp(string department)
        {
            var result = _employeeDal.GetNameWithDepartments(department);

            if (result != null && result.Data.Count > 0)
            {
                return new SuccessDataResult<List<PersonalNameWithDepartmentDto>>(result.Data, "Departman çalışanları listelendi");
            }

            return new ErrorDataResult<List<PersonalNameWithDepartmentDto>>("Departman çalışanları listelenemedi");
        }

        
    }
}
