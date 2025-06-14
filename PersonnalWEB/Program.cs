using Autofac.Extensions.DependencyInjection;
using Autofac;
using Business.Abstract;
using Business.Concrete;
using Business.DependencyResolver.AutoFac;

var builder = WebApplication.CreateBuilder(args);


builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

// Bu satýr: yazdýðýn AutofacBusinessModule'u yüklüyor.
builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
{
    containerBuilder.RegisterModule(new AutofacBusinessModule());
});
// Add services to the container.

builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStatusCodePagesWithReExecute("/Error/{0}");
// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
}
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
