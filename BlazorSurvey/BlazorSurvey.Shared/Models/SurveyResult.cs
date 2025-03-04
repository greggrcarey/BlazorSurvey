namespace BlazorSurvey.Shared.Models
{
    public class SurveyResult
    {
        public required Guid ResponseId { get; set; }
        public required string Title { get; set; }
        public required string ResponseType { get; set; }
        public required Guid QuestionId { get; set; }
        public string[] TextResults { get; set; } = [];
        public string[] DateResults { get; set; } = [];
        public int[] RatingResults { get; set; } = [];
    }
}
