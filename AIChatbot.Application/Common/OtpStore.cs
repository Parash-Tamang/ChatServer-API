using System.Collections.Concurrent;

public class OtpEntry
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public DateTime Expiry { get; set; }
    public int Attempts { get; set; }
}

public interface IOtpStore
{
    void Save(string otp, OtpEntry entry);
    OtpEntry? Get(string otp);
    void Remove(string otp);
}

public class InMemoryOtpStore : IOtpStore
{
    private static readonly ConcurrentDictionary<string, OtpEntry> _store = new();

    public void Save(string otp, OtpEntry entry)
    {
        _store[otp] = entry;
    }

    public OtpEntry? Get(string otp)
    {
        if (_store.TryGetValue(otp, out var entry))
        {
            if (entry.Expiry < DateTime.UtcNow)
            {
                _store.TryRemove(otp, out _);
                return null;
            }
            return entry;
        }
        return null;
    }

    public void Remove(string otp)
    {
        _store.TryRemove(otp, out _);
    }
}