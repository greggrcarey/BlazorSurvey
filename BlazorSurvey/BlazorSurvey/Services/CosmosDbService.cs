using BlazorSurvey.Shared.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;

namespace BlazorSurvey.Services;

public class CosmosDbService
{
    private readonly CosmosClient _client;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions
    {
        Size = 1024 * 2,
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
    };

    public CosmosDbService(CosmosClient client, ILogger<CosmosDbService> logger, IMemoryCache memoryCache)
    {
        _client = client;
        _logger = logger;
        _memoryCache = memoryCache;
    }


    //Need to add resiliency
    //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/conceptual-resilient-sdk-applications
    //https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli

    private readonly string surveyDbName = "SurveyDb";
    private readonly string surveyContainerName = "Survey";
    private readonly string storedProcedureId = "processResultsFromSingleSurvey";

    public async Task CreateSurveyBasetypeAsync(SurveyBase surveyBase)
    {
        Container container = GetSurveyContainer();
        try
        {
            ItemResponse<SurveyBase>? surveyBaseResponse = await container.CreateItemAsync(surveyBase, partitionKey: new PartitionKey(surveyBase.Id.ToString()));
            if (surveyBaseResponse.StatusCode == System.Net.HttpStatusCode.Created || 
                surveyBaseResponse.StatusCode == System.Net.HttpStatusCode.Accepted)
            {
                _memoryCache.Set<SurveyBase>(surveyBase.Id, surveyBaseResponse.Resource, CacheEntryOptions);
            }

        }
        catch (CosmosException cex)
        {
            _logger.LogError(cex, cex.Message, []);
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

    public async Task<SurveyBase> GetSurveyBaseAsync(Guid surveyBaseId)
    {
        SurveyBase output = new();

        if (!_memoryCache.TryGetValue<SurveyBase>(surveyBaseId, out SurveyBase? cachedValue))
        {
            Container surveyContainer = GetSurveyContainer();
            cachedValue = await surveyContainer.ReadItemAsync<SurveyBase>(id: surveyBaseId.ToString(), new PartitionKey(surveyBaseId.ToString()));

            _memoryCache.Set<SurveyBase>(surveyBaseId, cachedValue, CacheEntryOptions);

        }

        if (cachedValue is not null)
        {
            output = cachedValue;

        }

        return output;

    }

    public async Task<T> GetSurveyBaseAsync<T>(Guid surveyBaseId) where T : SurveyBase
    {
        SurveyBase output = new();

        if (!_memoryCache.TryGetValue<T>(surveyBaseId, out T? cachedValue))
        {
            //Breaks if the wrong type is passed in
            Container surveyContainer = GetSurveyContainer();

            ItemResponse<T>? response = await surveyContainer.ReadItemAsync<T>(id: surveyBaseId.ToString(), new PartitionKey(surveyBaseId.ToString()));
            if (response.Resource is not null)
            {
                cachedValue = response.Resource;
                _memoryCache.Set<T>(surveyBaseId, cachedValue, CacheEntryOptions);

            }
            else
            {
                throw new InvalidCastException();
            }

        }

        if (cachedValue is not null)
        {
            output = cachedValue;
        }

        return (T)output;


    }
    public async Task<SurveyBase> UpsertSurveyBaseAsync(SurveyBase surveyBase)
    {
        Container surveyContainer = GetSurveyContainer();
        ItemResponse<SurveyBase>? response = await surveyContainer.UpsertItemAsync(item: surveyBase, partitionKey: new PartitionKey(surveyBase.Id.ToString()));

        _memoryCache.Remove(surveyBase.Id);
        _memoryCache.Set<SurveyBase>(surveyBase.Id, response.Resource, CacheEntryOptions);

        return response.Resource;
    }
    public async Task DeleteSurveyBaseAsync(Guid surveyId)
    {
        Container surveyContainer = GetSurveyContainer();
        try
        {
            ItemResponse<SurveyBase>? response = await surveyContainer.DeleteItemAsync<SurveyBase>(surveyId.ToString(), new PartitionKey(surveyId.ToString()));

            if(response.StatusCode is System.Net.HttpStatusCode.OK)
            {
                _memoryCache.Remove(surveyId);
            }

        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }

    }

    public async Task<SurveyResponseRollup> GetResultsBySurveyBaseIdAsync(Guid surveyBaseId)
    {
        var result = await GetSurveyContainer().Scripts
            .ExecuteStoredProcedureAsync<SurveyResponseRollup>(storedProcedureId, new PartitionKey(surveyBaseId.ToString()), [surveyBaseId.ToString()]);

        return result.Resource;
    }

    private Container GetSurveyContainer()
    {
        Container? container = _client.GetContainer(surveyDbName, surveyContainerName);

        if (container is null)
        {
            throw new ArgumentNullException(nameof(container));
        }

        return container;
    }
}
