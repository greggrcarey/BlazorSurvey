using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(TextQuestion), typeDiscriminator: "textQuestion")]
[JsonDerivedType(typeof(DateQuestion), typeDiscriminator: "dateQuestion")]
public abstract record QuestionBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SurveyId { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "A title is required for the question")]
    public string Title { get; set; } = string.Empty;
    public DateTimeOffset Created { get; set; } = DateTimeOffset.Now;
    public DateTimeOffset Updated { get; set; }
    public int OrderBy { get; set; } = 0;
}
