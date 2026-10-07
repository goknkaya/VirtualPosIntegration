using Scalar.AspNetCore;
using VirtualPos.Application.Installments;
using VirtualPos.Application.Payments;
using VirtualPos.Application.Orders;
using VirtualPos.Api.BackgroundJobs;
using VirtualPos.Infrastructure;
using VirtualPos.Infrastructure.Payments;

var builder = WebApplication.CreateBuilder(args);

// ---------------------------------------------------------------
// 1) Ayarlar (appsettings.Development.json)
// ---------------------------------------------------------------
var connectionString = builder.Configuration.GetConnectionString("VirtualPosDb")
    ?? throw new InvalidOperationException("'VirtualPosDb' bağlantı cümlesi bulunamadı.");

var apiBaseUrl = builder.Configuration["App:ApiBaseUrl"]
    ?? throw new InvalidOperationException("'App:ApiBaseUrl' ayarı bulunamadı.");

var frontendBaseUrl = builder.Configuration["App:FrontendBaseUrl"]
    ?? throw new InvalidOperationException("'App:FrontendBaseUrl' ayarı bulunamadı.");

var paymentProvider = builder.Configuration["Payment:Provider"] ?? "MockBank";

IyzicoSettings? iyzicoSettings = null;
if (paymentProvider.Equals("Iyzico", StringComparison.OrdinalIgnoreCase))
{
    iyzicoSettings = new IyzicoSettings(
        builder.Configuration["Iyzico:ApiKey"]
            ?? throw new InvalidOperationException("'Iyzico:ApiKey' bulunamadı. dotnet user-secrets ile ekleyin."),
        builder.Configuration["Iyzico:SecretKey"]
            ?? throw new InvalidOperationException("'Iyzico:SecretKey' bulunamadı. dotnet user-secrets ile ekleyin."),
        builder.Configuration["Iyzico:BaseUrl"] ?? "https://sandbox-api.iyzipay.com");
}

// ---------------------------------------------------------------
// 2) Servis kayıtları (Dependency Injection)
// ---------------------------------------------------------------

// Web API altyapısı
builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(frontendBaseUrl)
              .AllowAnyHeader()
              .AllowAnyMethod());
});

// Infrastructure: veritabanı, BIN, sahte banka, SMS
builder.Services.AddInfrastructure(connectionString, apiBaseUrl, paymentProvider, iyzicoSettings);

// Application: iş kuralları
builder.Services.AddSingleton(new PaymentSettings(apiBaseUrl, frontendBaseUrl));
builder.Services.AddScoped<InstallmentService>();
builder.Services.AddScoped<OrderService>();
builder.Services.AddScoped<PaymentService>();
builder.Services.AddHostedService<PaymentTimeoutJob>();

var app = builder.Build();

// ---------------------------------------------------------------
// 3) İstek hattı (middleware)
// ---------------------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

// Sunucuda Vue'nun derlenmiş dosyaları wwwroot klasöründen sunulur.
// Bilgisayarımda wwwroot yok, Vue 5173'te ayrı çalışmaya devam eder.
app.UseDefaultFiles();   // "/" adresinde index.html'i açar
app.UseStaticFiles();    // wwwroot içindeki dosyaları (js, css) gönderir

app.UseCors("Frontend");
app.UseAuthorization();

app.MapControllers();

// /odeme/..., /result/... gibi Vue sayfaları sunucuda dosya olarak yok;
// bu istekleri index.html'e yönlendirir, sayfayı Vue Router açar.
app.MapFallbackToFile("index.html");

app.Run();