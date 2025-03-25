using BlazorSurvey.Services;
using BlazorSurvey.Shared.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace BlazorSurvey;

public class SurveyBaseModule
{
    private readonly CosmosDbService _cosmosDbService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SurveyBaseModule> _logger;

    public SurveyBaseModule(CosmosDbService cosmosDbService, ILogger<SurveyBaseModule> logger, IHttpContextAccessor httpContext)
    {
        _cosmosDbService = cosmosDbService;
        _logger = logger;
        _httpContextAccessor = httpContext;
    }

    public void MapSurveyBaseEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
    {
        var group = endpointRouteBuilder.MapGroup("/api/survey");

        group.MapGet("", GetSurveys)
            .WithName("GetSurveys")
            .RequireRateLimiting("fixed")
            .RequireAuthorization();

        group.MapGet("/{id}", GetSurveyById)
            .WithName("GetSurveyById")
            .RequireRateLimiting("fixed");//No Auth for anonymous endpoint

        group.MapPost("", PostSurvey)
            .WithName("PostSurvey")
            .RequireRateLimiting("fixed")
            .RequireAuthorization();

        group.MapPut("", PutSurvey)
            .WithName("PutSurvey")
            .RequireRateLimiting("fixed")
            .RequireAuthorization();

        group.MapDelete("/{id}", DeleteSurvey)
            .WithName("DeleteSurvey")
            .RequireRateLimiting("fixed")
            .RequireAuthorization();

        group.MapGet("/response/{id}", GetSurveyResponseById)
            .WithName("GetSurveyResponseById")
            .RequireRateLimiting("fixed")
            .RequireAuthorization();

        group.MapPost("/{id}/responses", PatchSurveyAtResponses)
            .WithName("PostSurveyAtResponses")
            .RequireRateLimiting("fixed");//No Auth for anonymous endpoint
    }

    public Results<Ok<IAsyncEnumerable<SurveyBase>>, NotFound, BadRequest> GetSurveys()
    {
        _logger.LogInformation("GetSurveys called");
        System.Security.Claims.ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;

        if (claimsPrincipal is null)
        {
            return TypedResults.BadRequest();
        }

        var results = _cosmosDbService.GetSurveyBaseIAsyncEnumerable<SurveyBase>(claimsPrincipal);
        return results switch
        {
            null => TypedResults.NotFound(),
            _ => TypedResults.Ok(results)
        };

    }

    public async Task<Results<Ok<SurveyBaseTakeSurveyDto>, NotFound>> GetSurveyById(
        Guid id)
    {
        /*GET: The HTTP GET method is used to read(or retrieve) a representation of a resource.
         * In the safe path, GET returns a representation in XML or JSON and an HTTP response code of 200(OK).
         * In an error case, it most often returns a 404(NOT FOUND) or 400(BAD REQUEST).*/
        _logger.LogInformation("GetSurveyById called: SurveyId: {id}", id.ToString());
        var result = await _cosmosDbService.GetSurveyBaseAsync(id);
        _logger.LogInformation("GetSurveyById result: {@result}", result);
        return result switch
        {
            null => TypedResults.NotFound(),
            _ => TypedResults.Ok(result)
        };

    }

    public async Task<Results<CreatedAtRoute<SurveyBase>, BadRequest>> PostSurvey(
        [FromBody] SurveyBase surveyBase)
    {
        //Post should create 
        /*
         POST: The POST verb is most often utilized to create new resources. 
        In particular, it’s used to create subordinate resources. 
        That is, subordinate to some other (e.g. parent) resource. 
        On successful creation, return HTTP status 201, 
        returning a Location header with a link to the newly-created resource with the 201 HTTP status.
         */
        _logger.LogInformation("PostSurvey called");
        System.Security.Claims.ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;

        if(claimsPrincipal is null)
        {
            return TypedResults.BadRequest();
        }

        var result = await _cosmosDbService.ReplaceSurveyBaseAsync(surveyBase, claimsPrincipal);
        _logger.LogInformation("PostSurvey: {@result}", result);
        return result switch
        {
            null => TypedResults.BadRequest(),
            _ => TypedResults.CreatedAtRoute(routeName: "GetSurveyById", routeValues: new { id = result.Id }, value: result)
        };
        //Need to prevent over posting
    }

    public async Task<Results<NoContent, BadRequest>> PutSurvey(
        [FromBody] SurveyBase surveyBase)
    {
        //PUT should repl
        /*
         PUT: It is used for updating the capabilities. 
        However, PUT can also be used to create a resource in the case where the resource ID is chosen by the client instead of by the server. 
        In other words, if the PUT is to a URI that contains the value of a non-existent resource ID. 
        On successful update, return 200 (or 204 if not returning any content in the body) from a PUT. 
        If using PUT for create, return HTTP status 201 on successful creation. PUT is not safe operation but it’s idempotent.
         */
        _logger.LogInformation("PutSurvey called");
        System.Security.Claims.ClaimsPrincipal? claimsPrincipal = _httpContextAccessor.HttpContext?.User;
        if (claimsPrincipal is null)
        {
            return TypedResults.BadRequest();
        }

        _ = await _cosmosDbService.ReplaceSurveyBaseAsync(surveyBase, claimsPrincipal);
        return TypedResults.NoContent();

        //Do I want to upsert here?
    }

    public async Task<NoContent> DeleteSurvey(Guid id)
    {
        /*
         * It is used to delete a resource identified by a URI. 
         * On successful deletion, return HTTP status 200 (OK) along with a response body.
         */
        _logger.LogInformation("DeleteSurvey called");
        await _cosmosDbService.DeleteSurveyBaseAsync(id);
        return TypedResults.NoContent();
    }

    public async Task<Results<Ok<SurveyResponseRollup>, NotFound>> GetSurveyResponseById(Guid id)
    {
        _logger.LogInformation("GetSurveyResponseById called");
        var result = await _cosmosDbService.GetResultsBySurveyBaseIdAsync(id);
        _logger.LogInformation("GetSurveyResponseById result: {@result}", result);

        return result switch
        {
            null => TypedResults.NotFound(),
            _ => TypedResults.Ok(result)
        };

    }

    public async Task<NoContent> PatchSurveyAtResponses([FromBody] SurveyBase surveyBase)
    {
        //TO DO: make this make sense
        await _cosmosDbService.PatchSurveyAtResponses(surveyBase);
        return TypedResults.NoContent();
    }


}