using BlazorSurvey.Data;
using BlazorSurvey.Shared.Dtos;
using BlazorSurvey.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.OpenApi.Writers;

namespace BlazorSurvey.Services;

public class CosmosDbService
{
    private readonly CosmosClient _client;
    private readonly ILogger _logger;
    private readonly IMemoryCache _memoryCache;
    private readonly string surveyDbName = "SurveyDb";
    private readonly string surveyContainerName = "Survey";
    private readonly string storedProcedureId = "processResultsFromSingleSurvey";
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IServiceProvider _serviceProvider;
    private readonly MemoryCacheEntryOptions CacheEntryOptions = new MemoryCacheEntryOptions
    {
        Size = 1024 * 2,
        AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
    };

    public CosmosDbService(CosmosClient client,
                           ILogger<CosmosDbService> logger,
                           IMemoryCache memoryCache,
                           IHttpContextAccessor httpContextAccessor,
                           IServiceProvider serviceProvider)
    {
        _client = client;
        _logger = logger;
        _memoryCache = memoryCache;
        _httpContextAccessor = httpContextAccessor;
        //TODO: https://learn.microsoft.com/en-us/aspnet/core/blazor/security/interactive-server-side-rendering?view=aspnetcore-9.0&preserve-view=true
        _serviceProvider = serviceProvider;
    }

    //Need to add resiliency
    //https://learn.microsoft.com/en-us/azure/cosmos-db/nosql/conceptual-resilient-sdk-applications
    //https://learn.microsoft.com/en-us/dotnet/core/resilience/?tabs=dotnet-cli

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
        var user = await GetCurrentUser();

        if (user is null)
        {
            yield break;
        }

        using FeedIterator<SurveyBase> iterator = surveyContainer.GetItemQueryIterator<SurveyBase>();

        while (iterator.HasMoreResults)
        {
            FeedResponse<SurveyBase> response = await iterator.ReadNextAsync();

            foreach (var item in response)
            {
                if (item is T t && user.Id == item.UserId)
                {
                    yield return (T)item;
                }

            }
        }
    }

    public async Task<SurveyBaseTakeSurveyDto> GetSurveyBaseAsync(Guid surveyBaseId)
    {

        if (!_memoryCache.TryGetValue<SurveyBase>(surveyBaseId, out SurveyBase? cachedValue))
        {
            Container surveyContainer = GetSurveyContainer();
            cachedValue = await surveyContainer.ReadItemAsync<SurveyBase>(id: surveyBaseId.ToString(), new PartitionKey(surveyBaseId.ToString()));

            _memoryCache.Set<SurveyBase>(surveyBaseId, cachedValue, CacheEntryOptions);

        }

        if (cachedValue is null)
        {
            throw new ArgumentNullException(nameof(cachedValue));
        }

        return cachedValue.ToTakeSurveyBaseDto();

    }
    public async Task<SurveyBase> UpsertSurveyBaseAsync(SurveyBase surveyBase)
    {

        if (surveyBase.Questions.Count > 10)
        {
            // TODO: fix validation
            throw new ArgumentOutOfRangeException(nameof(surveyBase.Questions), "SurveyBase types are only allowed 10 questions");
        }

        ApplicationUser? user = await GetCurrentUser();

        if (user is not null)
        {
            surveyBase.UserId = user.Id;
        }

        //TODO: Chase this down and see if I can remove it
        Container surveyContainer = GetSurveyContainer();
        ItemResponse<SurveyBase>? response = await surveyContainer.UpsertItemAsync(item: surveyBase, partitionKey: new PartitionKey(surveyBase.Id.ToString()));

        _memoryCache.Remove(response.Resource.Id);
        _memoryCache.Set<SurveyBase>(surveyBase.Id, response.Resource, CacheEntryOptions);

        return response.Resource;
    }

    private async Task<ApplicationUser?> GetCurrentUser()
    {
        try
        {
            System.Security.Claims.ClaimsPrincipal? principal = _httpContextAccessor.HttpContext?.User;

            if (principal is null) { throw new ArgumentNullException(nameof(principal)); }

            using IServiceScope? serviceProviderScope = _serviceProvider.CreateScope();

            if(serviceProviderScope is null) return null;

            var userManager = serviceProviderScope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            ApplicationUser? user = await userManager.GetUserAsync(principal);

            if (user is null) { throw new ArgumentNullException(nameof(user)); }

            return user;
        }
        catch (ArgumentNullException ane)
        {
            _logger.LogError(ane, "Error in {nameof(GetCurrentUser)}", nameof(GetCurrentUser));
            return null;
        }

    }

    public async Task PatchSurveyAtResponses(SurveyBase surveyBase)
    {
        try
        {
            var patchOperations = new List<PatchOperation>();

            foreach (var response in surveyBase.Responses)
            {
                patchOperations.Add(PatchOperation.Add("/responses/-", response));
            }

            Container surveyContainer = GetSurveyContainer();
            ItemResponse<SurveyBase> item = await surveyContainer.PatchItemAsync<SurveyBase>(
                id: surveyBase.Id.ToString(),
                partitionKey: new PartitionKey(surveyBase.Id.ToString()),
                patchOperations);

            _memoryCache.Remove(surveyBase.Id);
            _memoryCache.Set<SurveyBase>(surveyBase.Id, item.Resource, CacheEntryOptions);
        }
        catch (CosmosException cex)
        {
            _logger.LogError($"Error in PatchSurveyAtResponses: {cex.ToString()}");
        }

    }

    public async Task DeleteSurveyBaseAsync(Guid surveyId)
    {
        Container surveyContainer = GetSurveyContainer();
        try
        {
            ItemResponse<SurveyBase>? response = await surveyContainer.DeleteItemAsync<SurveyBase>(surveyId.ToString(), new PartitionKey(surveyId.ToString()));

            if (response.StatusCode is System.Net.HttpStatusCode.OK)
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
