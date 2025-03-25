using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;
public interface ISurveyService
{
    IAsyncEnumerable<SurveyBase> GetSurveys();
    Task PostSurveyAsync(SurveyBase surveyModel);
    Task<SurveyBaseTakeSurveyDto?> GetSurveyBaseAsync(Guid surveyBaseId);
    Task PostSurveyResponses(SurveyBase surveyModel);
    Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId);
    Task DeleteSurvey(Guid SurveyId);
}
