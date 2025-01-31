using BlazorSurvey.Shared.Models;
using BlazorSurvey.Shared.Validation;
using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(LivePoll), typeDiscriminator: "livePoll")]
public record LivePoll : SurveyBase
{
    [FutureDate]
    public DateTimeOffset PollExpiresAt { get; set; }

    public override string ToString() => $"PollExpiresAt: {PollExpiresAt}, " + base.ToString();

}
