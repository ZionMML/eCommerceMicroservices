using eCommerce.ProductsService.DataAccessLayer;
using eCommerce.ProductsService.BusinessLogicLayer;
using FluentValidation.AspNetCore;
using eCommerce.ProductsMicroService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();

builder.Services.AddControllers();

// FluentValidations
builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();

// Auth
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
