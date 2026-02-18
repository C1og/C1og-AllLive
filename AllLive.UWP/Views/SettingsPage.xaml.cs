using AllLive.UWP.Helper;
using CoreConfig = AllLive.Core.Helper.CoreConfig;
using AllLive.UWP.ViewModels;
using Microsoft.Toolkit.Uwp.Helpers;
using Microsoft.UI.Xaml.Controls;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace AllLive.UWP.Views
{
    /// <summary>
    /// 可用于自身或导航至 Frame 内部的空白页。
    /// </summary>
    public sealed partial class SettingsPage : Page
    {
        readonly SettingVM settingVM;
        private bool _isUpdatingDouyinCookie;
        public SettingsPage()
        {
            settingVM = new SettingVM();
            this.InitializeComponent();
            if (Utils.IsXbox)
            {
                SettingsPaneDiaplsyMode.Visibility = Visibility.Collapsed;
                SettingsMouseClosePage.Visibility = Visibility.Collapsed;
                SettingsFontSize.Visibility = Visibility.Collapsed;
                SettingsAutoClean.Visibility = Visibility.Collapsed;
                SettingsXboxMode.Visibility = Visibility.Visible;
                SettingsNewWindow.Visibility = Visibility.Collapsed;
            }
            BiliAccount.Instance.OnAccountChanged += BiliAccount_OnAccountChanged; 
            LoadUI();

        }

        private void BiliAccount_OnAccountChanged(object sender, EventArgs e)
        {
            if (BiliAccount.Instance.Logined)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
            else
            {
                txtBili.Text = "登录可享受高清直播";
                BtnLoginBili.Visibility = Visibility.Visible;
                BtnLogoutBili.Visibility = Visibility.Collapsed;
            }
        }

        private void LoadUI()
        {
            //主题
            cbTheme.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.THEME, 0);
            cbTheme.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbTheme.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.THEME, cbTheme.SelectedIndex);
                    Frame rootFrame = Window.Current.Content as Frame;
                    switch (cbTheme.SelectedIndex)
                    {
                        case 1:
                            rootFrame.RequestedTheme = ElementTheme.Light;
                            break;
                        case 2:
                            rootFrame.RequestedTheme = ElementTheme.Dark;
                            break;
                        default:
                            rootFrame.RequestedTheme = ElementTheme.Default;
                            break;
                    }
                    App.SetTitleBar();
                });
            });

            // xbox操作模式
            cbXboxMode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.XBOX_MODE, 0);
            cbXboxMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbXboxMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.XBOX_MODE, cbXboxMode.SelectedIndex);
                    Utils.ShowMessageToast("重启应用生效");
                });
            });

            //导航栏显示模式
            cbPaneDisplayMode.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.PANE_DISPLAY_MODE, 0);
            cbPaneDisplayMode.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbPaneDisplayMode.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.PANE_DISPLAY_MODE, cbPaneDisplayMode.SelectedIndex);
                    MessageCenter.UpdatePanelDisplayMode();
                });
            });

            //鼠标侧键返回
            swMouseClosePage.IsOn = SettingHelper.GetValue<bool>(SettingHelper.MOUSE_BACK, true);
            swMouseClosePage.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swMouseClosePage.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.MOUSE_BACK, swMouseClosePage.IsOn);
                });
            });

            //日志开关
            swLogEnabled.IsOn = SettingHelper.GetValue<bool>(SettingHelper.LOG_ENABLED, false);
            LogHelper.SetEnabled(swLogEnabled.IsOn);
            swLogEnabled.Toggled += new RoutedEventHandler((sender, e) =>
            {
                SettingHelper.SetValue(SettingHelper.LOG_ENABLED, swLogEnabled.IsOn);
                LogHelper.SetEnabled(swLogEnabled.IsOn);
            });

            //关注列表自动刷新间隔
            var favoriteRefreshMinutes = SettingHelper.GetValue<int>(SettingHelper.FAVORITE_AUTO_REFRESH_MINUTES, 5);
            if (favoriteRefreshMinutes < 1)
            {
                favoriteRefreshMinutes = 5;
            }
            numFavoriteRefreshInterval.Value = favoriteRefreshMinutes;
            numFavoriteRefreshInterval.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numFavoriteRefreshInterval.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    if (double.IsNaN(args.NewValue))
                    {
                        return;
                    }
                    var minutes = Convert.ToInt32(args.NewValue);
                    if (minutes < 1)
                    {
                        minutes = 1;
                    }
                    SettingHelper.SetValue(SettingHelper.FAVORITE_AUTO_REFRESH_MINUTES, minutes);
                });
            });
            //视频解码
            cbDecoder.SelectedIndex = SettingHelper.GetValue<int>(SettingHelper.VIDEO_DECODER, 1);
            cbDecoder.Loaded += new RoutedEventHandler((sender, e) =>
            {
                cbDecoder.SelectionChanged += new SelectionChangedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.VIDEO_DECODER, cbDecoder.SelectedIndex);
                });
            });

            swDouyuSignService.IsOn = SettingHelper.GetValue<bool>(SettingHelper.DOUYU_SIGN_ENABLED, true);
            swDouyuSignService.Toggled += new RoutedEventHandler((obj, args) =>
            {
                SettingHelper.SetValue(SettingHelper.DOUYU_SIGN_ENABLED, swDouyuSignService.IsOn);
                txtDouyuSignUrl.IsEnabled = swDouyuSignService.IsOn;
                ApplyDouyuSignServiceSetting(txtDouyuSignUrl.Text, swDouyuSignService.IsOn);
                _ = UpdateDouyuSignStatusAsync();
            });

            //斗鱼签名服务地址
            txtDouyuSignUrl.Text = SettingHelper.GetValue<string>(SettingHelper.DOUYU_SIGN_URL, SettingHelper.DOUYU_SIGN_URL_DEFAULT);
            txtDouyuSignUrl.IsEnabled = swDouyuSignService.IsOn;
            ApplyDouyuSignServiceSetting(txtDouyuSignUrl.Text, swDouyuSignService.IsOn);
            _ = UpdateDouyuSignStatusAsync();
            txtDouyuSignUrl.Loaded += new RoutedEventHandler((sender, e) =>
            {
                txtDouyuSignUrl.TextChanged += new TextChangedEventHandler((obj, args) =>
                {
                    var value = txtDouyuSignUrl.Text?.Trim() ?? "";
                    SettingHelper.SetValue(SettingHelper.DOUYU_SIGN_URL, value);
                    ApplyDouyuSignServiceSetting(value, swDouyuSignService.IsOn);
                });
            });

            swDouyinSignService.IsOn = SettingHelper.GetValue<bool>(SettingHelper.DOUYIN_SIGN_ENABLED, true);
            swDouyinSignService.Toggled += new RoutedEventHandler((obj, args) =>
            {
                SettingHelper.SetValue(SettingHelper.DOUYIN_SIGN_ENABLED, swDouyinSignService.IsOn);
                txtDouyinSignUrl.IsEnabled = swDouyinSignService.IsOn;
                ApplyDouyinSignServiceSetting(txtDouyinSignUrl.Text, swDouyinSignService.IsOn);
                _ = UpdateDouyinSignStatusAsync();
            });

            txtDouyinSignUrl.Text = SettingHelper.GetValue<string>(SettingHelper.DOUYIN_SIGN_URL, SettingHelper.DOUYIN_SIGN_URL_DEFAULT);
            txtDouyinSignUrl.IsEnabled = swDouyinSignService.IsOn;
            ApplyDouyinSignServiceSetting(txtDouyinSignUrl.Text, swDouyinSignService.IsOn);
            _ = UpdateDouyinSignStatusAsync();
            txtDouyinSignUrl.Loaded += new RoutedEventHandler((sender, e) =>
            {
                txtDouyinSignUrl.TextChanged += new TextChangedEventHandler((obj, args) =>
                {
                    var value = txtDouyinSignUrl.Text?.Trim() ?? "";
                    SettingHelper.SetValue(SettingHelper.DOUYIN_SIGN_URL, value);
                    ApplyDouyinSignServiceSetting(value, swDouyinSignService.IsOn);
                });
            });

            numFontsize.Value = SettingHelper.GetValue<double>(SettingHelper.MESSAGE_FONTSIZE, 14.0);
            numFontsize.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numFontsize.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.MESSAGE_FONTSIZE, args.NewValue);
                });
            });

            //新窗口打开
            swNewWindow.IsOn = SettingHelper.GetValue<bool>(SettingHelper.NEW_WINDOW_LIVEROOM, true);
            swNewWindow.Loaded += new RoutedEventHandler((sender, e) =>
            {
                swNewWindow.Toggled += new RoutedEventHandler((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.NEW_WINDOW_LIVEROOM, swNewWindow.IsOn);
                });
            });
            //弹幕开关
            var state = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.SHOW, false);
            DanmuSettingState.IsOn = state;
            DanmuSettingState.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHOW, DanmuSettingState.IsOn);
            });

            // 保留醒目留言
            var keepSC = SettingHelper.GetValue<bool>(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, true);
            SettingKeepSC.IsOn = keepSC;
            SettingKeepSC.Toggled += new RoutedEventHandler((e, args) =>
            {
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.KEEP_SUPER_CHAT, SettingKeepSC.IsOn);
            });

            //弹幕清理
            numCleanCount.Value = SettingHelper.GetValue<int>(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, 200);
            numCleanCount.Loaded += new RoutedEventHandler((sender, e) =>
            {
                numCleanCount.ValueChanged += new TypedEventHandler<NumberBox, NumberBoxValueChangedEventArgs>((obj, args) =>
                {
                    SettingHelper.SetValue(SettingHelper.LiveDanmaku.DANMU_CLEAN_COUNT, Convert.ToInt32(args.NewValue));
                });
            });
            //弹幕关键词
            LiveDanmuSettingListWords.ItemsSource = settingVM.ShieldWords;

            // 抖音Cookie
            _ = LoadDouyinCookieAsync();
            txtDouyinCookie.Loaded += new RoutedEventHandler((sender, e) =>
            {
                txtDouyinCookie.TextChanged += new TextChangedEventHandler((obj, args) =>
                {
                    if (_isUpdatingDouyinCookie)
                    {
                        return;
                    }
                    var rawValue = txtDouyinCookie.Text ?? "";
                    var normalized = NormalizeDouyinCookieInput(rawValue);
                    var trimmed = TrimDouyinCookieForSettings(normalized);
                    SettingHelper.SetValue(SettingHelper.DOUYIN_COOKIE, trimmed);
                    ApplyDouyinCookieSetting(normalized);
                    _ = DouyinCookieStore.SaveAsync(normalized);
                    if (!string.Equals(rawValue, normalized, StringComparison.Ordinal))
                    {
                        _isUpdatingDouyinCookie = true;
                        txtDouyinCookie.Text = normalized;
                        txtDouyinCookie.SelectionStart = txtDouyinCookie.Text.Length;
                        _isUpdatingDouyinCookie = false;
                    }
                });
            });


            if(BiliAccount.Instance.Logined)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
           
        }

        private static void ApplyDouyuSignServiceSetting(string value, bool enabled)
        {
            var url = string.IsNullOrWhiteSpace(value) ? SettingHelper.DOUYU_SIGN_URL_DEFAULT : value.Trim();
            if (enabled)
            {
                CoreConfig.SetDouyuSignServiceUrl(url);
            }
            else
            {
                CoreConfig.SetDouyuSignServiceUrl(SettingHelper.DOUYU_SIGN_URL_PUBLIC);
            }
        }

        private static void ApplyDouyinSignServiceSetting(string value, bool enabled)
        {
            var url = string.IsNullOrWhiteSpace(value) ? SettingHelper.DOUYIN_SIGN_URL_DEFAULT : value.Trim();
            if (enabled)
            {
                CoreConfig.SetDouyinSignServiceUrl(url);
            }
            else
            {
                CoreConfig.SetDouyinSignServiceUrl(SettingHelper.DOUYIN_SIGN_URL_PUBLIC);
            }
        }

        private static void ApplyDouyinCookieSetting(string value)
        {
            CoreConfig.SetDouyinCookie(value);
        }

        private async Task LoadDouyinCookieAsync()
        {
            var cookie = await DouyinCookieStore.LoadAsync();
            if (string.IsNullOrWhiteSpace(cookie))
            {
                cookie = SettingHelper.GetValue<string>(SettingHelper.DOUYIN_COOKIE, "");
            }
            _isUpdatingDouyinCookie = true;
            txtDouyinCookie.Text = cookie ?? "";
            txtDouyinCookie.SelectionStart = txtDouyinCookie.Text.Length;
            _isUpdatingDouyinCookie = false;
            ApplyDouyinCookieSetting(cookie ?? "");
        }

        private static string NormalizeDouyinCookieInput(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            var cookie = value.Trim();
            if (cookie.StartsWith("Cookie:", StringComparison.OrdinalIgnoreCase))
            {
                cookie = cookie.Substring("Cookie:".Length).Trim();
            }
            return cookie;
        }

        private static string TrimDouyinCookieForSettings(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            var keepKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "ttwid",
                "msToken",
                "__ac_nonce",
                "s_v_web_id"
            };
            var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            var parts = value.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var kv = part.Split(new[] { '=' }, 2);
                var key = kv[0].Trim();
                if (string.IsNullOrEmpty(key) || !keepKeys.Contains(key))
                {
                    continue;
                }
                var val = kv.Length > 1 ? kv[1].Trim() : "";
                map[key] = val;
            }
            if (map.Count == 0)
            {
                return "";
            }
            var orderedKeys = new[] { "ttwid", "msToken", "__ac_nonce", "s_v_web_id" };
            var items = new List<string>();
            foreach (var key in orderedKeys)
            {
                if (map.TryGetValue(key, out var val))
                {
                    items.Add($"{key}={val}");
                }
            }
            return string.Join("; ", items);
        }

        private async void BtnDouyuSignCheck_Click(object sender, RoutedEventArgs e)
        {
            await UpdateDouyuSignStatusAsync(true);
        }

        private async void BtnDouyinSignCheck_Click(object sender, RoutedEventArgs e)
        {
            await UpdateDouyinSignStatusAsync(true);
        }

        private async Task UpdateDouyuSignStatusAsync(bool showToast = false)
        {
            if (!swDouyuSignService.IsOn)
            {
                txtDouyuSignStatus.Text = "已关闭";
                return;
            }

            var url = string.IsNullOrWhiteSpace(txtDouyuSignUrl.Text)
                ? SettingHelper.DOUYU_SIGN_URL_DEFAULT
                : txtDouyuSignUrl.Text.Trim();
            var healthUrl = BuildHealthUrl(url);
            txtDouyuSignStatus.Text = "检测中...";

            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
                using (var response = await client.GetAsync(healthUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        txtDouyuSignStatus.Text = "运行中";
                        if (showToast)
                        {
                            Utils.ShowMessageToast("签名服务运行正常");
                        }
                        return;
                    }
                    txtDouyuSignStatus.Text = $"异常({(int)response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                txtDouyuSignStatus.Text = "不可用";
                if (showToast)
                {
                    Utils.ShowMessageToast($"签名服务不可用：{ex.Message}");
                }
            }
        }

        private async Task UpdateDouyinSignStatusAsync(bool showToast = false)
        {
            if (!swDouyinSignService.IsOn)
            {
                txtDouyinSignStatus.Text = "已关闭";
                return;
            }

            var url = string.IsNullOrWhiteSpace(txtDouyinSignUrl.Text)
                ? SettingHelper.DOUYIN_SIGN_URL_DEFAULT
                : txtDouyinSignUrl.Text.Trim();
            var healthUrl = BuildHealthUrl(url);
            txtDouyinSignStatus.Text = "检测中...";

            try
            {
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(2) })
                using (var response = await client.GetAsync(healthUrl))
                {
                    if (response.IsSuccessStatusCode)
                    {
                        txtDouyinSignStatus.Text = "运行中";
                        if (showToast)
                        {
                            Utils.ShowMessageToast("签名服务运行正常");
                        }
                        return;
                    }
                    txtDouyinSignStatus.Text = $"异常({(int)response.StatusCode})";
                }
            }
            catch (Exception ex)
            {
                txtDouyinSignStatus.Text = "不可用";
                if (showToast)
                {
                    Utils.ShowMessageToast($"签名服务不可用：{ex.Message}");
                }
            }
        }

        private static string BuildHealthUrl(string signUrl)
        {
            if (Uri.TryCreate(signUrl, UriKind.Absolute, out var uri))
            {
                var builder = new UriBuilder(uri)
                {
                    Path = "/health",
                    Query = ""
                };
                return builder.Uri.ToString();
            }
            return signUrl.TrimEnd('/') + "/health";
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            version.Text = $"{SystemInformation.Instance.ApplicationVersion.Major}.{SystemInformation.Instance.ApplicationVersion.Minor}.{SystemInformation.Instance.ApplicationVersion.Build}";
        }
        private void RemoveLiveDanmuWord_Click(object sender, RoutedEventArgs e)
        {
            var word = (sender as AppBarButton).DataContext as string;
            settingVM.ShieldWords.Remove(word);
            SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
        }

        private void LiveDanmuSettingTxtWord_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
        {
            if (string.IsNullOrEmpty(LiveDanmuSettingTxtWord.Text))
            {
                Utils.ShowMessageToast("关键字不能为空");
                return;
            }
            if (!settingVM.ShieldWords.Contains(LiveDanmuSettingTxtWord.Text))
            {
                settingVM.ShieldWords.Add(LiveDanmuSettingTxtWord.Text);
                SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
            }

            LiveDanmuSettingTxtWord.Text = "";
            SettingHelper.SetValue(SettingHelper.LiveDanmaku.SHIELD_WORD, JsonConvert.SerializeObject(settingVM.ShieldWords));
        }

        private async void BtnProjectFolder_Click(object sender, RoutedEventArgs e)
        {
            await Launcher.LaunchFolderPathAsync(@"D:\D-Software\C1og-AllLive");
        }

        private async void BtnLog_Click(object sender, RoutedEventArgs e)
        {
            Windows.Storage.StorageFolder storageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var logFolder = await storageFolder.CreateFolderAsync("log", Windows.Storage.CreationCollisionOption.OpenIfExists);
            await Launcher.LaunchFolderAsync(logFolder);
        }

        private async void BtnLoginBili_Click(object sender, RoutedEventArgs e)
        {
            if (BiliAccount.Instance.Logined)
            {
                Utils.ShowMessageToast("已登录");
                return;
            }
            var result= await MessageCenter.BiliBiliLogin();
            if (result)
            {
                txtBili.Text = $"已登录：{BiliAccount.Instance.UserName}";
                BtnLoginBili.Visibility = Visibility.Collapsed;
                BtnLogoutBili.Visibility = Visibility.Visible;
            }
        }

        private void BtnLogoutBili_Click(object sender, RoutedEventArgs e)
        {
            BiliAccount.Instance.Logout();
            txtBili.Text = "登录可享受高清直播";
            BtnLoginBili.Visibility = Visibility.Visible;
            BtnLogoutBili.Visibility = Visibility.Collapsed;

        }
    }
}
