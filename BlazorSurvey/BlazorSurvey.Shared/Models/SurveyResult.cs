using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSurvey.Shared.Models
{
    public class SurveyResult
    {
        public Guid ResponseId {  get; set; } = Guid.Empty;
        public string Title { get; set; } = string.Empty;
        public string ResponseType { get; set; } = string.Empty;
        public Guid QuestionId {  get; set; } = Guid.Empty;
        public string[] Results { get; set; } = [];
    }
}
