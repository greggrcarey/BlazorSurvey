using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;
public interface ISurveyService
{
    List<Survey> GetSurveys();
    IAsyncEnumerable<SurveyBase> GetSurveys2();
    List<LivePoll> GetLivePolls();
    void SaveSurvey(Survey surveyModel);
    Task PostSurveyAsync(Survey surveyModel);
    Task<Survey?> GetSurveyByIdAsync(Guid surveyId);
    Task PutSurveyAsync(Survey surveyModel);
    Task<List<SurveyResult>> GetSurveyResultsAsync(Guid surveyId);
}
