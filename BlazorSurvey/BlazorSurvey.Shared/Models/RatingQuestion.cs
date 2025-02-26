using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(RatingQuestion), typeDiscriminator: "ratingQuestion")]
public class RatingQuestion() : QuestionBase 
{
    public int ChoiceRange { get; set; } = 0;
}
