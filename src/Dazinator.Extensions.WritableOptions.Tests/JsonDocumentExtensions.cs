namespace System.Text.Json
{
    public static class JsonDocumentExtensions
    {
        public static bool TryGetPropertyAtPath(this JsonDocument document, string sectionPath, out JsonElement element)
        {
            var sectionSegments = sectionPath?.Split(':', StringSplitOptions.RemoveEmptyEntries);

            JsonElement currentElement = document.RootElement;
            foreach (var item in sectionSegments)
            {
                if (!currentElement.TryGetProperty(item, out currentElement))
                {
                    element = currentElement;
                    return false;
                }
            }
            element = currentElement;
            return true;

            //JsonSerializer.Deserialize(ref reader, )
            //var obj = await JsonSerializer.DeserializeAsync(reader., someType);

        }

    }
}

