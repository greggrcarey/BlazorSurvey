using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;

[JsonDerivedType(typeof(DateResponse), typeDiscriminator: "dateResponse")]
public class DateResponse : ResponseBase
{
    public DateOnly CalendarDateResponse { get; set; }
}
