using System.Xml.Linq;
using EmployeeWebApi.Endpoints;
using EmployeeWebApi.Infrastructure;
using Intergalactic.Azure.Cosmos;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();

ICosmosRepositoryBuilder intergalacticAzureCosmosBuilder = builder
    .Services
    .AddIntergalacticAzureCosmos(builder.Configuration);

intergalacticAzureCosmosBuilder.AddItemConfiguration<EmployeeItem, EmployeeItemConfiguration>();

WebApplication app = builder.Build();


app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("../swagger/v1/swagger.json", "Employee API");
    options.RoutePrefix = string.Empty;
});

app.MapEmployeeEndpoints();

app.Run();
