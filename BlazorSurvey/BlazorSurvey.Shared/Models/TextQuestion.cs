﻿using System.Text.Json.Serialization;

namespace BlazorSurvey.Shared.Models;
[JsonDerivedType(typeof(TextQuestion), typeDiscriminator: "textQuestion")]
public class TextQuestion : QuestionBase
{
}
