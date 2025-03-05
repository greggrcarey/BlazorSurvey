using BlazorSurvey.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlazorSurvey.Shared.Dtos;

public static class TakeSurveyMappingExtensions
{

    public static SurveyBaseTakeSurveyDto ToTakeSurveyBaseDto(this SurveyBase survey)
    {
        return survey switch
        {
            Survey s => s.ToTakeSurveyDto(),
            LivePoll lp => lp.ToLivePollTakeSurveyDto(),
            _ => throw new ArgumentException($"Unknown SurveyBase type: {nameof(survey)}")

        };
    }

    //Set TakeSurveyDtos to not leak responses from other surveys
    public static SurveyTakeSurveyDto ToTakeSurveyDto(this Survey survey) =>
        new SurveyTakeSurveyDto(survey.Id, survey.Title, survey.Questions);

    public static LivePollTakeSurveyDto ToLivePollTakeSurveyDto(this LivePoll livePoll) =>
        new LivePollTakeSurveyDto(livePoll.PollExpiresAt, livePoll.Id, livePoll.Title, livePoll.Questions);

}
