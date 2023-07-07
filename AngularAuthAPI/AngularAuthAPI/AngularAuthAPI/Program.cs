using AngularAuthAPI.Context;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(option =>
{
   option.AddPolicy("MyPolicy", builder =>
    {
       builder.AllowAnyOrigin()
        .AllowAnyHeader()
        .AllowAnyMethod();
         
    });
});

builder.Services.AddDbContext<AppDbContext>(
    options =>
    {
        options.UseMySql(builder.Configuration.GetConnectionString("MySqlConnStr"),
            new MySqlServerVersion(new Version(8, 0, 11)));
    
            

    });





var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("MyPolicy");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
