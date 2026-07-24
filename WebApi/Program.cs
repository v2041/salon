using Features.Appointments;
using Features.Offerings;
using Features.Schedules;
using Features.Users;
using FluentValidation;
using Infrastructure.BackgroundServices;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;
using WebApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidatorsFromAssembly(typeof(AppointmentsModule).Assembly);
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(SalonDbContext)))
);
builder.Services.AddHostedService<ScheduleService>();
var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapUsersEndpoints();
app.MapAppointmentsEndpoints();
app.MapOfferingsEndpoints();
app.MapSchedulesEndpoints();
app.UseHttpsRedirection();
app.Run();