using BlazorSurvey.Shared;
using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Services;

public class ServerSurveyService : ISurveyService
{
    private readonly CosmosDbService _cosmosDbService;
    public ServerSurveyService(CosmosDbService cosmosDbService) => _cosmosDbService = cosmosDbService;

    public async Task DeleteSurvey(Guid surveyId)
    {
        await _cosmosDbService.DeleteSurveyBaseAsync(surveyId);
    }

    public List<LivePoll> GetLivePolls()
    {
        throw new NotImplementedException();
    }

    public async Task<Survey?> GetSurveyByIdAsync(Guid surveyId)
    {
        return await _cosmosDbService.GetSurveyBaseAsync<Survey>(surveyId);
    }

    public async Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId)
    {
        return await _cosmosDbService.GetResultsBySurveyBaseIdAsync(surveyId);
    }

    public IAsyncEnumerable<SurveyBase> GetSurveys()
    {
        return _cosmosDbService.GetSurveyBaseIAsyncEnumerable<SurveyBase>();
    }

    public Task PostSurveyAsync(Survey surveyModel)
    {
        return _cosmosDbService.UpsertSurveyBaseAsync(surveyModel);
    }

    public Task PutSurveyAsync(Survey surveyModel)
    {
        return _cosmosDbService.UpsertSurveyBaseAsync(surveyModel);
    }

    public async Task SaveSurvey(Survey surveyModel)
    {
        await _cosmosDbService.CreateSurveyBasetypeAsync(surveyModel);
    }

}
