namespace AIChatbot.web.Filters
{
    public static class TokenHelper
    {
        public static string? ExtractRoleFromToken(string token)
        {
            try
            {
                var parts = token.Split('.');
                if (parts.Length != 3) return null;

                var payload = parts[1];
                var padded = payload.Length % 4 == 0 ? payload : payload + new string('=', 4 - payload.Length % 4);
                var base64 = padded.Replace('-', '+').Replace('_', '/');
                var json = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(base64));

                if (json.Contains("\"role\":"))
                {
                    var start = json.IndexOf("\"role\":\"") + 8;
                    var end = json.IndexOf('"', start);
                    return json.Substring(start, end - start);
                }

                return null;
            }
            catch
            {
                return null;
            }
        }
    }
}