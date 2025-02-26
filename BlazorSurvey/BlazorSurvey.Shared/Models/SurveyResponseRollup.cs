using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSurvey.Shared.Models;

public record SurveyResponseRollup(string SurveyTitle, List<SurveyResult> Results);