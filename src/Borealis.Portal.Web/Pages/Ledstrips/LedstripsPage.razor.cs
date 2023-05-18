using Borealis.Domain.Ledstrips;
using Borealis.Portal.Web.Components.Ledstrips;
using Borealis.Portal.Web.Extensions;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Extensions;



namespace Borealis.Portal.Web.Pages.Ledstrips;


public partial class LedstripsPage : ComponentBase
{
    /// <summary>
    /// The ledstrips.
    /// </summary>
    public List<Ledstrip> Ledstrips { get; set; } = new List<Ledstrip>();


    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        _logger.LogInformation("Opening the ledstrips page.");
        Ledstrips = new List<Ledstrip>(await _ledstripManager.GetLedstripsAsync());

        await base.OnParametersSetAsync();
    }


    protected virtual async Task OnAddAsync()
    {
        IDialogReference reference = await _dialogService.ShowAsync<EditLedstripDialog>("Add Ledstrip", EditLedstripDialog.GenerateDialogParameters());

        DialogResult result = await reference.Result;

        // Check if we cancelled the dialog.
        if (result.Canceled)
        {
            _logger.LogDebug("Edit ledstrip cancelled.");

            return;
        }

        // Reading the ledstrip and saving it.
        Ledstrip editedLedstrip = result.Data.As<Ledstrip>();
        await _ledstripManager.SaveAsync(editedLedstrip);
        _logger.LogInformation("Ledstrip saved!");
        _snackbar.AddSuccess("Ledstrip saved!");
        StateHasChanged();
    }


    protected virtual async Task OnEditLedstripAsync(Ledstrip ledstrip)
    {
        _logger.LogInformation("Opening the edit ledstrip dialog.");

        IDialogReference reference = await _dialogService.ShowAsync<EditLedstripDialog>("Edit Ledstrip", EditLedstripDialog.GenerateDialogParameters(ledstrip));

        DialogResult result = await reference.Result;

        // Check if we cancelled the dialog.
        if (result.Canceled)
        {
            _logger.LogDebug("Edit ledstrip cancelled.");

            return;
        }

        // Reading the ledstrip and saving it.
        Ledstrip editedLedstrip = result.Data.As<Ledstrip>();
        await _ledstripManager.SaveAsync(editedLedstrip);
        _logger.LogInformation("Ledstrip saved!");
        _snackbar.AddSuccess("Ledstrip saved!");
    }


    protected virtual async Task OnDeleteLedstripAsync(Ledstrip ledstrip)
    {
        // Asking if we ware sure that we want to delete the ledstrip.
        bool result = await _dialogService.ShowMessageBox("Delete Ledstrip?", $"Are you sure you would like to delete {ledstrip.Name}?") ?? false;

        if (!result) return;

        // Deleting the ledstrip.
        _logger.LogInformation("Delete ledstrip");
        await _ledstripManager.DeleteAsync(ledstrip);
        Ledstrips.Remove(ledstrip);

        StateHasChanged();
    }
}