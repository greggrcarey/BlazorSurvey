using BlazorSurvey.Shared;
using BlazorSurvey.Shared.Models;
using System.Security.Claims;

namespace BlazorSurvey.Services;

internal class ServerSurveyService : ISurveyService
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly UserService _userService;

    public ServerSurveyService(CosmosDbService cosmosDbService, UserService userService)
    {
        _cosmosDbService = cosmosDbService;
        _userService = userService;
    }

    public async Task DeleteSurvey(Guid surveyId)
    {
        await _cosmosDbService.DeleteSurveyBaseAsync(surveyId);
    }
   
    public async Task<SurveyBaseTakeSurveyDto?> GetSurveyBaseAsync(Guid surveyId)
    {
        return await _cosmosDbService.GetSurveyBaseAsync(surveyId);
    }

    public async Task<SurveyResponseRollup?> GetSurveyResultsAsync(Guid surveyId)
    {
        return await _cosmosDbService.GetResultsBySurveyBaseIdAsync(surveyId);
    }

    public IAsyncEnumerable<SurveyBase> GetSurveys()
    {
        ClaimsPrincipal? claimsPrincipal = _userService.GetUser();
        if (claimsPrincipal == null)
        {
            throw new InvalidOperationException("ClaimsPrincipal cannot be null for GetSurveys");
        }
        return _cosmosDbService.GetSurveyBaseIAsyncEnumerable<SurveyBase>(claimsPrincipal);
    }

    public Task PostSurveyAsync(SurveyBase surveyModel)
    {
        ClaimsPrincipal? claimsPrincipal = _userService.GetUser();
        if (claimsPrincipal == null)
        {
            throw new InvalidOperationException("ClaimsPrincipal cannot be null for PostSurveyAsync");
        }

        return _cosmosDbService.CreateSurveyBasetypeAsync(surveyModel, claimsPrincipal);
    }

    public Task PostSurveyResponses(SurveyBase surveyModel)
    {
        return _cosmosDbService.PatchSurveyAtResponses(surveyModel);
    }

}
