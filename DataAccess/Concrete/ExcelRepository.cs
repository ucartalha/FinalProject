using Core.Entities;
using Core.Utilites.Helpers;
using DataAccess.Abstract;
using Entities.Concrete;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class ExcelRepository<TEntity> : IExcelRepository<TEntity> where TEntity : class, IEntity, new()
    {
        private readonly InputContext _context;
        private readonly DbSet<TEntity> _tableSet;

        public ExcelRepository(InputContext context)
        {
            _context = context;
            _tableSet = _context.Set<TEntity>();
        }

        public List<TEntity> GetAll(Expression<Func<TEntity, bool>> filter = null)
        {
            IQueryable<TEntity> query = _tableSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.ToList();
        }

        public TEntity Get(Expression<Func<TEntity, bool>> filter)
        {
            IQueryable<TEntity> query = _tableSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            return query.FirstOrDefault();
        }

        public void Add(TEntity entity)
        {
            _tableSet.Add(entity);
            _context.SaveChanges();
        }

        public void Update(TEntity entity)
        {
            _tableSet.Update(entity);
            _context.SaveChanges();
        }

        public void Delete(TEntity entity)
        {
            _tableSet.Remove(entity);
            _context.SaveChanges();
        }
        

        public List<TEntity> ProcessExcelFile(IFormFile file)
        {
            
            if (file!=null&&file.Length>0)
            {
                string hashString=HashHelper.ComputeFileHash(file);
            }
            using (var stream = new MemoryStream())
            {
                file.CopyTo(stream);
                stream.Position = 0;

                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    var records = new List<TEntity>();
                    var uploadedFiles = new List<UploadedFile>();
                    while (reader.Read())
                    {
                        try
                        {
                            var record = (TEntity)Activator.CreateInstance(typeof(TEntity));

                            // Verileri Excel'den okuyarak nesnelere dönüştürme
                            if (record is EmployeeRecord employeeRecord)
                            {
                                employeeRecord.ID = Convert.ToInt32(reader.GetValue(0));
                                employeeRecord.CardId = Convert.ToInt32(reader.GetValue(1));
                                employeeRecord.Name = reader.GetString(2);
                                employeeRecord.SurName = reader.GetString(3);
                                employeeRecord.Sirket = reader.GetString(4);
                                employeeRecord.Department = reader.GetString(5);
                                employeeRecord.blok = reader.GetString(6);
                                //employeeRecord.Date = reader.GetDateTime(7);
                                employeeRecord.FirstRecord = reader.GetDateTime(8);
                                employeeRecord.LastRecord = reader.GetDateTime(9);
                                employeeRecord.WorkingHour = TimeSpan.TryParse(reader.GetValue(10)?.ToString(), out var workingHour) ? workingHour : TimeSpan.Zero;
                                //employeeRecord.WorkingHour = reader.GetDateTime(10);
                                // Diğer özelliklerin doldurulması
                            }
                            else if (record is VpnEmployee remoteWorkEmployee)
                            {
                                var userValue = reader.GetValue(2);
                                if (userValue != null)
                                {
                                    var fullName = userValue.ToString().Split('.');
                                    remoteWorkEmployee.FirstName = fullName.Length > 0 ? fullName[0] : "";
                                    remoteWorkEmployee.LastName = fullName.Length > 1 ? fullName[1] : "";
                                }
                                remoteWorkEmployee.LogDate = reader.GetDateTime(0);
                                //remoteWorkEmployee.Remip = reader.GetValue(1)?.ToString() ?? string.Empty;

                                //remoteWorkEmployee.Group = reader.GetValue(3)?.ToString() ?? string.Empty;
                                //remoteWorkEmployee.BytesOut = int.TryParse(reader.GetValue(4)?.ToString(), out var bytesOut) ? bytesOut : 0;
                                //remoteWorkEmployee.BytesIn = int.TryParse(reader.GetValue(5)?.ToString(), out var bytesIn) ? bytesIn : 0;
                                //remoteWorkEmployee.Duration = int.TryParse(reader.GetValue(6).ToString(), out var duration) ? duration : 0;
                                //remoteWorkEmployee.Msg = reader.GetValue(7)?.ToString() ?? string.Empty;
                                //remoteWorkEmployee.Reason = reader.GetValue(8)?.ToString() ?? string.Empty;
                                //remoteWorkEmployee.TunnelIp = reader.GetValue(9)?.ToString() ?? string.Empty;
                                // Diğer özelliklerin doldurulması
                            }

                            // Nesneyi listeye ekleme
                            records.Add(record);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine(ex.Message);
                        }
                    }

                    _tableSet.AddRange(records);
                    _context.SaveChanges();

                    return records;
                }
            }
        }
    }

}