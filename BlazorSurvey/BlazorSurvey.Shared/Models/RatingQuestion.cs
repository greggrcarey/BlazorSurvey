using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(RatingQuestion), typeDiscriminator: "ratingQuestion")]
public class RatingQuestion() : QuestionBase
{
    [Range(1, 15, ErrorMessage = "Choice Rnage must be between 0 and 15")]
    public int ChoiceRange { get; set; } = 1;
    
}
