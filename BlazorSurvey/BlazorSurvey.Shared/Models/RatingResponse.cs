using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(RatingResponse), typeDiscriminator: "ratingResponse")]
public record RatingResponse(int Choice) : ResponseBase { }