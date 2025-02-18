using ClubPadel.DL;
using ClubPadel.DL.EfCore;
using ClubPadel.Models;
using ClubPadel.Services;
using Microsoft.EntityFrameworkCore;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.WithOrigins("http://localhost:3000", "https://7880-18-216-133-68.ngrok-free.app", "https://black-dune-0dbad6403.4.azurestaticapps.net/")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


//builder.Services.AddSingleton<IBaseRepository<Event>>(sp =>
//    new EventRepository("eventRegistration.db", "events"));
builder.Services.AddScoped<IBaseRepository<Event>, EventSqlRepository>();
builder.Services.AddScoped<EventSqlRepository>();
builder.Services.AddScoped<ParticipantSqlRepository>();
builder.Services.AddScoped<EventService>();

builder.Services.AddHttpClient();
builder.Services.AddScoped<ITelegramBotClient>(provider =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("TgBotToken")));
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();


builder.Configuration.SetBasePath(Directory.GetCurrentDirectory());
builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new Exception("Database connection string is missing in appsettings.json");
}

// Register EF Core with SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString), ServiceLifetime.Scoped);



var app = builder.Build();
app.UseCors("AllowVueApp");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.Run();
