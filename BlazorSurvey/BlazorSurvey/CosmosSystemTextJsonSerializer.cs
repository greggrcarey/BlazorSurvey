using Azure.Core.Serialization;
using Microsoft.Azure.Cosmos;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;


namespace BlazorSurvey;

/// <summary>
/// Uses <see cref="JsonObjectSerializer"/> which leverages System.Text.Json providing a simple API to interact with on the Azure SDKs.
/// </summary>
/// <remarks>
/// For item CRUD operations and non-LINQ queries, implementing CosmosSerializer is sufficient. To support LINQ query translations as well, CosmosLinqSerializer must be implemented.
/// Taken from GitHub at: Microsoft.Azure.Cosmos.Samples/Usage/SystemTextJson/CosmosSystemTextJsonSerializer.cs
/// </remarks>
// <SystemTextJsonSerializer>
public class CosmosSystemTextJsonSerializer : CosmosLinqSerializer
{
    private readonly JsonObjectSerializer systemTextJsonSerializer;
    private readonly JsonSerializerOptions jsonSerializerOptions;

    public CosmosSystemTextJsonSerializer(JsonSerializerOptions jsonSerializerOptions)
    {
        systemTextJsonSerializer = new JsonObjectSerializer(jsonSerializerOptions);
        this.jsonSerializerOptions = jsonSerializerOptions;
    }

    public override T FromStream<T>(Stream stream)
    {

        using (stream)
        {
            if (stream.CanSeek
                   && stream.Length == 0)
            {
#pragma warning disable CS8603 // Possible null reference return.
                return default;
#pragma warning restore CS8603 // Possible null reference return.
            }

            if (typeof(Stream).IsAssignableFrom(typeof(T)))
            {
                return (T)(object)stream;
            }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8603 // Possible null reference return.
            return (T)systemTextJsonSerializer.Deserialize(stream, typeof(T), default);
#pragma warning restore CS8603 // Possible null reference return.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        }
    }

    public override Stream ToStream<T>(T input)
    {
        MemoryStream streamPayload = new MemoryStream();
        systemTextJsonSerializer.Serialize(streamPayload, input, input.GetType(), default);
        streamPayload.Position = 0;
        return streamPayload;
    }

    public override string SerializeMemberName(MemberInfo memberInfo)
    {
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        JsonExtensionDataAttribute jsonExtensionDataAttribute = memberInfo.GetCustomAttribute<JsonExtensionDataAttribute>(true);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        if (jsonExtensionDataAttribute != null)
        {
#pragma warning disable CS8603 // Possible null reference return.
            return null;
#pragma warning restore CS8603 // Possible null reference return.
        }

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
        JsonPropertyNameAttribute jsonPropertyNameAttribute = memberInfo.GetCustomAttribute<JsonPropertyNameAttribute>(true);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
        if (!string.IsNullOrEmpty(jsonPropertyNameAttribute?.Name))
        {
            return jsonPropertyNameAttribute.Name;
        }

        if (jsonSerializerOptions.PropertyNamingPolicy != null)
        {
            return jsonSerializerOptions.PropertyNamingPolicy.ConvertName(memberInfo.Name);
        }

        // Do any additional handling of JsonSerializerOptions here.

        return memberInfo.Name;
    }
}
// </SystemTextJsonSerializer>
