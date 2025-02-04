using DoctorAppointment.Repositories;
using DoctorAppointment.Services;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; // IConfiguration buradan alınır

// MongoDB konfiqurasiyası
builder.Services.AddSingleton<IMongoClient>(sp =>
{
    var connectionString = configuration.GetValue<string>("MongoDb:Uri");
    return new MongoClient(connectionString);
});

// Xidmətlərin DI konfiqurasiyası
builder.Services.AddSingleton<CloudinaryService>();
builder.Services.AddSingleton<MongoDbService>();
builder.Services.AddSingleton<AdminService>();
builder.Services.AddSingleton<IAdminRepository, AdminRepository>();


var frontendUrl = configuration.GetValue<string>("frontend_url");
var adminUrl = configuration.GetValue<string>("admin_url");

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontendAndAdmin", policyBuilder =>
    {
        policyBuilder.SetIsOriginAllowed(origin =>
            origin == frontendUrl || origin == adminUrl)
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});

builder.Services.AddControllersWithViews();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseCors("AllowFrontendAndAdmin"); 

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
