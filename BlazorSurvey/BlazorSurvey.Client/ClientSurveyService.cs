using BlazorSurvey.Shared;
using BlazorSurvey.Shared.Models;
using System.Net.Http.Json;

internal class ClientSurveyService(HttpClient httpClient) : ISurveyService
{
    public List<LivePoll> GetLivePolls()
    {
        throw new NotImplementedException();
    }

    public async Task<Survey?> GetSurveyByIdAsync(Guid surveyId)
    {
        return await httpClient.GetFromJsonAsync<Survey>($"/api/survey/{surveyId}");
    }

    public async Task<IEnumerable<SurveyResult>> GetSurveyResultsAsync(Guid surveyId)
    {
        return await httpClient.GetFromJsonAsync<List<SurveyResult>>($"/api/survey/response/{surveyId}") ?? [];
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

    public async Task PostSurveyAsync(Survey surveyModel)
    {
        await httpClient.PostAsJsonAsync($"/api/survey", surveyModel);
    }

    public async Task PutSurveyAsync(Survey surveyModel)
    {
        _ = await httpClient.PutAsJsonAsync($"/api/survey", surveyModel);
    }

    public Task SaveSurvey(Survey surveyModel)
    {
        throw new NotImplementedException();
    }
}