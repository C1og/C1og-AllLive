using System;
using System.Threading.Tasks;
using Windows.Storage;

namespace AllLive.UWP.Helper
{
    public static class DouyinCookieStore
    {
        private const string FileName = "douyin_cookie.txt";

        public static async Task<string> LoadAsync()
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                var file = await folder.TryGetItemAsync(FileName).AsTask().ConfigureAwait(false) as StorageFile;
                if (file == null)
                {
                    return "";
                }
                return await FileIO.ReadTextAsync(file).AsTask().ConfigureAwait(false);
            }
            catch (Exception)
            {
                return "";
            }
        }

        public static async Task SaveAsync(string value)
        {
            try
            {
                var folder = ApplicationData.Current.LocalFolder;
                if (string.IsNullOrWhiteSpace(value))
                {
                    var item = await folder.TryGetItemAsync(FileName).AsTask().ConfigureAwait(false);
                    if (item != null)
                    {
                        await item.DeleteAsync().AsTask().ConfigureAwait(false);
                    }
                    return;
                }
                var file = await folder.CreateFileAsync(FileName, CreationCollisionOption.ReplaceExisting)
                    .AsTask()
                    .ConfigureAwait(false);
                await FileIO.WriteTextAsync(file, value).AsTask().ConfigureAwait(false);
            }
            catch (Exception)
            {
            }
        }
    }
}
