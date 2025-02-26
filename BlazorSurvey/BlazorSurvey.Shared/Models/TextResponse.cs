using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(TextResponse), typeDiscriminator: "textResponse")]
public record TextResponse : ResponseBase
{
    public string TextQuestionResponse { get; set; } = string.Empty;
}
