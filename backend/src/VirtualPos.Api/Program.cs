using VirtualPos.Application.Installments;
using VirtualPos.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var connectionString = builder.Configuration.GetConnectionString("VirtualPosDb")
    ?? throw new InvalidOperationException("'VirtualPosDb' bağlantı cümlesi bulunamadı.");
var apiBaseUrl = builder.Configuration["App:ApiBaseUrl"]
    ?? throw new InvalidOperationException("'App:ApiBaseUrl' ayarı bulunamadı.");
builder.Services.AddInfrastructure(connectionString, apiBaseUrl);
builder.Services.AddScoped<InstallmentService>();

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();

app.Run();
