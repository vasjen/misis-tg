using Microsoft.EntityFrameworkCore;
using misis_tg.Builder;
using misis_tg.Data;
using misis_tg.Models;
using misis_tg.Services;
using Telegram.Bot;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
IConfigurationRoot configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .AddUserSecrets<Program>()
    .Build();
builder.Services.AddScoped<ITelegramBotClient, TelegramBotClient>(cfg =>
{
    var token = builder.Configuration["Telegram:Token"] ?? throw new ArgumentNullException("Token is not specified");
    return new TelegramBotClient(token);
});
builder.Services.AddScoped<ParserService>();
builder.Services.AddScoped<InitialService>();
builder.Services.AddScoped<EnrolledService>();
builder.Services.AddScoped<EducationBuilder>();
builder.Services.AddScoped<NotificationService>();
builder.Services.AddScoped<SubscriptionService>();
builder.Services.AddSingleton<ITelegramBotClient,TelegramBotClient>( p=>
{
    string? token = configuration.GetValue<string>("Telegram:Token");
    return new TelegramBotClient(token: token ?? throw new ArgumentNullException("Token is not specified"));
});
builder.Services.AddSingleton<UrlsConfig>((cfg) =>
{
    var urls = builder.Configuration.GetSection("Education:Urls").GetChildren().ToDictionary(p => p.Key, p => p.Value);
    return new UrlsConfig { UrlList = urls };
});
builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration.GetConnectionString("SqliteConnection")));
builder.Services.AddHostedService<UpdaterWorkerService>();
builder.Services.AddHostedService<TelegramListener>();

builder.Services.AddHttpClient("misis",client =>
{ 
    client.BaseAddress = new Uri("https://misis.ru/applicants/admission/progress/"); 
});

WebApplication app = builder.Build();
using IServiceScope scope = app.Services.CreateScope();
InitialService init = scope.ServiceProvider.GetRequiredService<InitialService>();
await init.InitDb();
app.MapGet("/",  () => "Hello World!");
app.Run();