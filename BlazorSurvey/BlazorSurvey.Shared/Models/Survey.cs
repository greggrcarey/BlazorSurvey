using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(Survey), typeDiscriminator: "survey")]
public record Survey : SurveyBase
{
}
