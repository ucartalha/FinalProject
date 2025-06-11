using Entities.Concrete;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataAccess.Concrete
{
    public class InputContext : DbContext
    {
        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=InputProject;TrustServerCertificate=True;Trusted_Connection=true;MultipleActiveResultSets=true", options=>options.MigrationsAssembly("DataAccess"));
        }

        public DbSet<EmployeeRecord> EmployeeRecords { get; set; }
        public DbSet<RemoteWorkEmployee> RemoteWorkEmployees { get; set; }
        public DbSet<UploadedFile> UploadedFiles { get; set; }
        public DbSet<Personal> Personal { get; set; }
        public DbSet<OverShift> OverShifts { get; set; }
        //public DbSet<RemoteEmployeeDto> RemoteEmployeeDto { get; set; }
        //public DbSet<InfoEmployeeDto> InfoEmployeeDto { get; set; }
       
        public DbSet<RemoteEmployee> EmployeeDtos { get; set; }
        public DbSet<ReaderDataDto> ReaderDataDtos { get; set; }

        
    }
}
