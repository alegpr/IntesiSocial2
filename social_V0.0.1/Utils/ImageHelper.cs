namespace social_V0._0._1.Utils
{
    //
    // Utility statica per la conversione di immagini binarie (byte array)
    // in formato Base64 URI, direttamente utilizzabile nell'attributo src
    // di un elemento &lt;img&gt; nel browser.
    //
    public static class ImageHelper
    {
        //
        // Converte un array di byte (es. da colonna varbinary SQL) in un
        // data URI Base64 con prefisso image/jpeg.
        //
        // Byte array dell'immagine, o null.
        //
        // Stringa data URI (es. "data:image/jpeg;base64,/9j...") se bytes
        // non è null; null altrimenti.
        //
        public static string? ToBase64(byte[]? bytes) =>
            bytes != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(bytes)}" : null;
    }
}