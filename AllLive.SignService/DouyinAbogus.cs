using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AllLive.SignService
{
    public static class DouyinAbogus
    {
        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/125.0.0.0 Safari/537.36 Edg/125.0.0.0";
        private const string DefaultBrowserVersion = "125.0.0.0";

        public static string BuildSignedUrl(string url, string userAgent, string body = "")
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new ArgumentException("url empty");
            }
            var ua = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            var uri = new Uri(url);
            var baseUrl = uri.GetLeftPart(UriPartial.Path);
            var query = uri.Query;
            if (!string.IsNullOrEmpty(query) && query.StartsWith("?"))
            {
                query = query.Substring(1);
            }

            var pairs = ParseQuery(query);
            RemoveParam(pairs, "a_bogus");

            EnsureParam(pairs, "aid", "6383");
            EnsureParam(pairs, "compress", "gzip");
            EnsureParam(pairs, "device_platform", "web");
            EnsureParam(pairs, "browser_language", "zh-CN");
            EnsureParam(pairs, "browser_platform", "Win32");
            EnsureParam(pairs, "browser_name", "Edge");
            EnsureParam(pairs, "browser_version", DefaultBrowserVersion);
            EnsureParam(pairs, "msToken", GenerateMsToken());

            var paramStr = BuildQueryString(pairs);
            var abogus = new Abogus(userAgent: ua);
            var signedParams = abogus.GenerateAbogus(paramStr, body ?? "");
            return $"{baseUrl}?{signedParams}";
        }

        private static List<KeyValuePair<string, string>> ParseQuery(string query)
        {
            var result = new List<KeyValuePair<string, string>>();
            if (string.IsNullOrWhiteSpace(query))
            {
                return result;
            }
            var parts = query.Split(new[] { '&' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var idx = part.IndexOf('=');
                string key;
                string value;
                if (idx >= 0)
                {
                    key = part.Substring(0, idx);
                    value = idx + 1 < part.Length ? part.Substring(idx + 1) : "";
                }
                else
                {
                    key = part;
                    value = "";
                }
                key = Uri.UnescapeDataString(key);
                value = Uri.UnescapeDataString(value);
                result.Add(new KeyValuePair<string, string>(key, value));
            }
            return result;
        }

        private static void EnsureParam(List<KeyValuePair<string, string>> pairs, string key, string value)
        {
            for (var i = 0; i < pairs.Count; i++)
            {
                if (string.Equals(pairs[i].Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    if (string.IsNullOrWhiteSpace(pairs[i].Value))
                    {
                        pairs[i] = new KeyValuePair<string, string>(pairs[i].Key, value);
                    }
                    return;
                }
            }
            pairs.Add(new KeyValuePair<string, string>(key, value));
        }

        private static void RemoveParam(List<KeyValuePair<string, string>> pairs, string key)
        {
            pairs.RemoveAll(p => string.Equals(p.Key, key, StringComparison.OrdinalIgnoreCase));
        }

        private static string BuildQueryString(List<KeyValuePair<string, string>> pairs)
        {
            return string.Join("&", pairs.Select(p =>
                $"{Uri.EscapeDataString(p.Key)}={Uri.EscapeDataString(p.Value ?? "")}"));
        }

        private static string GenerateMsToken(int length = 184)
        {
            const string baseStr = "ABCDEFGHIGKLMNOPQRSTUVWXYZabcdefghigklmnopqrstuvwxyz0123456789=";
            var sb = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var index = Random.Shared.Next(baseStr.Length);
                sb.Append(baseStr[index]);
            }
            return sb.ToString();
        }
    }

    internal static class StringProcessor
    {
        public static string ToCharStr(IReadOnlyList<int> codes)
        {
            var chars = new char[codes.Count];
            for (var i = 0; i < codes.Count; i++)
            {
                chars[i] = (char)codes[i];
            }
            return new string(chars);
        }

        public static List<int> ToOrdArray(string s)
        {
            return s.Select(c => (int)c).ToList();
        }

        public static int JsShiftRight(long val, int n)
        {
            var x = (uint)(val & 0xFFFFFFFF);
            return (int)(x >> n);
        }

        public static string GenerateRandomBytes(int length = 3)
        {
            var result = new StringBuilder();
            for (var i = 0; i < length; i++)
            {
                var rd = Random.Shared.Next(10000);
                var b0 = (char)(((rd & 255) & 170) | 1);
                var b1 = (char)(((rd & 255) & 85) | 2);
                var b2 = (char)(((JsShiftRight(rd, 8) & 170) | 5));
                var b3 = (char)(((JsShiftRight(rd, 8) & 85) | 40));
                result.Append(b0).Append(b1).Append(b2).Append(b3);
            }
            return result.ToString();
        }
    }

    internal sealed class CryptoUtility
    {
        private readonly string _salt;
        private readonly string[] _base64Alphabet;
        private readonly List<int> _bigArray;

        public CryptoUtility(string salt, string[] base64Alphabet)
        {
            _salt = salt;
            _base64Alphabet = base64Alphabet;
            _bigArray = new List<int>
            {
                121,243,55,234,103,36,47,228,30,231,106,6,115,95,78,101,250,207,198,50,
                139,227,220,105,97,143,34,28,194,215,18,100,159,160,43,8,169,217,180,120,
                247,45,90,11,27,197,46,3,84,72,5,68,62,56,221,75,144,79,73,161,
                178,81,64,187,134,117,186,118,16,241,130,71,89,147,122,129,65,40,88,150,
                110,219,199,255,181,254,48,4,195,248,208,32,116,167,69,201,17,124,125,104,
                96,83,80,127,236,108,154,126,204,15,20,135,112,158,13,1,188,164,210,237,
                222,98,212,77,253,42,170,202,26,22,29,182,251,10,173,152,58,138,54,141,
                185,33,157,31,252,132,233,235,102,196,191,223,240,148,39,123,92,82,128,109,
                57,24,38,113,209,245,2,119,153,229,189,214,230,174,232,63,52,205,86,140,
                66,175,111,171,246,133,238,193,99,60,74,91,225,51,76,37,145,211,166,151,
                213,206,0,200,244,176,218,44,184,172,49,216,93,168,53,21,183,41,67,85,
                224,155,226,242,87,177,146,70,190,12,162,19,137,114,25,165,163,192,23,59,
                9,94,179,107,35,7,142,131,239,203,149,136,61,249,14,156
            };
        }

        public List<int> ParamsToArray(object param, bool addSalt = true)
        {
            if (param is string s)
            {
                var processed = addSalt ? s + _salt : s;
                return Sm3.ToArray(processed);
            }
            if (param is IReadOnlyList<int> list)
            {
                return Sm3.ToArray(list);
            }
            if (param is byte[] bytes)
            {
                return Sm3.ToArray(bytes);
            }
            throw new ArgumentException("param");
        }

        public string TransformBytes(List<int> bytesList)
        {
            var bytesStr = StringProcessor.ToCharStr(bytesList);
            var resultStr = new StringBuilder();
            var indexB = _bigArray[1];
            var initialValue = 0;
            var valueE = 0;

            for (var index = 0; index < bytesStr.Length; index++)
            {
                var sumInitial = 0;
                if (index == 0)
                {
                    initialValue = _bigArray[indexB];
                    sumInitial = indexB + initialValue;
                    _bigArray[1] = initialValue;
                    _bigArray[indexB] = indexB;
                }
                else
                {
                    sumInitial = initialValue + valueE;
                }

                var charValue = bytesStr[index];
                sumInitial %= _bigArray.Count;
                var valueF = _bigArray[sumInitial];
                var encryptedChar = charValue ^ valueF;
                resultStr.Append((char)encryptedChar);

                valueE = _bigArray[(index + 2) % _bigArray.Count];
                sumInitial = (indexB + valueE) % _bigArray.Count;
                initialValue = _bigArray[sumInitial];
                _bigArray[sumInitial] = _bigArray[(index + 2) % _bigArray.Count];
                _bigArray[(index + 2) % _bigArray.Count] = initialValue;
                indexB = sumInitial;
            }
            return resultStr.ToString();
        }

        public string Base64Encode(string inputString, int selectedAlphabet)
        {
            var bytes = inputString.Select(c => (byte)c).ToArray();
            return EncodeWithAlphabet(bytes, _base64Alphabet[selectedAlphabet]);
        }

        public string AbogusEncode(string abogusBytesStr, int selectedAlphabet)
        {
            var bytes = abogusBytesStr.Select(c => (byte)c).ToArray();
            return EncodeWithAlphabet(bytes, _base64Alphabet[selectedAlphabet]);
        }

        private static string EncodeWithAlphabet(byte[] data, string alphabet)
        {
            var sb = new StringBuilder();
            var i = 0;
            while (i < data.Length)
            {
                int b0 = data[i++];
                int b1 = i < data.Length ? data[i++] : -1;
                int b2 = i < data.Length ? data[i++] : -1;

                int n = (b0 << 16) | ((b1 >= 0 ? b1 : 0) << 8) | (b2 >= 0 ? b2 : 0);

                sb.Append(alphabet[(n >> 18) & 0x3F]);
                sb.Append(alphabet[(n >> 12) & 0x3F]);
                sb.Append(b1 >= 0 ? alphabet[(n >> 6) & 0x3F] : '=');
                sb.Append(b2 >= 0 ? alphabet[n & 0x3F] : '=');
            }
            return sb.ToString();
        }

        public static byte[] Rc4Encrypt(byte[] key, string plaintext)
        {
            var s = Enumerable.Range(0, 256).Select(i => (byte)i).ToArray();
            var j = 0;
            for (var i = 0; i < 256; i++)
            {
                j = (j + s[i] + key[i % key.Length]) % 256;
                (s[i], s[j]) = (s[j], s[i]);
            }
            var iIdx = 0;
            j = 0;
            var output = new List<byte>();
            foreach (var ch in plaintext.Select(c => (byte)c))
            {
                iIdx = (iIdx + 1) % 256;
                j = (j + s[iIdx]) % 256;
                (s[iIdx], s[j]) = (s[j], s[iIdx]);
                var k = s[(s[iIdx] + s[j]) % 256];
                output.Add((byte)(ch ^ k));
            }
            return output.ToArray();
        }
    }

    internal static class BrowserFingerprintGenerator
    {
        public static string GenerateFingerprint(string browserType = "Edge")
        {
            return browserType switch
            {
                "Safari" => GenerateFingerprint("MacIntel"),
                _ => GenerateFingerprint("Win32")
            };
        }

        private static string GenerateFingerprint(string platform)
        {
            var random = Random.Shared;
            var innerWidth = 1024 + random.Next(1920 - 1024 + 1);
            var innerHeight = 768 + random.Next(1080 - 768 + 1);
            var outerWidth = innerWidth + (24 + random.Next(32 - 24 + 1));
            var outerHeight = innerHeight + (75 + random.Next(90 - 75 + 1));
            var screenX = 0;
            var screenY = random.Next(2) == 0 ? 0 : 30;
            var sizeWidth = 1024 + random.Next(1920 - 1024 + 1);
            var sizeHeight = 768 + random.Next(1080 - 768 + 1);
            var availWidth = 1280 + random.Next(1920 - 1280 + 1);
            var availHeight = 800 + random.Next(1080 - 800 + 1);

            return $"{innerWidth}|{innerHeight}|{outerWidth}|{outerHeight}|{screenX}|{screenY}|0|0|{sizeWidth}|{sizeHeight}|{availWidth}|{availHeight}|{innerWidth}|{innerHeight}|24|24|{platform}";
        }
    }

    internal sealed class Abogus
    {
        private const int Aid = 6383;
        private const int PageId = 0;
        private const string Salt = "cus";
        private const string Character = "Dkdpgh2ZmsQB80/MfvV36XI1R45-WUAlEixNLwoqYTOPuzKFjJnry79HbGcaStCe";
        private const string Character2 = "ckdp1h4ZKsUB80/Mfvw36XIgR25+WQAlEi7NLboqYTOPuzmFjJnryx9HVGDaStCe";
        private static readonly byte[] UaKey = { 0, 1, 14 };

        private readonly string _userAgent;
        private readonly string _browserFp;
        private readonly int[] _options;
        private readonly CryptoUtility _cryptoUtility;
        private readonly int[] _sortIndex =
        {
            18,20,52,26,30,34,58,38,40,53,42,21,27,54,55,31,35,57,39,41,43,22,28,
            32,60,36,23,29,33,37,44,45,59,46,47,48,49,50,24,25,65,66,70,71
        };
        private readonly int[] _sortIndex2 =
        {
            18,20,26,30,34,38,40,42,21,27,31,35,39,41,43,22,28,32,36,23,29,33,37,
            44,45,46,47,48,49,50,24,25,52,53,54,55,57,58,59,60,65,66,70,71
        };

        public Abogus(string userAgent, string browserFp = null, int[] options = null)
        {
            _userAgent = string.IsNullOrWhiteSpace(userAgent) ? DefaultUserAgent : userAgent;
            _browserFp = string.IsNullOrWhiteSpace(browserFp) ? BrowserFingerprintGenerator.GenerateFingerprint() : browserFp;
            _options = options ?? new[] { 0, 1, 14 };
            _cryptoUtility = new CryptoUtility(Salt, new[] { Character, Character2 });
        }

        private const string DefaultUserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36 Edg/130.0.0.0";

        public string GenerateAbogus(string @params, string body)
        {
            var abDir = new Dictionary<int, int>
            {
                [8] = 3,
                [18] = 44,
                [66] = 0,
                [69] = 0,
                [70] = 0,
                [71] = 0
            };

            var start = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            var array1 = _cryptoUtility.ParamsToArray(_cryptoUtility.ParamsToArray(@params));
            var array2 = _cryptoUtility.ParamsToArray(_cryptoUtility.ParamsToArray(body));
            var rc4Bytes = CryptoUtility.Rc4Encrypt(UaKey, _userAgent);
            var array3 = _cryptoUtility.ParamsToArray(
                _cryptoUtility.Base64Encode(StringProcessor.ToCharStr(rc4Bytes.Select(b => (int)b).ToList()), 1),
                addSalt: false);

            var end = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            abDir[20] = (int)((start >> 24) & 255);
            abDir[21] = (int)((start >> 16) & 255);
            abDir[22] = (int)((start >> 8) & 255);
            abDir[23] = (int)(start & 255);
            abDir[24] = StringProcessor.JsShiftRight(start, 32);
            abDir[25] = StringProcessor.JsShiftRight(start, 40);

            abDir[26] = (_options[0] >> 24) & 255;
            abDir[27] = (_options[0] >> 16) & 255;
            abDir[28] = (_options[0] >> 8) & 255;
            abDir[29] = _options[0] & 255;

            abDir[30] = (_options[1] / 256) & 255;
            abDir[31] = _options[1] % 256;
            abDir[32] = (_options[1] >> 24) & 255;
            abDir[33] = (_options[1] >> 16) & 255;

            abDir[34] = (_options[2] >> 24) & 255;
            abDir[35] = (_options[2] >> 16) & 255;
            abDir[36] = (_options[2] >> 8) & 255;
            abDir[37] = _options[2] & 255;

            abDir[38] = array1[21];
            abDir[39] = array1[22];
            abDir[40] = array2[21];
            abDir[41] = array2[22];
            abDir[42] = array3[23];
            abDir[43] = array3[24];

            abDir[44] = (int)((end >> 24) & 255);
            abDir[45] = (int)((end >> 16) & 255);
            abDir[46] = (int)((end >> 8) & 255);
            abDir[47] = (int)(end & 255);
            abDir[48] = abDir[8];
            abDir[49] = StringProcessor.JsShiftRight(end, 32);
            abDir[50] = StringProcessor.JsShiftRight(end, 40);

            abDir[51] = (PageId >> 24) & 255;
            abDir[52] = (PageId >> 16) & 255;
            abDir[53] = (PageId >> 8) & 255;
            abDir[54] = PageId & 255;
            abDir[55] = PageId;
            abDir[56] = Aid;
            abDir[57] = Aid & 255;
            abDir[58] = (Aid >> 8) & 255;
            abDir[59] = (Aid >> 16) & 255;
            abDir[60] = (Aid >> 24) & 255;

            abDir[64] = _browserFp.Length;
            abDir[65] = _browserFp.Length;

            var sortedValues = _sortIndex.Select(i => abDir.TryGetValue(i, out var v) ? v : 0).ToList();
            var edgeFpArray = StringProcessor.ToOrdArray(_browserFp);

            var abXor = 0;
            for (var index = 0; index < _sortIndex2.Length; index++)
            {
                var value = abDir.TryGetValue(_sortIndex2[index], out var v) ? v : 0;
                abXor = index == 0 ? value : (abXor ^ value);
            }

            sortedValues.AddRange(edgeFpArray);
            sortedValues.Add(abXor);

            var abogusBytesStr = StringProcessor.GenerateRandomBytes() + _cryptoUtility.TransformBytes(sortedValues);
            var abogus = _cryptoUtility.AbogusEncode(abogusBytesStr, 0);
            return $"{@params}&a_bogus={abogus}";
        }
    }

    internal static class Sm3
    {
        private static readonly uint[] Iv =
        {
            0x7380166Fu, 0x4914B2B9u, 0x172442D7u, 0xDA8A0600u,
            0xA96F30BCu, 0x163138AAu, 0xE38DEE4Du, 0xB0FB0E4Eu
        };

        public static List<int> ToArray(string input)
        {
            return ToArray(Encoding.UTF8.GetBytes(input));
        }

        public static List<int> ToArray(IReadOnlyList<int> bytes)
        {
            var raw = bytes.Select(b => (byte)b).ToArray();
            return ToArray(raw);
        }

        public static List<int> ToArray(byte[] bytes)
        {
            var hash = Hash(bytes);
            return hash.Select(b => (int)b).ToList();
        }

        private static byte[] Hash(byte[] data)
        {
            var padded = Pad(data);
            uint a, b, c, d, e, f, g, h;
            var v = (uint[])Iv.Clone();
            var w = new uint[68];
            var w1 = new uint[64];

            for (var i = 0; i < padded.Length; i += 64)
            {
                for (var j = 0; j < 16; j++)
                {
                    w[j] = ToUInt32(padded, i + j * 4);
                }
                for (var j = 16; j < 68; j++)
                {
                    var x = w[j - 16] ^ w[j - 9] ^ Rotl(w[j - 3], 15);
                    w[j] = P1(x) ^ Rotl(w[j - 13], 7) ^ w[j - 6];
                }
                for (var j = 0; j < 64; j++)
                {
                    w1[j] = w[j] ^ w[j + 4];
                }

                a = v[0]; b = v[1]; c = v[2]; d = v[3];
                e = v[4]; f = v[5]; g = v[6]; h = v[7];

                for (var j = 0; j < 64; j++)
                {
                    var ss1 = Rotl((Rotl(a, 12) + e + Rotl(T(j), j)) & 0xFFFFFFFFu, 7);
                    var ss2 = ss1 ^ Rotl(a, 12);
                    var tt1 = (Ff(a, b, c, j) + d + ss2 + w1[j]) & 0xFFFFFFFFu;
                    var tt2 = (Gg(e, f, g, j) + h + ss1 + w[j]) & 0xFFFFFFFFu;
                    d = c;
                    c = Rotl(b, 9);
                    b = a;
                    a = tt1;
                    h = g;
                    g = Rotl(f, 19);
                    f = e;
                    e = P0(tt2);
                }

                v[0] ^= a;
                v[1] ^= b;
                v[2] ^= c;
                v[3] ^= d;
                v[4] ^= e;
                v[5] ^= f;
                v[6] ^= g;
                v[7] ^= h;
            }

            var output = new byte[32];
            for (var i = 0; i < 8; i++)
            {
                FromUInt32(v[i], output, i * 4);
            }
            return output;
        }

        private static byte[] Pad(byte[] input)
        {
            var len = input.Length;
            var bitLen = (ulong)len * 8;
            var k = (56 - (len + 1) % 64 + 64) % 64;
            var padded = new byte[len + 1 + k + 8];
            Buffer.BlockCopy(input, 0, padded, 0, len);
            padded[len] = 0x80;
            for (var i = 0; i < 8; i++)
            {
                padded[padded.Length - 1 - i] = (byte)((bitLen >> (8 * i)) & 0xFF);
            }
            return padded;
        }

        private static uint ToUInt32(byte[] data, int offset)
        {
            return (uint)(data[offset] << 24 | data[offset + 1] << 16 | data[offset + 2] << 8 | data[offset + 3]);
        }

        private static void FromUInt32(uint n, byte[] output, int offset)
        {
            output[offset] = (byte)(n >> 24);
            output[offset + 1] = (byte)(n >> 16);
            output[offset + 2] = (byte)(n >> 8);
            output[offset + 3] = (byte)n;
        }

        private static uint Rotl(uint x, int n)
        {
            return (x << n) | (x >> (32 - n));
        }

        private static uint P0(uint x) => x ^ Rotl(x, 9) ^ Rotl(x, 17);
        private static uint P1(uint x) => x ^ Rotl(x, 15) ^ Rotl(x, 23);

        private static uint T(int j) => j <= 15 ? 0x79CC4519u : 0x7A879D8Au;

        private static uint Ff(uint x, uint y, uint z, int j)
        {
            return j <= 15 ? (x ^ y ^ z) : ((x & y) | (x & z) | (y & z));
        }

        private static uint Gg(uint x, uint y, uint z, int j)
        {
            return j <= 15 ? (x ^ y ^ z) : ((x & y) | (~x & z));
        }
    }
}
