using Nacos.AspNetCore.V2;

var builder = WebApplication.CreateBuilder(args);

var enableConfig = bool.Parse(builder.Configuration["EnableConfig"]);
var enableNaming = bool.Parse(builder.Configuration["EnableNaming"]);

if (enableConfig)
{
    builder.Host.UseNacosConfig("NacosConfig");
    Console.WriteLine("enable config.");
}

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

if (enableNaming)
{
    builder.Services.AddNacosAspNet(builder.Configuration);
    Console.WriteLine("enable naming.");
}

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();
