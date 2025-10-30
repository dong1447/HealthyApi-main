using HealthyApi;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// ===================== Database Connection =====================
builder.Services.AddDbContext<DataContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// ===================== Controller and JSON Naming =====================
builder.Services.AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower;
        o.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.SnakeCaseLower;
    });

// ===================== CORS Configuration =====================
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// ✅ 必须放在 MapControllers() 之前
app.UseCors("AllowAll");

// ✅ 你之前 **缺少这句**
app.UseAuthorization();

app.MapControllers();

app.Run();

