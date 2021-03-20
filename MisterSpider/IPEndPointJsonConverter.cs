using System;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MisterSpider
{
    public class IPEndPointJsonConverter : JsonConverter<IPEndPoint>
    {
        public override IPEndPoint Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // convert an ipaddress represented as a string into an IPAddress object and return it to the caller
            if (typeToConvert == typeof(IPEndPoint))
            {
                return IPEndPoint.Parse(reader.GetString());
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, IPEndPoint value, JsonSerializerOptions options)
        {
            // convert an IPAddress object to a string representation of itself and Write it to the serialiser
            if (value.GetType() == typeof(IPEndPoint))
            {
                writer.WriteStringValue(value.ToString());
            }
        }
    }
}
