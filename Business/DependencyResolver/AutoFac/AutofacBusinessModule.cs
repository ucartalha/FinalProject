using Autofac;
using Autofac.Extras.DynamicProxy;
using Business.Abstract;
using Business.Concrete;
using Castle.DynamicProxy;
using Core.Entities;
using Core.Utilites.Interceptors;
using Core.Utilities.Helpers.FileHelper;
using DataAccess.Abstract;
using DataAccess.Concrete;
using Entities.Concrete;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Business.DependencyResolver.AutoFac
{
    public class AutofacBusinessModule:Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<EmployeeRecordManager>().As<IEmployeeRecordService>().SingleInstance();
            builder.RegisterType<EfEmployeeRecordDal>().As<IEmployeeRecordDal>().SingleInstance();
            
            builder.RegisterType<RemoteWorkEmployeeManager>().As<IRemoteWorkEmployeeService>().SingleInstance();
            builder.RegisterType<EfRemoteEmployeeDal>().As<IRemoteEmployee>().SingleInstance();
            
            builder.RegisterType<UploadFileManager>().As<IUploadFileService>().SingleInstance();
            builder.RegisterType<EfUploadedFileDal>().As<IUploadedFilesDal>().SingleInstance();
            
            builder.RegisterType<PersonalManager>().As<IPersonalService>().SingleInstance();
            builder.RegisterType<EfPersonalDal>().As<IPersonalDal>().SingleInstance();
            
            builder.RegisterType<OverShiftManager>().As<IOverShifService>().SingleInstance();
            builder.RegisterType<EfOverShiftDal>().As<IOverShiftDal>().SingleInstance();

            builder.RegisterType<InputContext>().AsSelf();

            builder.RegisterType<FileHelperManager>().As<IFileHelper>().SingleInstance();
            builder.RegisterType<DbContext>().AsSelf().InstancePerLifetimeScope();
            var optionsBuilder = new DbContextOptionsBuilder<DbContext>();
            optionsBuilder.UseSqlServer(@"Server=1015TUCA;Database=InputProject;Trusted_Connection=true");
            var dbContextOptions = optionsBuilder.Options;
            builder.RegisterGeneric(typeof(ExcelRepository<>)).As(typeof(IExcelRepository<>));
            var assembly = System.Reflection.Assembly.GetExecutingAssembly();
            builder.RegisterInstance(dbContextOptions).As<DbContextOptions>();
            builder.RegisterAssemblyTypes(assembly).AsImplementedInterfaces()
                .EnableInterfaceInterceptors(new ProxyGenerationOptions()
                {
                    Selector = new AspectInterceptorSelector()
                }).SingleInstance();
        }
    }
}
