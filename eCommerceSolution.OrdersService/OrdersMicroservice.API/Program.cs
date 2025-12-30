using eCommerce.OrdersMicroservice.BusinessLogicLayer;
using eCommerce.OrdersMicroservice.BusinessLogicLayer.Policies;
using eCommerce.OrdersMicroservice.DataAccessLayer;
using eCommerce.UsersMicroservice.BusinessLogicLayer.HttpClients;
using FluentValidation.AspNetCore;
using OrdersMicroservice.API.Middleware;
using Polly;

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
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddTransient<IUsersMicroservicePolicies, UsersMicroservicePolicies>();

builder.Services.AddHttpClient<UsersMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
        $"{builder.Configuration["UsersMicroserviceName"]}:" +
        $"{builder.Configuration["UsersMicroservicePort"]}");
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
    return policies.GetRetryPolicy();
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
    return policies.GetCircuitBreakerPolicy();
});


builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
        $"{builder.Configuration["ProductsMicroserviceName"]}:" +
        $"{builder.Configuration["ProductsMicroservicePort"]}");
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
// Comment out or remove this in Development
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

app.Run();
