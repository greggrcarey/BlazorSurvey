using BlazorSurvey.Shared;
using BlazorSurvey.Shared.Models;
using System.Net.Http.Json;

internal class ClientSurveyService(HttpClient httpClient) : ISurveyService
{
   
    public async Task<SurveyBaseTakeSurveyDto?> GetSurveyBaseAsync(Guid surveyId)
    {
        return await httpClient.GetFromJsonAsync<SurveyBaseTakeSurveyDto>($"/api/survey/{surveyId}");
    }

    public async Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId)
    {
        return await httpClient.GetFromJsonAsync<SurveyResponseRollup>($"/api/survey/response/{surveyId}");
    }

    public async IAsyncEnumerable<SurveyBase> GetSurveys()
    {
        var surveys = httpClient.GetFromJsonAsAsyncEnumerable<SurveyBase>("/api/survey");

        await foreach (var survey in surveys)
        {
            if (survey is not null)
            {
                yield return survey;
            }

        }
    }

    public async Task PostSurveyAsync(SurveyBase surveyModel)
    {
        await httpClient.PostAsJsonAsync($"/api/survey", surveyModel);
    }

    public async Task PutSurveyAsync(SurveyBase surveyModel)
    {
        _ = await httpClient.PutAsJsonAsync($"/api/survey", surveyModel);
    }

    public async Task DeleteSurvey(Guid surveyId)
    {
        _ = await httpClient.DeleteAsync($"/api/survey/{surveyId}");
    }

    public async Task SaveSurvey(SurveyBase surveyModel)
    {
        _ = await httpClient.PostAsJsonAsync($"/api/survey", surveyModel);
    }

    public async Task PostSurveyResponses(SurveyBase surveyModel)
    {
        _ = await httpClient.PostAsJsonAsync<SurveyBase>($"/api/survey/{surveyModel.Id}/responses", surveyModel);
    }
}