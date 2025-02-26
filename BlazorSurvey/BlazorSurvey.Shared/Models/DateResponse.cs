using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;

[JsonDerivedType(typeof(DateResponse), typeDiscriminator: "dateResponse")]
public record DateResponse : ResponseBase
{
    public DateOnly CalendarDateResponse { get; set; }
}
