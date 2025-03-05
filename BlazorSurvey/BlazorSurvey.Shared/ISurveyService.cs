using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;
public interface ISurveyService
{
    IAsyncEnumerable<SurveyBase> GetSurveys();
    List<LivePoll> GetLivePolls();
    Task SaveSurvey(SurveyBase surveyModel);
    Task PostSurveyAsync(SurveyBase surveyModel);
    Task<SurveyBase?> GetSurveyBaseAsync(Guid surveyBaseId);
    Task PutSurveyAsync(SurveyBase surveyModel);
    Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId);
    Task DeleteSurvey(Guid SurveyId);
}
