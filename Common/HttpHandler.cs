using System;
using System.Net.Http;

namespace DarkBot.Common
{
    public abstract class HttpHandler
    {
        private static readonly HttpClientHandler Handler = new HttpClientHandler() { AllowAutoRedirect = false };
        protected static readonly HttpClient Http = new HttpClient(Handler, true);
        protected static readonly Random random = new Random();
    }
}