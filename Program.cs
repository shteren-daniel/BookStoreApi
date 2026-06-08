using BookStoreApi.Middleware;
using BookStoreApi.Repositories;
using BookStoreApi.Services;
using BookStoreApi.Validators;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<CreateBookValidator>();

builder.Services.AddScoped<IBookRepository, BookRepository>();
builder.Services.AddScoped<IBookService, BookService>();

var app = builder.Build();

app.UseGlobalExceptionHandler();
app.MapControllers();
app.UseSwagger();
app.UseSwaggerUI();
app.Run();