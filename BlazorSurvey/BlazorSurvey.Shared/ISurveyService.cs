using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;
public interface ISurveyService
{
    IAsyncEnumerable<SurveyBase> GetSurveys();
    List<LivePoll> GetLivePolls();
    Task SaveSurvey(Survey surveyModel);
    Task PostSurveyAsync(Survey surveyModel);
    Task<Survey?> GetSurveyByIdAsync(Guid surveyId);
    Task PutSurveyAsync(Survey surveyModel);
    Task<IEnumerable<SurveyResult>> GetSurveyResultsAsync(Guid surveyId);
}
