using System;

namespace Surveillance.Steam.Response
{
    public struct SteamApiResponse<T>
    {
        public DateTimeOffset Expires { get; set; }
        public T Content { get; set; }
    }
}