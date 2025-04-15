using ClubPadel.DL;
using ClubPadel.DL.EfCore;
using ClubPadel.Models;
using ClubPadel.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel((context, options) =>
{
    options.Configure(context.Configuration.GetSection("Kestrel"));
});

builder.Services.AddLogging();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.WithOrigins("http://localhost:3000", "https://5023-94-204-215-144.ngrok-free.app", "https://black-dune-0dbad6403.4.azurestaticapps.net/")
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .SetIsOriginAllowed(origin => true));
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            //ValidIssuer = "your-issuer",
            //ValidAudience = "your-audience",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(Environment.GetEnvironmentVariable("TgBotToken")))
        };
        options.TokenValidationParameters.ValidateIssuer = false;
        options.TokenValidationParameters.ValidateAudience = false;
    });
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("admin"));
});


//builder.Services.AddSingleton<IBaseRepository<Event>>(sp =>
//    new EventRepository("eventRegistration.db", "events"));
builder.Services.AddScoped<IBaseRepository<Event>, EventSqlRepository>();
builder.Services.AddScoped<EventSqlRepository>();
builder.Services.AddScoped<ParticipantSqlRepository>();
builder.Services.AddScoped<UserRepository>();
builder.Services.AddScoped<EventService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<ITelegramBotClient>(provider =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("TgBotToken")));
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();


builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
//builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


var connectionString = Environment.GetEnvironmentVariable("DefaultConnection");//builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Database connection string is missing in appsettings.json");
}

// Register EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    //options.UseSqlServer(connectionString), ServiceLifetime.Scoped);
    options.UseNpgsql(connectionString), ServiceLifetime.Scoped);



var app = builder.Build();
app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
