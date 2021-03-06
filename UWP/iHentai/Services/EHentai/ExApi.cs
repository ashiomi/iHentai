﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using iHentai.Common;
using iHentai.Common.Helpers;
using Microsoft.Toolkit.Helpers;

namespace iHentai.Services.EHentai
{
    internal class ExApi : EHApi, ICustomHttpHandler
    {
        private const string COOKIE_KEY = "exhentai_cookie";
        public override string Host => "https://exhentai.org/";
        public bool RequireLogin => string.IsNullOrEmpty(GetCookie());

        public ExApi()
        {
            Singleton<HentaiHttpMessageHandler>.Instance.RegisterHandler(this);
        }

        public bool CanHandle(Uri uri)
        {
            if (uri.Host.Equals("exhentai.org", StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            if (uri.Host.Equals("e-hentai.org", StringComparison.InvariantCultureIgnoreCase) && !string.IsNullOrEmpty(GetCookie()))
            {
                return true;
            }

            return false;
        }

        public void Handle(HttpRequestMessage message)
        {
            var cookie = GetCookie();
            message.Headers.Add("Cookie", cookie);
        }

        private string GetCookie()
        {
            return Singleton<Settings>.Instance.Get(COOKIE_KEY, string.Empty);
        }
        

        private void SetCookie(string cookie)
        {
            Singleton<Settings>.Instance.Set(COOKIE_KEY, cookie.TrimEnd(';'));
        }
        
        public async Task Login(string userName, string password)
        {
            using var handler = new NoCookieHttpMessageHandler();
            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Post,
                "http://forums.e-hentai.org/index.php?act=Login&CODE=01&CookieDate=1")
            {
                Content = new FormUrlEncodedContent(new[]
                {
                    KeyValuePair.Create("UserName", userName), KeyValuePair.Create("PassWord", password)
                })
            };
            using var response = await client.SendAsync(request, completionOption: HttpCompletionOption.ResponseHeadersRead);
            response.Headers.TryGetValues("Set-Cookie", out var cookies);
            var cookie = cookies
                .Select(item => item.Split(';').FirstOrDefault())
                .Let(it => string.Join((string) ";", (IEnumerable<string>) it));
            cookie += ";" + await UpdateCookie(cookie);
            SetCookie(cookie);
        }

        private async Task<string> UpdateCookie(string cookie)
        {
            using var handler = new NoCookieHttpMessageHandler();
            using var client = new HttpClient(handler);
            using var request = new HttpRequestMessage(HttpMethod.Get, "https://exhentai.org/mytags ");
            request.Headers.Add("Cookie", $"{cookie};igneous=");
            using var response = await client.SendAsync(request);
            response.Headers.TryGetValues("Set-Cookie", out var cookies);
            return cookies
                .Select(item => item.Split(';').FirstOrDefault())
                .Let(it => string.Join(";", it));
        }
    }
}