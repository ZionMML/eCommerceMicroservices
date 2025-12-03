using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cors
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
       policy.WithOrigins("https://localhost:4200")
             .AllowAnyHeader()
             .AllowAnyMethod();
    });
});

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
        $"{builder.Configuration["UsersMicroserviceName"]}:" +
        $"{builder.Configuration["UsersMicroservicePort"]}");
});

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

// Cors
app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

app.Run();
