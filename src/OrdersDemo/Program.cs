﻿using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.EntityFrameworkCore;
using OrdersDemo.Api.Handlers;
using OrdersDemo.Domain.Contracts;
using OrdersDemo.Domain.Services;
using OrdersDemo.Infrastructure;
using OrdersDemo.Infrastructure.Mail;
using OrdersDemo.Setup;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

var logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

builder.Services.Configure<MailConfig>(builder.Configuration.GetSection(MailConfig.CONFIG_NAME));

var connectionString = builder.Configuration.GetConnectionString("OrderDbConnection");
builder.Services.AddDbContext<OrderDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddScoped<OrderDbInitializer>();

builder.Services.AddSingleton<IDateTime, DateTimeProvider>();
builder.Services.AddScoped<IDocumentNoGenerator, DocumentNoGenerator>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddSingleton<IOrderCalculator, OrderCalculator>();

builder.Services.AddSingleton<IPriceCalculator, CustomerStandardCalculator>(s => CustomerStandardCalculator.Instance);
builder.Services.AddSingleton<IPriceCalculator, CustomerPremiumCalculator>(s => CustomerPremiumCalculator.Instance);
builder.Services.AddSingleton<IPriceCalculator, CustomerVipCalculator>(s => CustomerVipCalculator.Instance);
builder.Services.AddSingleton<IPriceCalculator, CustomerPresidentCalculator>(s => CustomerPresidentCalculator.Instance);

builder.Services.AddScoped<OrderCompleteHandler>();
builder.Services.AddScoped<OrderCreateHandler>();
builder.Services.AddScoped<OrderDeleteHandler>();
builder.Services.AddScoped<OrderGetAllHandler>();
builder.Services.AddScoped<OrderGetHandler>();
builder.Services.AddScoped<OrderItemCreateHandler>();
builder.Services.AddScoped<OrderItemDeleteHandler>();
builder.Services.AddScoped<OrderUpdateHandler>();

builder.Services.AddAutoMapper(typeof(Program).Assembly);

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(x =>
{
    x.EnableAnnotations();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.ConfigureExceptionHandler();
}

app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

await OrderDbInitializer.SeedAsync(app.Services);

app.Run();
