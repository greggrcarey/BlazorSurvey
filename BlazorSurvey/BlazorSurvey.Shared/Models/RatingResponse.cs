using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(RatingResponse), typeDiscriminator: "ratingResponse")]
public record RatingResponse : ResponseBase 
{
    public int Choice { get; set; }
    public int ChoiceRange { get; set; }
} 