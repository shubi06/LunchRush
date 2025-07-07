using Dapr;
using Dapr.Client;
using Microsoft.AspNetCore.Mvc;
using LunchRush.API.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers().AddDapr()
    .AddJsonOptions(opts =>
    {
        opts.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
    });
});

builder.Services.AddDaprClient(clientBuilder =>
{
    var grpcAddress = Environment.GetEnvironmentVariable("DAPR_GRPC_ADDRESS") ?? "http://lunchrush-api-dapr:50001";
    clientBuilder.UseGrpcEndpoint(grpcAddress);
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
// Configure the HTTP request pipeline
app.UseCors("AllowAll");
app.UseCloudEvents();
app.UseAuthorization();
app.MapControllers();
app.MapSubscribeHandler();

app.Run();