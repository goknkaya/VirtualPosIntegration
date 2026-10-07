# VirtualPos — Sanal POS Entegrasyonu

iyzico benzeri bir ödeme akışı: kart numarası girilirken bankayı tespit eden, kredi kartına taksit sunan ve ödemeyi **3D Secure** ile tamamlayan, sağlayıcıdan bağımsız bir sanal POS entegrasyonu.

**Backend:** .NET 10 Web API · **Frontend:** Vue 3 + TypeScript · **Veritabanı:** SQL Server

## Özellikler

- **Banka tespiti (BIN):** Kart numarasının ilk 6 hanesinde banka, kart şeması (Visa, Mastercard, Troy, Amex), kart tipi ve program adı (Bonus, Maximum vb.) BKM kaynaklı BIN listesinden bulunur; 8 hanede kesinleşir.
- **Taksit:** Kredi kartına vade farklı taksit seçenekleri, banka kartına yalnızca tek çekim.
- **3D Secure akışı:** Ödeme başlatma → banka doğrulama sayfası (SMS kodu, 3 dk süre, 3 deneme hakkı) → callback → sunucu tarafında tamamlama.
- **Sipariş yapısı:** Tutar sunucuda tutulur; ödeme sayfası yalnızca sipariş numarasıyla çalışır.
- **Zaman aşımı:** 5 dakika içinde tamamlanmayan ödemeler arka plan servisiyle otomatik kapatılır.
- **İz kaydı:** Her ödeme adımı (`Initialize`, `Callback`, `Complete`, `Timeout`) `PaymentLogs` tablosuna yazılır.

## Mimari

Katmanlı monolit; dört proje, tek çalışan uygulama.

```
backend/src
├─ VirtualPos.Domain          Entity'ler ve enum'lar (bağımlılığı yok)
├─ VirtualPos.Application     İş kuralları ve sözleşmeler (IBinProvider, IPaymentProvider ...)
├─ VirtualPos.Infrastructure  EF Core, BIN sorgusu, simülasyon bankası
└─ VirtualPos.Api             Controller'lar, arka plan servisi, Program.cs
frontend                      Vue 3 + TypeScript + Vue Router
```

Ödeme sağlayıcısı `IPaymentProvider` arayüzünün arkasındadır. Şu an `MockPaymentProvider` (simülasyon bankası) kullanılır; gerçek bir sağlayıcı eklemek yeni bir sınıf ve `DependencyInjection.cs`'te tek satırlık değişikliktir.

## Güvenlik ilkeleri

- Tam kart numarası ve CVV hiçbir yerde saklanmaz; yalnızca BIN ve maskeli numara (`474508******0012`).
- Tutar istemciden alınmaz; sipariş ve taksit tutarı sunucuda hesaplanır.
- Kart doğrulaması (Luhn, son kullanma, CVV) hem istemcide hem sunucuda yapılır.
- Ödemeyi sunucu tamamlar; banka kendi kaydını kontrol ettiği için sahte bir callback ödeme tamamlatamaz.
- Callback idempotenttir: aynı ödeme iki kez işlenmez.
- Dışarıya sıralı ID yerine tahmin edilemez GUID verilir.
- CORS yalnızca frontend adresine açıktır.

## Kurulum

**Gereksinimler:** .NET 10 SDK, Node.js 20+, SQL Server (Express yeterli), `dotnet-ef` aracı (`dotnet tool install --global dotnet-ef`)

### 1. Veritabanı

`backend/src/VirtualPos.Api/appsettings.Development.json` içindeki bağlantı cümlesini kendi SQL Server'ınıza göre düzenleyin, sonra:

```powershell
cd backend
dotnet ef database update --project src/VirtualPos.Infrastructure --startup-project src/VirtualPos.Api
```

### 2. BIN verisi

[guncel-kart-bin-listesi](https://github.com/hazarkarabay/guncel-kart-bin-listesi/releases/latest) sürümlerinden `create_mssql.sql` ve `insert_mssql.sql` dosyalarını indirip `VirtualPosDb` üzerinde sırayla çalıştırın.

> Bu liste BKM verisini yayımlayan üçüncü taraf açık kaynak bir projedir; BKM ile ilişkisi yoktur. Demo amaçlı kullanılmıştır, canlı ortamda BIN verisi BKM'den kurum adına alınmalıdır.

### 3. Çalıştırma

```powershell
# Backend (https://localhost:7177, API dokümantasyonu: /scalar/v1)
cd backend
dotnet run --project src/VirtualPos.Api

# Frontend (http://localhost:5173)
cd frontend
npm install
npm run dev
```

## Kullanım

1. `http://localhost:5173` adresinde tutar ve telefon girip **Ödemeye geç**.
2. Test kartlarından birini girin; banka bilgisi ve taksitler otomatik görünür.
3. **Öde** → simülasyon bankasının sayfasında doğrulama kodunu girin (kod sayfada "Demo" kutusunda ve API konsolunda görünür).
4. Sonuç sayfasında ödemenin durumunu görün.

### Test kartları

Herhangi bir ileri tarih (ör. `12/30`) ve 3 haneli CVV ile:

| Banka | Tip | Kart numarası |
| --- | --- | --- |
| İş Bankası | Visa, kredi | `4745 0800 0000 0012` |
| Garanti BBVA | Mastercard, kredi | `5289 5600 0000 0012` |
| Akbank | Visa, kredi | `4256 6900 0000 0019` |
| Yapı Kredi | Mastercard, kredi | `5526 4500 0000 0019` |
| Halkbank | Troy, kredi | `6587 8406 0000 0019` |
| VakıfBank | Visa, banka kartı | `4234 7800 0000 0015` |
| QNB | Mastercard, banka kartı | `5343 1907 0000 0015` |

## API

| Metot | Adres | Açıklama |
| --- | --- | --- |
| GET | `/api/bins/{bin}` | BIN'den banka bilgisi |
| GET | `/api/installments?bin=&amount=` | Taksit seçenekleri |
| POST | `/api/orders` | Sipariş oluştur |
| GET | `/api/orders/{orderNo}` | Sipariş bilgisi |
| POST | `/api/payments/3ds/initialize` | 3D Secure ödeme başlat |
| POST | `/api/payments/3ds/callback` | Bankadan dönüş (form post) |
| GET | `/api/payments/{paymentNo}` | Ödeme sonucu |

## Yol haritası

- [ ] iyzico sandbox entegrasyonu (`IyzicoPaymentProvider`)
- [ ] İptal ve iade
- [ ] Webhook ile ödeme sonucu bildirimi
- [ ] İşlem listesi ekranı
