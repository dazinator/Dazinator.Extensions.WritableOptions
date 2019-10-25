using System.Collections.Generic;

namespace System.Text.Json
{
    public static class Utf8JsonWriterExtensions
    {
        public static void WriteJsonWithModifiedSection<TObject>(this Utf8JsonWriter writer, Utf8JsonReader reader, string sectionPath, TObject newValue, JsonSerializerOptions options = default)
        {

            var sectionSegments = sectionPath?.Split(':', StringSplitOptions.RemoveEmptyEntries);

            // Special case - when writing to the root we don't need to preserve anything from the reader.
            if (sectionSegments.Length == 0)
            {
                JsonSerializer.Serialize<TObject>(writer, newValue, options);
                try
                {
                    // if the reader does contain a start object token we'll skip over it so the reader is posioned at end.
                    reader.Read(); // start object token if present.
                    reader.Skip();
                    reader.Read(); // end object
                }
                catch (JsonException ex)
                {
                    // If the reader didn't have any data / any json tokens - don't worry about it.
                    if (ex.Path == null)
                    {

                    }
                    else
                    {
                        throw ex;
                    }
                }
                return;
            }

            // Write the serialised json object at the designated section path, preserving all other information from the reader.
            int currentSectionDepth = 0;
            var sectionQueue = new Queue<string>(sectionSegments);
            bool readerNoData = false;

            while (sectionQueue.Count > 0)
            {
                var sectionPropertyName = sectionQueue.Dequeue();
                currentSectionDepth = currentSectionDepth + 1;

                try
                {
                    while (reader.Read())
                    {
                        // If we reach the end of the object without finding the current section name (as a property name)
                        // then we need to add this property (and all nested properties in the remaining section name segments) and then add our json value
                        // at the newly added destination, before we close the object.
                        if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentSectionDepth - 1)
                        {
                            WriteObjectAtPath(writer, ref currentSectionDepth, sectionQueue, ref sectionPropertyName, () => JsonSerializer.Serialize<TObject>(writer, newValue, options));
                            writer.WriteEndObject();
                        }
                        else if (reader.CurrentDepth != currentSectionDepth)
                        {
                            reader.WriteValue(writer);
                            continue;
                        }
                        else if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            var propName = reader.GetString();
                            if (propName.Equals(sectionPropertyName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                reader.WriteValue(writer);
                                if (sectionQueue.Count == 0)
                                {
                                    // This is the destination section path where we need to append our updated json
                                    reader.Skip();
                                    JsonSerializer.Serialize<TObject>(writer, newValue, options);
                                }
                                break;

                            }
                            else
                            {
                                // some other property to preserve.
                                reader.WriteValue(writer);
                            }
                        }
                        else
                        {
                            // preserve whatever we found
                            reader.WriteValue(writer);
                        }

                    }

                }
                catch (JsonException ex)
                {
                    // If the reader didn't have any data / any json tokens - don't worry about it.
                    if (ex.Path == null)
                    {
                        readerNoData = true;
                        writer.WriteStartObject();
                        WriteObjectAtPath(writer, ref currentSectionDepth, sectionQueue, ref sectionPropertyName, () => JsonSerializer.Serialize<TObject>(writer, newValue, options));
                        writer.WriteEndObject();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            // We have updated our section of json, but there may be more json to process from the reader,
            // so make sure we process that before we finish.  
            if(!readerNoData)
            {
                while (reader.Read())
                {
                    reader.WriteValue(writer);
                }
            }         

            //JsonSerializer.Deserialize(ref reader, )
            //var obj = await JsonSerializer.DeserializeAsync(reader., someType);

        }

        public static void WriteJsonWithModifiedSection<TObject>(this Utf8JsonWriter writer, Utf8JsonStreamReader reader, string sectionPath, TObject newValue, JsonSerializerOptions options = default)
        {

            var sectionSegments = sectionPath?.Split(':', StringSplitOptions.RemoveEmptyEntries);

            // Special case - when writing to the root we don't need to preserve anything from the reader.
            if (sectionSegments.Length == 0)
            {
                JsonSerializer.Serialize<TObject>(writer, newValue, options);
                try
                {
                    // if the reader does contain a start object token we'll skip over it so the reader is posioned at end.
                    reader.Read(); // start object token if present.
                    reader.Skip();
                    reader.Read(); // end object
                }
                catch (JsonException ex)
                {
                    // If the reader didn't have any data / any json tokens - don't worry about it.
                    if (ex.Path == null)
                    {

                    }
                    else
                    {
                        throw ex;
                    }
                }
                return;
            }

            // Write the serialised json object at the designated section path, preserving all other information from the reader.
            int currentSectionDepth = 0;
            var sectionQueue = new Queue<string>(sectionSegments);
            bool readerNoData = false;

            while (sectionQueue.Count > 0)
            {
                var sectionPropertyName = sectionQueue.Dequeue();
                currentSectionDepth = currentSectionDepth + 1;

                try
                {
                    while (reader.Read())
                    {
                        // If we reach the end of the object without finding the current section name (as a property name)
                        // then we need to add this property (and all nested properties in the remaining section name segments) and then add our json value
                        // at the newly added destination, before we close the object.
                        if (reader.TokenType == JsonTokenType.EndObject && reader.CurrentDepth == currentSectionDepth - 1)
                        {
                            WriteObjectAtPath(writer, ref currentSectionDepth, sectionQueue, ref sectionPropertyName, () => JsonSerializer.Serialize<TObject>(writer, newValue, options));
                            writer.WriteEndObject();
                        }
                        else if (reader.CurrentDepth != currentSectionDepth)
                        {
                            reader.WriteValue(writer);
                            continue;
                        }
                        else if (reader.TokenType == JsonTokenType.PropertyName)
                        {
                            var propName = reader.GetString();
                            if (propName.Equals(sectionPropertyName, StringComparison.CurrentCultureIgnoreCase))
                            {
                                reader.WriteValue(writer);
                                if (sectionQueue.Count == 0)
                                {
                                    // This is the destination section path where we need to append our updated json
                                    reader.Skip();
                                    JsonSerializer.Serialize<TObject>(writer, newValue, options);
                                }
                                break;

                            }
                            else
                            {
                                // some other property to preserve.
                                reader.WriteValue(writer);
                            }
                        }
                        else
                        {
                            // preserve whatever we found
                            reader.WriteValue(writer);
                        }

                    }

                }
                catch (JsonException ex)
                {
                    // If the reader didn't have any data / any json tokens - don't worry about it.
                    if (ex.Path == null)
                    {
                        readerNoData = true;
                        writer.WriteStartObject();
                        WriteObjectAtPath(writer, ref currentSectionDepth, sectionQueue, ref sectionPropertyName, () => JsonSerializer.Serialize<TObject>(writer, newValue, options));
                        writer.WriteEndObject();
                    }
                    else
                    {
                        throw ex;
                    }
                }
            }

            // We have updated our section of json, but there may be more json to process from the reader,
            // so make sure we process that before we finish.  
            if (!readerNoData)
            {
                while (reader.Read())
                {
                    reader.WriteValue(writer);
                }
            }

            //JsonSerializer.Deserialize(ref reader, )
            //var obj = await JsonSerializer.DeserializeAsync(reader., someType);

        }


        private static void WriteObjectAtPath(Utf8JsonWriter writer, ref int currentSectionDepth, Queue<string> sectionQueue, ref string sectionPropertyName, Action writeObject)
        {
            // rest of path segements will also be missing so create them with empty objects until we reach last one.
            int emptyObjectsToClose = 0;
            writer.WritePropertyName(sectionPropertyName);
            while (sectionQueue.Count >= 1)
            {
                writer.WriteStartObject();
                emptyObjectsToClose = emptyObjectsToClose + 1;
                sectionPropertyName = sectionQueue.Dequeue();
                currentSectionDepth = currentSectionDepth + 1;
                writer.WritePropertyName(sectionPropertyName);
            }

            // Now write the actual updated json object
            // writer.WritePropertyName(sectionPropertyName);
            writeObject();
            //// This is where we need to append our updated json
            //reader.Skip();


            for (int i = 0; i < emptyObjectsToClose; i++)
            {
                writer.WriteEndObject();
            }
        }


    }
}

