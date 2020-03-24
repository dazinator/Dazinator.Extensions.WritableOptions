using System.Text.Json;

namespace Dazinator.Extensions.Options
{
    public static class JsonSerializerOptionsExtensions
    {
        public static JsonWriterOptions ToJsonWriterOptions(this JsonSerializerOptions options)
        {
            return new JsonWriterOptions
            {
                Encoder = options.Encoder,
                Indented = options.WriteIndented
            };
        }
    }
}
