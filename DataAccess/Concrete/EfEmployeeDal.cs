using Core.DataAccess.EntityFramework;
using Core.Utilites.Results;
using DataAccess.Abstract;
using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataAccess.Concrete
{
    public class EfEmployeeDal : EfEntityRepositoryBase<Personnal, InputContext>, IEmployeeDal
    {
        public IDataResult<EmployeeDto> GetDepartmanEmployeeId(int id)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var entity = db.Personnals.FirstOrDefault(b => b.Id == id);
                    if (entity == null)
                        return new ErrorDataResult<EmployeeDto>("Çalışan bulunamadı.");

                    var dto = MapToDto(entity);
                    return new SuccessDataResult<EmployeeDto>(dto);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<EmployeeDto>("Hata oluştu: " + ex.Message);
            }
        }

        public IDataResult<List<EmployeeDto>> AllEmployeeDepartmantUpdate()
        {
            try
            {
                using (var db = new InputContext())
                {
                    var employees = this.GetAll();
                    var employeeDtos = new List<EmployeeDto>();

                    foreach (var item in employees)
                    {
                         var dto = MapToDto(item);
                        dto.Department = item.DepartmentId != null && item.DepartmentId != 0
                            ? db.Departments.FirstOrDefault(x => x.Id == item.DepartmentId)?.Name ?? "Bilinmeyen"
                            : "Bilinmeyen";

                        employeeDtos.Add(dto);
                    }

                    return new SuccessDataResult<List<EmployeeDto>>(employeeDtos);
                }
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<EmployeeDto>>("Hata: " + ex.Message);
            }
        }

        public IDataResult<List<EmployeeDto>> EmployeeDepartmantUpdate()
        {
            try
            {
                var personnels = this.GetAll(x => x.DepartmentId == 0 || x.DepartmentId == null);
                var dtoList = personnels.Select(e => MapToDto(e)).ToList();
                return new SuccessDataResult<List<EmployeeDto>>(dtoList);
            }
            catch (Exception ex)
            {
                return new ErrorDataResult<List<EmployeeDto>>("Hata: " + ex.Message);
            }
        }

        public IResult UpdateDepartman(EmployeeDto dto)
        {
            try
            {
                using (var db = new InputContext())
                {
                    var data = db.Personnals.Where(x => x.Id == dto.Id).ToList();
                    var officeData = db.EmployeeRecords.Where(x => x.RemoteEmployeeId == dto.Id).ToList();
                    var vpnData = db.FinalVpnEmployees.Where(x => x.RemoteEmployeeId == dto.Id).ToList();
                    var departman = db.Departments.FirstOrDefault(x => x.Id == dto.DepartmentId);

                    if (data.Count > 0 && departman != null)
                    {
                        data.ForEach(d => d.DepartmentId = departman.Id);
                        officeData.ForEach(d => d.DepartmentId = departman.Id);
                        vpnData.ForEach(d => d.DepartmentId = departman.Id);

                        if (db.SaveChanges() > 0)
                            return new SuccessResult("Departman güncellendi.");
                    }

                    return new ErrorResult("Güncelleme başarısız.");
                }
            }
            catch (Exception ex)
            {
                return new ErrorResult("Hata oluştu: " + ex.Message);
            }
        }

        public IResult Add(EmployeeDto dto)
        {
            if (dto == null)
                return new ErrorResult("Çalışan bilgisi boş olamaz.");

            if (IsEmployeeExist(dto.UserName))
                return new ErrorResult("Bu kullanıcı adı zaten mevcut.");

            var entity = new Personnal
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                DepartmentId = dto.DepartmentId,
                Email = dto.Email,
            };
            this.Add(entity);

            base.Add(entity); // base class üzerinden erişim

            return new SuccessResult("Çalışan başarıyla eklendi.");
        }

        public List<DepartmentDto> GetDepartments()
        {
            using (var db = new InputContext())
            {
                return db.Departments
                         .Select(d => new DepartmentDto
                         {
                             Id = d.Id,
                             Name = d.Name
                         })
                         .ToList();
            }
        }


        public IResult CheckEmployeeExists(string userName)
        {
            if (string.IsNullOrWhiteSpace(userName))
                return new ErrorResult("Kullanıcı adı boş olamaz.");

            var result = this.Get(x => x.UserName == userName);
            if (result != null)
                return new SuccessResult("Çalışan mevcut.");

            return new ErrorResult("Çalışan bulunamadı.");
        }

        private bool IsEmployeeExist(string userName)
        {
            if (string.IsNullOrEmpty(userName))
                return true;

            var result = this.Get(x => x.UserName == userName);
            return result != null;
        }
        public Personnal MapToEntity(EmployeeDto dto)
        {
            return new Personnal
            {
                Id = dto.Id,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                UserName = dto.UserName,
                Email = dto.Email,
                DepartmentId = dto.DepartmentId
            };
        }
        private EmployeeDto MapToDto(Personnal entity)
        {
            return new EmployeeDto
            {
                Id = entity.Id,
                FirstName = entity.FirstName,
                LastName = entity.LastName,
                UserName = entity.UserName,
                Email = entity.Email,
                DepartmentId = entity.DepartmentId
            };
        }



    }
}
