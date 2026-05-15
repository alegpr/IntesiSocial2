namespace social_V0._0._1.Utils
{
// Utility statica per la conversione di immagini binarie in formato Base64 visualizzabile nel browser.
    public static class ImageHelper
    {
// Converte un array di byte (es. da varbinary SQL) in un data URI Base64 per l'attributo src di &lt;img&gt;.

        public static string? ToBase64(byte[]? bytes) =>
            bytes != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}" : null;
    }
}
