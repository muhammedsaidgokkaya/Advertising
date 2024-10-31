using Core.Data;
using Microsoft.EntityFrameworkCore;
using Repository.Implementations;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Default Connection bulunmamaktadýr.");
}
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(Repository<>));
Utilities.Helper.Sql.Initialize(connectionString);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();
