using System.Text;
using Domain.Exceptions;
using Domain.ValueObjects;
using Features.Analytics;
using Features.Appointments;
using Features.Offerings;
using Features.Schedules;
using Features.Settings;
using Features.Users;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);
if (builder.Environment.IsDevelopment())
{
    builder.Logging.ClearProviders();
    builder.Logging.AddConsole();
    builder.Logging.AddDebug();
}
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:5173", "http://localhost:3000"];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
builder.Services.AddValidatorsFromAssembly(typeof(AppointmentsModule).Assembly);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(SalonDbContext)))
);
var vkId = builder.Configuration.GetSection(VkIdOptions.SectionName);
var vkAppId = vkId["AppId"];
var vkServiceToken = vkId["ServiceToken"];
var adminPhone = builder.Configuration["Admin:Phone"];
if (!string.IsNullOrWhiteSpace(adminPhone))
{
    try
    {
        PhoneNumber.Create(adminPhone);
    }
    catch (BusinessException)
    {
        throw new InvalidOperationException("Admin:Phone имеет неверный формат. Задайте ADMIN_PHONE в формате +7XXXXXXXXXX.");
    }
}
builder.Services.AddHttpClient(VkIdClient.HttpClientName, client =>
    client.Timeout = TimeSpan.FromSeconds(VkIdClient.HttpClientTimeoutSeconds));
builder.Services.AddTransient(sp => new VkIdClient(
    sp.GetRequiredService<IHttpClientFactory>().CreateClient(VkIdClient.HttpClientName),
    new VkIdOptions { AppId = vkAppId, ServiceToken = vkServiceToken },
    sp.GetRequiredService<ILogger<VkIdClient>>()));
var jwt = builder.Configuration.GetSection("Jwt");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwt["Key"]!)),
            ClockSkew = TimeSpan.Zero
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("some-cookies", out var token))
                    context.Token = token;
                return Task.CompletedTask;
            }
        };
    });
builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", p => p.RequireRole("Admin"))
    .AddPolicy("User", p => p.RequireAuthenticatedUser());
builder.Services.AddScoped<ScheduleGenerationRunner>();
builder.Services.AddHostedService<ScheduleService>();
var app = builder.Build();
// Генерация docs/openapi.json при dotnet build стартует приложение в Production.
var isOpenApiGeneration = Environment.GetCommandLineArgs()[0]
    .Contains("GetDocument.Insider", StringComparison.OrdinalIgnoreCase);
if (!app.Environment.IsDevelopment() && !isOpenApiGeneration)
{
    if (string.IsNullOrWhiteSpace(vkAppId) || string.IsNullOrWhiteSpace(vkServiceToken))
        throw new InvalidOperationException("VkId:AppId или VkId:ServiceToken не заданы. Задайте VK_APP_ID и VK_SERVICE_TOKEN.");
    if (string.IsNullOrWhiteSpace(adminPhone))
        throw new InvalidOperationException("Admin:Phone не задан. Задайте ADMIN_PHONE.");
}
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapAppointmentsEndpoints();
app.MapOfferingsEndpoints();
app.MapSchedulesEndpoints();
app.MapSettingsEndpoints();
app.MapUsersEndpoints();
app.MapAnalyticsEndpoints();
app.Run();
