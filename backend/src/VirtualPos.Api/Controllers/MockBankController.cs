using System.Globalization;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using VirtualPos.Infrastructure.MockBank;

namespace VirtualPos.Api.Controllers;

// Sahte bankanın 3D Secure sayfaları.
// Gerçekte bu sayfalar bankanın kendi sunucusunda (ACS) çalışır; demo için API içinde duruyor.
[ApiController]
[Route("mock-bank/3ds")]
[ApiExplorerSettings(IgnoreApi = true)]
public class MockBankController(MockBankSessionStore store) : ControllerBase
{
    private const int MaxAttempts = 3;
    private static readonly CultureInfo Tr = new("tr-TR");

    // 1) Şifre sayfasını göster
    [HttpGet("{token}")]
    public IActionResult ShowPage(string token)
    {
        var session = store.Get(token);
        if (session is null)
            return Html(MessagePage("İşlem bulunamadı", "Bu doğrulama bağlantısı geçersiz."), 404);

        if (session.IsVerified || session.IsCompleted)
            return Html(MessagePage("İşlem tamamlandı", "Bu ödeme için doğrulama zaten yapıldı."));

        if (DateTime.UtcNow > session.ExpiresAt)
            return Html(CallbackPage(session, success: false));

        return Html(OtpPage(session, error: null));
    }

    // 2) Girilen kodu kontrol et
    [HttpPost("{token}/verify")]
    public IActionResult Verify(string token, [FromForm] string? code, [FromForm] string? action)
    {
        var session = store.Get(token);
        if (session is null)
            return Html(MessagePage("İşlem bulunamadı", "Bu doğrulama bağlantısı geçersiz."), 404);

        if (session.IsVerified || session.IsCompleted)
            return Html(MessagePage("İşlem tamamlandı", "Bu ödeme için doğrulama zaten yapıldı."));

        if (action == "cancel" || DateTime.UtcNow > session.ExpiresAt)
            return Html(CallbackPage(session, success: false));

        if (code?.Trim() == session.OtpCode)
        {
            session.IsVerified = true;
            return Html(CallbackPage(session, success: true));
        }

        session.FailedAttempts++;
        if (session.FailedAttempts >= MaxAttempts)
            return Html(CallbackPage(session, success: false));

        return Html(OtpPage(session, $"Kod hatalı. Kalan deneme hakkı: {MaxAttempts - session.FailedAttempts}"));
    }

    // Kod giriş ekranı
    private static string OtpPage(MockBankSession s, string? error)
    {
        var amount = s.Amount.ToString("N2", Tr);
        var errorHtml = error is null ? "" : $"<p class=\"error\">{Enc(error)}</p>";
        var expiresIso = s.ExpiresAt.ToString("o");

        return Layout($$"""
            <h2>Ödeme doğrulama</h2>
            <table>
              <tr><td>Üye işyeri</td><td>VirtualPos Demo Mağaza</td></tr>
              <tr><td>Tutar</td><td><strong>{{amount}} TL</strong></td></tr>
              <tr><td>Kart</td><td>{{Enc(s.MaskedCardNumber)}}</td></tr>
            </table>
            <p>{{Enc(s.MaskedPhone)}} numaralı telefonunuza gönderilen 6 haneli kodu giriniz.</p>
            {{errorHtml}}
            <form method="post" action="/mock-bank/3ds/{{s.Token}}/verify">
              <input name="code" maxlength="6" inputmode="numeric" autocomplete="one-time-code" autofocus required pattern="\d{6}" />
              <div class="buttons">
                <button type="submit" name="action" value="verify">Onayla</button>
                <button type="submit" name="action" value="cancel" formnovalidate class="secondary">İptal</button>
              </div>
            </form>
            <p class="timer">Kalan süre: <span id="t"></span></p>
            <div class="demo">Demo: SMS kodu <strong>{{s.OtpCode}}</strong> (gerçek bankada bu kutu gösterilmez)</div>
            <script>
              const end = new Date('{{expiresIso}}').getTime();
              const el = document.getElementById('t');
              function tick() {
                const sec = Math.max(0, Math.floor((end - Date.now()) / 1000));
                el.textContent = sec === 0
                  ? 'Süre doldu'
                  : String(Math.floor(sec / 60)).padStart(2, '0') + ':' + String(sec % 60).padStart(2, '0');
              }
              tick();
              setInterval(tick, 1000);
            </script>
            """);
    }

    // Sonucu mağazanın callback adresine otomatik form ile gönderen ara sayfa
    private static string CallbackPage(MockBankSession s, bool success)
    {
        var status = success ? "success" : "failure";
        var mdStatus = success ? "1" : "0";
        var text = success ? "Doğrulama başarılı." : "Doğrulama başarısız.";

        return Layout($$"""
            <p>{{text}} Mağazaya yönlendiriliyorsunuz...</p>
            <form id="f" method="post" action="{{Enc(s.CallbackUrl)}}">
              <input type="hidden" name="status" value="{{status}}" />
              <input type="hidden" name="paymentId" value="{{s.Token}}" />
              <input type="hidden" name="conversationId" value="{{s.PaymentNo}}" />
              <input type="hidden" name="mdStatus" value="{{mdStatus}}" />
              <noscript><button type="submit">Devam</button></noscript>
            </form>
            <script>document.getElementById('f').submit();</script>
            """);
    }

    private static string MessagePage(string title, string text) =>
        Layout($"<h2>{Enc(title)}</h2><p>{Enc(text)}</p>");

    private static string Layout(string content) => $$"""
        <!DOCTYPE html>
        <html lang="tr">
        <head>
          <meta charset="utf-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1" />
          <title>Sanal Banka - 3D Secure</title>
          <style>
            body { font-family: system-ui, sans-serif; background: #eef1f5; margin: 0; color: #222; }
            .card { max-width: 380px; margin: 40px auto; background: #fff; border-radius: 10px; box-shadow: 0 2px 12px rgba(0,0,0,.08); overflow: hidden; }
            header { background: #0b3d91; color: #fff; padding: 14px 18px; font-weight: 600; }
            main { padding: 18px; }
            h2 { margin-top: 0; font-size: 18px; }
            table { width: 100%; font-size: 14px; margin-bottom: 12px; }
            td:first-child { color: #666; }
            input[name=code] { width: 100%; box-sizing: border-box; font-size: 22px; letter-spacing: 6px; text-align: center; padding: 10px; border: 1px solid #bbb; border-radius: 6px; }
            .buttons { display: flex; gap: 8px; margin-top: 12px; }
            button { flex: 1; padding: 10px; border: none; border-radius: 6px; background: #0b3d91; color: #fff; font-size: 15px; cursor: pointer; }
            button.secondary { background: #ddd; color: #333; }
            .error { color: #c0392b; }
            .timer { font-size: 13px; color: #666; }
            .demo { margin-top: 12px; padding: 10px; background: #fff8e1; border: 1px dashed #e0b000; border-radius: 6px; font-size: 13px; }
          </style>
        </head>
        <body>
          <div class="card">
            <header>Sanal Banka · 3D Secure</header>
            <main>{{content}}</main>
          </div>
        </body>
        </html>
        """;

    private static string Enc(string value) => WebUtility.HtmlEncode(value);

    private static ContentResult Html(string body, int statusCode = 200) =>
        new() { Content = body, ContentType = "text/html; charset=utf-8", StatusCode = statusCode };
}