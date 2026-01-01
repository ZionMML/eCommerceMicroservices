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
builder.Services.AddTransient<IProductsMicroservicePolicies, ProductsMicroservicePolicies>();

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
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IUsersMicroservicePolicies>();
    return policies.GetTimeoutPolicy();
});


builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
        $"{builder.Configuration["ProductsMicroserviceName"]}:" +
        $"{builder.Configuration["ProductsMicroservicePort"]}");
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IProductsMicroservicePolicies>();
    return policies.GetFallbackPolicy();
})
.AddPolicyHandler((serviceProvider, request) =>
{
    var policies = serviceProvider.GetRequiredService<IProductsMicroservicePolicies>();
    return policies.GetBulkheadIsolationPolicy();
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
