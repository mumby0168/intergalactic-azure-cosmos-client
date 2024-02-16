using System.Text.Json;
using Azure.Core.Serialization;

namespace Intergalactic.Azure.Cosmos.Internals.Serialisation;

internal class CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions) : CosmosSerializer
{
    private readonly JsonObjectSerializer _systemTextJsonSerializer = new(jsonSerializerOptions);

    public override T FromStream<T>(Stream stream)
    {
        using (stream)
        {
            if (stream is {CanSeek: true, Length: 0})
            {
                return default!;
            }

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

            return (T)_systemTextJsonSerializer.Deserialize(stream, typeof(T), default)!;
        }
    }

    public override Stream ToStream<T>(T input)
    {
        var streamPayload = new MemoryStream();
        _systemTextJsonSerializer.Serialize(streamPayload, input, input.GetType(), default);
        streamPayload.Position = 0;
        return streamPayload;
    }
}