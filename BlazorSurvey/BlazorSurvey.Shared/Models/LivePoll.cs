using BlazorSurvey.Shared.Validation;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(LivePoll), typeDiscriminator: "livePoll")]
public record LivePoll : SurveyBase
{
    [FutureDate]
    public DateTimeOffset PollExpiresAt { get; set; }

    public override string ToString() => $"PollExpiresAt: {PollExpiresAt}, " + base.ToString();

}

public record LivePollTakeSurveyDto(DateTimeOffset PollExpiresAt,
                                    Guid Id,
                                    string Title,
                                    List<QuestionBase> Questions) : SurveyBaseTakeSurveyDto(Id, Title,  Questions);