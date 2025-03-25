using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;
public interface ISurveyService
{
    IAsyncEnumerable<SurveyBase> GetSurveys();
    Task SaveSurvey(SurveyBase surveyModel);
    Task PostSurveyAsync(SurveyBase surveyModel);
    Task<SurveyBaseTakeSurveyDto?> GetSurveyBaseAsync(Guid surveyBaseId);
    Task PutSurveyAsync(SurveyBase surveyModel);
    Task PostSurveyResponses(SurveyBase surveyModel);
    Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId);
    Task DeleteSurvey(Guid SurveyId);
}
