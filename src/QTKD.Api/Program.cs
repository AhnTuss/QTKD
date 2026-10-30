using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using QTKD.Api.Data;
using QTKD.Api.Services;

var builder = WebApplication.CreateBuilder(args);

// Chuỗi kết nối KHÔNG chứa mật khẩu trong appsettings.json.
// Mật khẩu cấu hình bằng user-secrets hoặc biến môi trường (xem README).
var connectionString = builder.Configuration.GetConnectionString("QtkdDb")
    ?? throw new InvalidOperationException("Thiếu ConnectionStrings:QtkdDb. Xem README, mục cấu hình database.");

builder.Services.AddDbContext<QtkdDbContext>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IContractService, ContractService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();

    await DbSeeder.SeedAsync(app.Services);
}

app.MapControllers();

app.Run();
