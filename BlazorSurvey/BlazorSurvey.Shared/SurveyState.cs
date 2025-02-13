using BlazorSurvey.Shared.Models;

namespace BlazorSurvey.Shared;

/*
 * In-memory state container service
 * https://learn.microsoft.com/en-us/aspnet/core/blazor/state-management?view=aspnetcore-9.0&pivots=server#in-memory-state-container-service
 */
public class SurveyState
{
    public Survey CurrentSurvey { get; private set; } = new Survey();
    public SurveyResponseRollup? SurveyResponseRollup { get; private set; }
    public QuestionBase? Question { get; set; }

    public event Action? OnChange;

    public void InitializeSurvey()
    {
        CurrentSurvey.Title = "Test title from SurveyState";
        CurrentSurvey.Created = DateTimeOffset.Now;
        NotifyStateChanged();
    }

    public void InitializeSurvey(Survey surveyModel)
    {
        //Should I pass the survey here? 
        CurrentSurvey = surveyModel;
    }

    public void CreateQuestion(QuestionBase question)
    {
        question.SurveyId = CurrentSurvey.Id;
        Question = question;
    }

    public void AddQuestion()
    {
        if (Question is null) return;
        CurrentSurvey.Questions.Add(Question);
        Question = null;
        NotifyStateChanged();
    }

    public void UpdateQuestion(QuestionBase question)
    {
        var index = CurrentSurvey.Questions.FindIndex(q => q.Id == question.Id);
        if (index != -1)
        {
            CurrentSurvey.Questions[index] = question;
            NotifyStateChanged();
        }
    }
    public void InsertOrUpdateResponse(ResponseBase response)
    {
        var index = CurrentSurvey.Responses.FindIndex(r => r.Id == response.Id);
        if (index != -1)
        {
            CurrentSurvey.Responses[index] = response;
        }
        else
        {
            CurrentSurvey.Responses.Add(response);
        }
    }
    public void RemoveQuestion(QuestionBase question)
    {
        CurrentSurvey.Questions.Remove(question);
        NotifyStateChanged();
    }

    public void ResetSurvey()
    {
        CurrentSurvey = new Survey();
        Question = null;
        NotifyStateChanged();
    }

    public void SetSurveyResults(SurveyResponseRollup results)
    {
        if(results is not null)
        {
            SurveyResponseRollup = results;
            NotifyStateChanged();
        }
    }

    private void NotifyStateChanged() => OnChange?.Invoke();
}
