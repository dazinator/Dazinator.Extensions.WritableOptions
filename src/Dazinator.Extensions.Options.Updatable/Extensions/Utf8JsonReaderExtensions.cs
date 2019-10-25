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

        public static void WriteValue(this Utf8JsonReader reader, Utf8JsonWriter writer)
        {

            // if im not on the path, write stuff.
            switch (reader.TokenType)
            {

                case JsonTokenType.Comment:
                    writer.WriteCommentValue(reader.ValueSpan);
                    break;
                case JsonTokenType.EndArray:
                    writer.WriteEndArray();
                    break;
                case JsonTokenType.EndObject:
                    writer.WriteEndObject();
                    break;
                case JsonTokenType.False:
                    writer.WriteBooleanValue(false);
                    break;
                case JsonTokenType.None:
                    break;
                case JsonTokenType.Null:
                    writer.WriteNullValue();
                    break;
                case JsonTokenType.Number:
                    // Not really sure about this..
                    if (reader.TryGetInt32(out Int32 int1))
                    {
                        writer.WriteNumberValue(int1);
                    }
                    else if (reader.TryGetInt64(out Int64 int2))
                    {
                        writer.WriteNumberValue(int2);
                    }
                    else if (reader.TryGetDecimal(out decimal someDecimal))
                    {
                        writer.WriteNumberValue(someDecimal);
                    }
                    break;
                case JsonTokenType.PropertyName:
                    writer.WritePropertyName(reader.ValueSpan);
                    break;
                case JsonTokenType.StartArray:
                    writer.WriteStartArray();
                    break;
                case JsonTokenType.StartObject:
                    writer.WriteStartObject();
                    break;
                case JsonTokenType.String:
                    writer.WriteStringValue(reader.ValueSpan);
                    break;
                case JsonTokenType.True:
                    writer.WriteBooleanValue(true);
                    break;

            }
        }


    }
}

