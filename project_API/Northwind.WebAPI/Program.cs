using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Northwind.Application.Intefaces;
using Northwind.Application.Services;
using Northwind.Infrastructure.Persistence;
using Northwind.Infrastructure.Repositories;
using Northwind.WebAPI.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<NorthwindDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.AllowAnyOrigin()
            .AllowAnyHeader()
            .AllowAnyMethod());
});

builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerRepository, CustomerRepository>();
builder.Services.AddScoped<ICustomerService, CustomerService>();
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddHttpClient();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Northwind OMS API",
        Version = "v1",
        Description = "Northwind order management endpoints"
    });
});

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Ok(new
{
    status = "ok",
    message = "Northwind OMS API running",
    docs = "/swagger",
    openapi = "/swagger/v1/swagger.json",
    orders = "/api/orders",
    customers = "/api/customers",
    employees = "/api/employees",
    products = "/api/products"
}));

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.UseCors("Frontend");
app.MapControllers();

app.Run();