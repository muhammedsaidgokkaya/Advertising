using Core.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Repository.Implementations;
using Service.Implementations;
using Service.Implementations.Calendar;
using Service.Implementations.Chat;
using Service.Implementations.Google;
using Service.Implementations.Meta;
using Service.Implementations.Report;
using Service.Implementations.Task;
using Service.Implementations.User;
using System.Text;
using Utilities.Utilities.GoogleData;
using Utilities.Utilities.MetaData;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader());
});

builder.Services.AddControllersWithViews();
builder.Services.AddSingleton<MetaData>();
builder.Services.AddSingleton<GoogleData>();
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Default Connection bulunmamaktadýr.");
}
builder.Services.AddDbContext<Context>(options => options.UseNpgsql(connectionString));

builder.Services.AddAuthentication("Bearer")
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdmin", policy => policy.RequireRole("SuperAdmin"));
    options.AddPolicy("Admin", policy => policy.RequireRole("Admin"));
    options.AddPolicy("User", policy => policy.RequireRole("User"));
    options.AddPolicy("UserSession", policy => policy.RequireRole("UserSession"));
    options.AddPolicy("MetaAdmin", policy => policy.RequireRole("MetaAdmin"));
    options.AddPolicy("GoogleAdmin", policy => policy.RequireRole("GoogleAdmin"));
    options.AddPolicy("MetaView", policy => policy.RequireRole("MetaView"));
    options.AddPolicy("GoogleView", policy => policy.RequireRole("GoogleView"));
    options.AddPolicy("TaskAdmin", policy => policy.RequireRole("TaskAdmin"));
});

builder.Services.AddHttpClient();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<MetaService>();
builder.Services.AddScoped<Meta>();
builder.Services.AddScoped<GoogleService>();
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<TaskService>();
builder.Services.AddScoped<ChatService>();
builder.Services.AddScoped<CalendarService>();
builder.Services.AddScoped(typeof(Repository<>));
builder.Services.AddScoped<JwtService>();
Utilities.Helper.Sql.Initialize(connectionString);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAllOrigins");

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
