using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using BlazorSurvey.Shared.Models;
using System.Diagnostics.CodeAnalysis;

namespace BlazorSurvey.Services;

public class CosmosDbService([FromServices] CosmosClient client, [FromServices] ILogger logger)
{
    //Need to add resiliency
    //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/conceptual-resilient-sdk-applications
    //https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli

    private readonly string surveyDbName = "SurveyDb";
    private readonly string surveyContainerName = "Survey";
    private readonly string storedProcedureId = "processResultsFromSingleSurvey";

    public async Task CreateSurveyBasetype(SurveyBase surveyBase)
    {
        Container container = GetSurveyContainer();
        try
        {
            ItemResponse<SurveyBase>? surveyBaseResponse = await container.CreateItemAsync(surveyBase, partitionKey: new PartitionKey(surveyBase.Id.ToString()));
            
        }
        catch (CosmosException cex)
        {
            logger.LogError(cex, cex.Message, []);
        }
    }

    public async IAsyncEnumerable<T> GetSurveyBaseIAsyncEnumerable<T>() where T : SurveyBase
    {
        Container surveyContainer = GetSurveyContainer();

        using FeedIterator<SurveyBase> iterator = surveyContainer.GetItemQueryIterator<SurveyBase>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<SurveyBase> response = await iterator.ReadNextAsync();

            foreach (var item in response)
            {
                if (item is T t)
                {
                    yield return (T)item;
                }

            }
        }
    }

    public async Task<SurveyBase> GetSurveyBaseAsync(Guid SurveyBaseId)
    {
        Container surveyContainer = GetSurveyContainer();
        return await surveyContainer.ReadItemAsync<SurveyBase>(id: SurveyBaseId.ToString(), new PartitionKey(SurveyBaseId.ToString()));
    }

    public async Task<T> GetSurveyBaseAsync<T>(Guid SurveyBaseId) where T : SurveyBase
    {
        //Breaks if the wrong type is passed in
        Container surveyContainer = GetSurveyContainer();

        ItemResponse<T>? response = await surveyContainer.ReadItemAsync<T>(id: SurveyBaseId.ToString(), new PartitionKey(SurveyBaseId.ToString()));
        if (response.Resource is not null)
        {
            return response.Resource;
        }
        else
        {
            throw new InvalidCastException();
        }

    }
    public async Task<SurveyBase> UpsertSurveyBaseAsync(SurveyBase surveyBase)
    {
        Container surveyContainer = GetSurveyContainer();
        ItemResponse<SurveyBase>? response = await surveyContainer.UpsertItemAsync(item: surveyBase, partitionKey: new PartitionKey(surveyBase.Id.ToString()));

        return response.Resource;
    }
    public async Task DeleteSurveyBaseAsync(Guid surveyId)
    {
        Container surveyContainer = GetSurveyContainer();
        try
        {
            ItemResponse<SurveyBase>? response = await surveyContainer.DeleteItemAsync<SurveyBase>(surveyId.ToString(), new PartitionKey(surveyId.ToString()));
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    public async Task<IEnumerable<SurveyResult>> GetResultsBySurveyBaseId(Guid surveyBaseId)
    {
        var result = await GetSurveyContainer().Scripts
            .ExecuteStoredProcedureAsync<IEnumerable<SurveyResult>>(storedProcedureId, new PartitionKey(surveyBaseId.ToString()), [surveyBaseId.ToString()]);

        return result.Resource;
    }

    private Container GetSurveyContainer()
    {
        Container? container = client.GetContainer(surveyDbName, surveyContainerName);

        if (container is null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        return container;
    }
}
