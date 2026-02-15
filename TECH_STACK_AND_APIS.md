# AllLive Tech Stack and API Surface

This document summarizes the technical stack and the external API endpoints referenced in the repo.

## Solution layout
- `AllLive.Core`: netstandard2.0 core library that implements site adapters, danmaku clients, and shared models.
- `AllLive.UWP`: UWP (UAP 10.x) client with XAML UI, playback, storage, sync, and settings.
- `AllLive.Console`: .NET 8 console app that uses `AllLive.Core`.

## Key dependencies
- `Newtonsoft.Json`: JSON parsing/serialization.
- `WebSocketSharp-netstandard-customheaders`: websocket client for danmaku.
- `protobuf-net`: Douyin danmaku payload decoding.
- `QuickJS.NET`: JS execution for Douyu signing (non-UWP only).
- `FFmpegInteropX` + `FFmpegInteropX.FFmpegUWP`: media playback in UWP.
- `Microsoft.AspNetCore.SignalR.Client`: cross-device sync channel.
- `Microsoft.Data.Sqlite` + `SQLitePCLRaw.bundle_e_sqlite3`: local storage.
- `NLog`: logging.
- `NSDanmaku`: danmaku rendering in UWP.
- `WebDav.Client`: sync/storage helper.
- `ZXing.Net`: QR code generation for Bilibili login.

## Internal interfaces
- `ILiveSite`: site adapter interface for categories, search, room detail, play quality, play urls, live status, and super chat.
- `ILiveDanmaku`: danmaku client interface with Start/Stop, heartbeat, and message events.

## Shared networking utilities
- `HttpUtil`: wrapper around `HttpClient` for GET/POST/HEAD and JSON bodies.

## External APIs by platform

### Bilibili
Core usage lives in `AllLive.Core/BiliBili.cs` and `AllLive.Core/Danmaku/BiliBiliDanmaku.cs`.

- Categories: `https://api.live.bilibili.com/room/v1/Area/getList`
- Category rooms: `https://api.live.bilibili.com/xlive/web-interface/v1/second/getList`
- Recommend rooms: `https://api.live.bilibili.com/xlive/web-interface/v1/second/getListByArea`
- Room detail: `https://api.live.bilibili.com/xlive/web-room/v1/index/getInfoByRoom`
- Search: `https://api.bilibili.com/x/web-interface/search/type`
- Play info (new): `https://api.live.bilibili.com/xlive/web-room/v2/index/getRoomPlayInfo`
- Play info (old): `https://api.live.bilibili.com/room/v1/Room/playUrl`
- Live status: `https://api.live.bilibili.com/room/v1/Room/get_info`
- Super chat list: `https://api.live.bilibili.com/av/v1/SuperChat/getMessageList`
- WBI keys: `https://api.bilibili.com/x/web-interface/nav`
- BUVID: `https://api.bilibili.com/x/frontend/finger/spi`
- Access id: `https://live.bilibili.com/lol`
- Danmaku info: `https://api.live.bilibili.com/xlive/web-room/v1/index/getDanmuInfo`
- Danmaku websocket: `wss://{host}/sub` (host from `getDanmuInfo`)

### Douyin
Core usage lives in `AllLive.Core/Douyin.cs` and `AllLive.Core/Danmaku/DouyinDanmaku.cs`.

- Categories & list: `https://live.douyin.com/webcast/web/partition/detail/room/v2/`
- Room enter (web): `https://live.douyin.com/webcast/room/web/enter/`
- Room info (reflow): `https://webcast.amemv.com/webcast/room/reflow/info/`
- Search: `https://www.douyin.com/aweme/v1/web/live/search/`
- a_bogus signing service: `https://dy.nsapps.cn/abogus`
- Websocket signature service: `https://dy.nsapps.cn/signature`
- Danmaku websocket: `wss://webcast3-ws-web-lq.douyin.com/webcast/im/push/v2/`

### Douyu
Core usage lives in `AllLive.Core/Douyu.cs` and `AllLive.Core/Danmaku/DouyuDanmaku.cs`.

- Categories: `https://m.douyu.com/api/cate/list`
- Category rooms: `https://www.douyu.com/gapi/rkc/directory/mixList/2_{categoryId}/{page}`
- Recommend rooms: `https://www.douyu.com/japi/weblist/apinc/allpage/6/{page}`
- Room detail: `https://www.douyu.com/betard/{roomId}`
- Room sign (html): `https://www.douyu.com/swf_api/homeH5Enc?rids={roomId}`
- Search: `https://www.douyu.com/japi/search/api/searchShow`
- Play info: `https://www.douyu.com/lapi/live/getH5Play/{roomId}`
- UWP sign service: `http://alive.nsapps.cn/api/AllLive/DouyuSign`
- Danmaku websocket: `wss://danmuproxy.douyu.com:8506`

### Huya
Core usage lives in `AllLive.Core/Huya.cs` and `AllLive.Core/Danmaku/HuyaDanmaku.cs`.

- Category list: `https://live.cdn.huya.com/liveconfig/game/bussLive?bussType={id}`
- Category rooms: `https://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&gameId={id}&page={page}`
- Recommend rooms: `https://www.huya.com/cache.php?m=LiveList&do=getLiveListByPage&tagAll=0&page={page}`
- Room page: `https://m.huya.com/{roomId}`
- Search: `https://search.cdn.huya.com/?m=Search&do=getSearchContent...`
- Anonymous login: `https://udblgn.huya.com/web/anonymousLogin`
- CDN token (Tars over HTTP): `http://wup.huya.com`
- Danmaku websocket: `wss://cdnws.api.huya.com`

## UWP app services
- Bilibili login QR:
  - `https://passport.bilibili.com/x/passport-login/web/qrcode/generate`
  - `https://passport.bilibili.com/x/passport-login/web/qrcode/poll`
- Bilibili account info: `https://api.bilibili.com/x/member/web/account`
- Cross-device sync (SignalR): `https://sync1.nsapps.cn/sync`
- Version check: `https://cdn.jsdelivr.net/gh/xiaoyaocz/AllLive@master/AllLive.UWP/version.json`

## Notes on protocol handling
- Bilibili uses WBI signing for some endpoints and BUVID cookies for auth.
- Douyin uses a_bogus signing and a websocket signature service.
- Douyu uses JS-based signing for play info; UWP uses an external sign service.
- Huya playback URLs rely on Tars-encoded token requests.
