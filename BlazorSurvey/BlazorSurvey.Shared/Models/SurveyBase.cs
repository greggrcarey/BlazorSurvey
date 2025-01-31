using BlazorSurvey.Shared.Models;
using BlazorSurvey.Shared.Validation;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(LivePoll), typeDiscriminator: "livePoll")]
[JsonDerivedType(typeof(Survey), typeDiscriminator: "survey")]
public record SurveyBase
{
    [JsonPropertyName("id")]
    public Guid Id { get; set; } = Guid.NewGuid();
    public int Version { get; set; }
    [Required(AllowEmptyStrings = false, ErrorMessage = "A title is required for the survey")]
    public string Title { get; set; } = string.Empty;
    public int OrderBy { get; set; }
    public DateTimeOffset Created { get; set; }
    public DateTimeOffset Updated { get; set; }
    [FutureDate]
    public DateTimeOffset ExpiresAfter { get; set; }
    public bool IsActive => ExpiresAfter > DateTimeOffset.Now;
    public List<QuestionBase> Questions { get; set; } = [];
    public List<ResponseBase> Responses { get; set; } = [];
    public override string ToString()
    {
        base.ToString();

        return $"Id: {Id}, "
               + $"Version: {Version}, " 
               + $"Title: {Title}, "
               + $"OrderBy: {OrderBy}, "
               + $"Created: {Created}, "
               + $"Updated: {Updated}, "
               + $"ExpiresAfter: {ExpiresAfter}, "
               + $"IsActive: {IsActive}, "
               + $"Questions: {Questions.ToArray()}";
    }

}