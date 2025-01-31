using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;

[JsonDerivedType(typeof(DateQuestion), typeDiscriminator: "dateQuestion")]
public record DateQuestion : QuestionBase
{

}
