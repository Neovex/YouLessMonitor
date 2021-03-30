using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace YouLessAPI
{
    public enum Resolution
    {
        Minutes,
        DecaMinutes,
        Hours,
        Days
    }


    public class YouLessService
    {
        private readonly char[] _Resolutions = new[] { 'h', 'w', 'd', 'm' };


        public Uri BaseAdress { get; }

        
        public YouLessService(Uri baseAdress)
        {
            if (baseAdress == null) throw new ArgumentNullException(nameof(baseAdress));
            if (String.IsNullOrWhiteSpace(baseAdress.AbsoluteUri)) throw new ArgumentException($"{nameof(baseAdress)} must not be empty");
            BaseAdress = baseAdress;
        }

        public async Task<StatusInfo> GetStatus()
        {
            var rawString = await GetString(new Uri(BaseAdress, new Uri("/a?f=j", UriKind.Relative)));
            return (StatusInfo)JsonConvert.DeserializeObject(rawString, typeof(StatusInfo));
        }

        public async Task<History> GetHistory(int page = 1, Resolution res = Resolution.Minutes)
        {
            var rawString = await GetString(new Uri(BaseAdress, new Uri($"/V?{_Resolutions[(int)res]}={page}&f=j", UriKind.Relative)));
            var history = (History)JsonConvert.DeserializeObject(rawString, typeof(History));
            if (history.Samples.Count != 0) history.Samples.RemoveAt(history.Samples.Count - 1);
            return history;
        }

        private async Task<string> GetString(Uri uri)
        {
            using (var client = new HttpClient())
            {
                var reply = await client.GetAsync(uri);
                return await reply.Content.ReadAsStringAsync();
            }
        }
    }
}