using System;
using System.Text.Json;

namespace System.Text.Json
{

    public static class Utf8JsonReaderExtensions
    {
        public static bool NavigateToSection(this Utf8JsonReader reader, string sectionPath)
        {
            var sectionSegments = sectionPath?.Split(':', StringSplitOptions.RemoveEmptyEntries);
            int navigatedDepth = 0;

            foreach (var item in sectionSegments)
            {
                bool found = false;
                while (reader.Read() && reader.CurrentDepth >= navigatedDepth)
                {
                    if (reader.TokenType == JsonTokenType.PropertyName)
                    {
                        var propName = reader.GetString();
                        if (propName != null)
                        {
                            if (propName.Equals(item, StringComparison.CurrentCultureIgnoreCase))
                            {
                                found = true;
                                navigatedDepth = reader.CurrentDepth;
                                break;
                            }
                            else
                            {
                                reader.Skip();
                            }
                        }
                    }
                }

                if (!found)
                {
                    return false;
                }
            }

            return true;
            //JsonSerializer.Deserialize(ref reader, )
            //var obj = await JsonSerializer.DeserializeAsync(reader., someType);

        }

    }
}

