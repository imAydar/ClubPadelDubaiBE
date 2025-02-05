using ClubPadel.DL;
using ClubPadel.Models;
using ClubPadel.Services;
using Telegram.Bot;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowVueApp",
        policy => policy.WithOrigins("http://localhost:3000", "https://7880-18-216-133-68.ngrok-free.app", "https://clubpadeldubaibe-f5heg2ajhed7fqcv.uaenorth-01.azurewebsites.net")
                        .AllowAnyMethod()
                        .AllowAnyHeader());
});


builder.Services.AddSingleton<IBaseRepository<Event>>(sp =>
    new EventRepository("eventRegistration.db", "events"));

builder.Services.AddSingleton<EventService>();

builder.Services.AddHttpClient();
builder.Services.AddSingleton<ITelegramBotClient>(provider =>
    new TelegramBotClient(Environment.GetEnvironmentVariable("TgBotToken")));
builder.Services.AddScoped<ITelegramBotService, TelegramBotService>();



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
