using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(Survey), typeDiscriminator: "survey")]
public record Survey : SurveyBase
{
}

public record SurveyTakeSurveyDto(Guid Id,
                                  string Title,
                                  List<QuestionBase> Questions)
    : SurveyBaseTakeSurveyDto(Id, Title, Questions);
