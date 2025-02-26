using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;

[JsonPolymorphic(TypeDiscriminatorPropertyName = "$responseType")]
[JsonDerivedType(typeof(DateResponse), typeDiscriminator: "dateResponse")]
[JsonDerivedType(typeof(TextResponse), typeDiscriminator: "textResponse")]
public abstract record ResponseBase
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string QuestionTitle { get; set; } = string.Empty;
    public Guid QuestionId { get; set; }
}
