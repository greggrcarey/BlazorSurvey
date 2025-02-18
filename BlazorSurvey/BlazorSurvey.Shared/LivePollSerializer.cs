using BlazorSurvey.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSurvey.Shared;

[JsonSourceGenerationOptions(
       WriteIndented = true,
       PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
       GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(LivePoll))]
[JsonSerializable(typeof(LivePoll[]))]
public partial class LivePollSerializer : JsonSerializerContext
{
}
