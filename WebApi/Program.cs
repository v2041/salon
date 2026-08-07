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
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
builder.Services.AddValidatorsFromAssembly(typeof(AppointmentsModule).Assembly);
builder.Services.AddOpenApi();
builder.Services.AddDbContext<SalonDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString(nameof(SalonDbContext)))
);
builder.Services.AddHostedService<ScheduleService>();
var app = builder.Build();
app.UseCors();
app.UseMiddleware<ExceptionHandlingMiddleware>();
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

//app.MapUsersEndpoints();
app.MapAppointmentsEndpoints();
app.MapOfferingsEndpoints();
app.MapSchedulesEndpoints();
//app.UseHttpsRedirection();
app.Run();