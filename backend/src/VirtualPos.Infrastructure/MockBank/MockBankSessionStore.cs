using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Concurrent;

namespace VirtualPos.Infrastructure.MockBank;

// API'nin dışarıdan erişilen adresi (şifre sayfasının linki için)
public record MockBankSettings(string ApiBaseUrl);

// Bankanın tek bir 3DS doğrulaması için tuttuğu bilgiler
public class MockBankSession
{
    public required string Token { get; init; }
    public required Guid OrderNo { get; init; }
    public required decimal Amount { get; init; }
    public required string MaskedCardNumber { get; init; }
    public required string CallbackUrl { get; init; }
    public required string OtpCode { get; init; }
    public required DateTime ExpiresAt { get; init; }
    public int FailedAttempts { get; set; }
    public bool IsVerified { get; set; }
    public bool IsCompleted { get; set; }
}

// Bankanın hafızası: token -> oturum
public class MockBankSessionStore
{
    private readonly ConcurrentDictionary<string, MockBankSession> _sessions = new();

    public void Add(MockBankSession session) => _sessions[session.Token] = session;

    public MockBankSession? Get(string token) =>
        _sessions.TryGetValue(token, out var session) ? session : null;
}
