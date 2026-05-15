using System;

namespace social_V0._0._1.Utils
{
    public static class ImageHelper
    {
        public static string? ToBase64(byte[]? bytes) =>
            bytes != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}" : null;
    }
}
