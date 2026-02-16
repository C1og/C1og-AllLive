using Jint;
using AllLive.SignService;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Linq;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.Urls.Add("http://0.0.0.0:8788");

const string DouyinVersionCode = "180800";
const string DouyinSdkVersion = "1.0.14-beta.0";
var douyinWebmssdkScript = new Lazy<string>(LoadDouyinWebmssdkScript);

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapPost("/api/douyu/sign", async (HttpRequest request) =>
{
    SignRequest? payload;
    try
    {
        payload = await request.ReadFromJsonAsync<SignRequest>();
    }
    catch
    {
        return Results.Json(new { code = -1, msg = "invalid json" });
    }

    var html = payload?.html ?? string.Empty;
    var rid = payload?.rid ?? string.Empty;
    if (string.IsNullOrWhiteSpace(html) || string.IsNullOrWhiteSpace(rid))
    {
        return Results.Json(new { code = -1, msg = "html or rid empty" });
    }

    try
    {
        var js = ExtractSignJs(html);
        if (string.IsNullOrEmpty(js))
        {
            js = html;
        }
        if (string.IsNullOrEmpty(js) || !js.Contains("ub98484234"))
        {
            return Results.Json(new { code = -1, msg = "sign js empty" });
        }
        js = Regex.Replace(js, @"eval.*?;}", "strc;}");

        var did = "10000000000000000000000000001501";
        var t10 = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

        var engine = new Engine();
        engine.Execute(js);
        var jsEvalResult = engine.Evaluate("ub98484234()").ToString();
        if (string.IsNullOrEmpty(jsEvalResult))
        {
            return Results.Json(new { code = -1, msg = "ub98484234 empty" });
        }

        var vMatch = Regex.Match(jsEvalResult, @"v=(\d+)");
        var v = vMatch.Success ? vMatch.Groups[1].Value : "";
        if (string.IsNullOrEmpty(v))
        {
            return Results.Json(new { code = -1, msg = "v empty" });
        }

        var rb = Md5Hex(rid + did + t10 + v);
        var jsSign = Regex.Replace(jsEvalResult, @"return rt;}\);?", "return rt;}");
        jsSign = jsSign.Replace("(function (", "function sign(");
        jsSign = jsSign.Replace("CryptoJS.MD5(cb).toString()", $"\"{rb}\"");

        engine.Execute(jsSign);
        var signExpr = BuildSignCall(rid, did, t10);
        var args = engine.Evaluate(signExpr).ToString();
        if (string.IsNullOrEmpty(args))
        {
            return Results.Json(new { code = -1, msg = "sign empty" });
        }

        return Results.Json(new { code = 0, data = args, msg = "" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { code = -1, msg = ex.Message });
    }
});

app.MapPost("/api/douyin/sign", async (HttpRequest request) =>
{
    DouyinSignRequest? payload;
    try
    {
        payload = await request.ReadFromJsonAsync<DouyinSignRequest>();
    }
    catch
    {
        return Results.Json(new { code = -1, msg = "invalid json" });
    }

    var roomId = payload?.roomId ?? string.Empty;
    var uniqueId = payload?.uniqueId ?? string.Empty;
    if (string.IsNullOrWhiteSpace(roomId) || string.IsNullOrWhiteSpace(uniqueId))
    {
        return Results.Json(new { code = -1, msg = "roomId or uniqueId empty" });
    }

    try
    {
        var script = douyinWebmssdkScript.Value;
        if (string.IsNullOrWhiteSpace(script))
        {
            return Results.Json(new { code = -1, msg = "webmssdk script empty" });
        }

        var sign = BuildDouyinSignature(script, roomId, uniqueId);
        if (string.IsNullOrWhiteSpace(sign))
        {
            return Results.Json(new { code = -1, msg = "signature empty" });
        }
        return Results.Json(new { code = 0, data = new { signature = sign }, msg = "" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { code = -1, msg = ex.Message });
    }
});

app.MapPost("/api/douyin/abogus", async (HttpRequest request) =>
{
    AbogusRequest? payload;
    try
    {
        payload = await request.ReadFromJsonAsync<AbogusRequest>();
    }
    catch
    {
        return Results.Json(new { code = -1, msg = "invalid json" });
    }

    var url = payload?.url ?? string.Empty;
    if (string.IsNullOrWhiteSpace(url))
    {
        return Results.Json(new { code = -1, msg = "url empty" });
    }

    try
    {
        var signedUrl = DouyinAbogus.BuildSignedUrl(url, payload?.userAgent ?? "", payload?.body ?? "");
        return Results.Json(new { code = 0, data = new { url = signedUrl }, msg = "" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { code = -1, msg = ex.Message });
    }
});

app.Run();

static string BuildDouyinSignature(string script, string roomId, string uniqueId)
{
    var signParam = BuildDouyinSignParam(roomId, uniqueId);
    if (string.IsNullOrWhiteSpace(signParam))
    {
        return "";
    }
    var md5 = Md5Hex(signParam);
    var engine = new Engine();
    engine.Execute(script);
    var result = engine.Evaluate($"get_sign('{EscapeJs(md5)}')")?.ToString();
    return result ?? "";
}

static string BuildDouyinSignParam(string roomId, string uniqueId)
{
    var pairs = new List<KeyValuePair<string, string>>
    {
        new KeyValuePair<string, string>("live_id", "1"),
        new KeyValuePair<string, string>("aid", "6383"),
        new KeyValuePair<string, string>("version_code", DouyinVersionCode),
        new KeyValuePair<string, string>("webcast_sdk_version", DouyinSdkVersion),
        new KeyValuePair<string, string>("room_id", roomId ?? ""),
        new KeyValuePair<string, string>("sub_room_id", ""),
        new KeyValuePair<string, string>("sub_channel_id", ""),
        new KeyValuePair<string, string>("did_rule", "3"),
        new KeyValuePair<string, string>("user_unique_id", uniqueId ?? ""),
        new KeyValuePair<string, string>("device_platform", "web"),
        new KeyValuePair<string, string>("device_type", ""),
        new KeyValuePair<string, string>("ac", ""),
        new KeyValuePair<string, string>("identity", "audience"),
    };
    return string.Join(",", pairs.Select(item => $"{item.Key}={item.Value}"));
}

static string LoadDouyinWebmssdkScript()
{
    try
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Assets", "douyin-webmssdk.js");
        if (!File.Exists(path))
        {
            return "";
        }
        return File.ReadAllText(path, Encoding.UTF8);
    }
    catch
    {
        return "";
    }
}

static string ExtractSignJs(string html)
{
    var match = Regex.Match(html, @"(vdwdae325w_64we[\s\S]*function ub98484234[\s\S]*?)function", RegexOptions.Singleline);
    return match.Groups.Count > 1 ? match.Groups[1].Value : "";
}

static string Md5Hex(string input)
{
    using var md5 = MD5.Create();
    var bytes = Encoding.UTF8.GetBytes(input);
    var hash = md5.ComputeHash(bytes);
    var sb = new StringBuilder(hash.Length * 2);
    foreach (var b in hash)
    {
        sb.Append(b.ToString("x2"));
    }
    return sb.ToString();
}

static string BuildSignCall(string rid, string did, string t10)
{
    if (long.TryParse(rid, out var ridNum))
    {
        return $"sign({ridNum},'{EscapeJs(did)}',{t10})";
    }
    return $"sign('{EscapeJs(rid)}','{EscapeJs(did)}',{t10})";
}

static string EscapeJs(string value)
{
    return value.Replace("\\", "\\\\").Replace("'", "\\'");
}

record DouyinSignRequest(string roomId, string uniqueId);
record SignRequest(string html, string rid);
record AbogusRequest(string url, string? userAgent, string? body);
