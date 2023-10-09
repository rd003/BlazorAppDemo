using BlazorAppDemo.DataAccess.Repositories;
using BlazorAppDemo.Helpers;
using BlazorAppDemo.ViewModels;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BlazorAppDemo.Pages.Activity;

public partial class ManageActivity
{
    [Inject]
    private IActivityRepository activityRepos { get; set; }

    [Inject]
    private ILogger<ManageActivity> logger { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; }

    string message = "";
    int state = (int)States.None;

    List<DataAccess.Models.Activity> activities = new List<DataAccess.Models.Activity>();
    DataAccess.Models.Activity activity = new DataAccess.Models.Activity { ActivityDate = DateTime.UtcNow };

    protected override async Task OnInitializedAsync()
    {
        await LoadActivities();
    }

    async Task HandleSubmit(DataAccess.Models.Activity activity)
    {
        try
        {
            state = (int)States.Pending;
            if (activity.Id == 0)
            {
                await activityRepos.AddActivityAsync(activity);
                activities.Add(activity);
            }
            else
            {
                await activityRepos.UpdateActivityAsync(activity);
            }
            message = "Saved successfully";
            state = (int)States.Completed;
            HandleFormReset();
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            message = "Error has occured";
            state = (int)States.Error;
        }

    }

    async Task LoadActivities(DateTime? startDate = null, DateTime? endDate = null)
    {
        try
        {
            state = (int)States.Pending;
            if (startDate != null && endDate != null)
            {
                activities = (await activityRepos.GetAllActivitiesAsync(startDate, endDate)).ToList();

            }
            else
            {
                activities = (await activityRepos.GetAllActivitiesAsync()).ToList();
            }
            state = (int)States.Completed;
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            message = "Error has occured";
        }
    }

    async Task HandleDelete(
        BlazorAppDemo.DataAccess.Models.Activity activityToDelete)
    {
        try
        {
            state = (int)States.Pending;
            bool result = await JSRuntime.InvokeAsync<bool>("confirm", "Are you sure to delete??");
            if (result)
            {
                await activityRepos.DeleteActivityAsync(activityToDelete.Id);
                activities.Remove(activityToDelete);
                state = (int)States.Completed;
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex.Message);
            message = "Could not deleted";
            state = (int)States.Error;
        }
    }

    async Task HandleEdit(
        BlazorAppDemo.DataAccess.Models.Activity submitedActivity)
    {
        activity = submitedActivity;
    }


    void HandleFormReset()
    {
        activity.ActivityDate = DateTime.UtcNow;
        activity.TotalHours = 0;
        activity.Description = "";

    }

    void HandleSelectDate(FilterModel filters)
    {
        var startDate = filters.StartDate;
        var endDate = filters.EndDate;
        // if(startDate!=null & endDate!=null)
        // {

        // }
        // Console.WriteLine($"sDate=${startDate}, eDate=${endDate}");
    }
}
