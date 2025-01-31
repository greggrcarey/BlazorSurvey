using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$responseType")]
[JsonDerivedType(typeof(DateResponse), typeDiscriminator: "dateResponse")]
public abstract class ResponseBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string QuestionTitle { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
}
