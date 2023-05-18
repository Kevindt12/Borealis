using Borealis.Domain.Effects;
using Borealis.Portal.Web.Components.Effects;
using Borealis.Portal.Web.Extensions;
using Borealis.Portal.Web.Models.Effects;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Extensions;



namespace Borealis.Portal.Web.Pages.Effects;


public partial class EffectsPage : ComponentBase
{
    public int SelectedEffectIndex { get; set; }


    public List<Effect> Effects { get; set; } = new List<Effect>();


    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        _logger.LogInformation("Loading effects page.");
        Effects = new List<Effect>(await _effectManager.GetEffectsAsync());

        await base.OnParametersSetAsync();
    }


    #region Functionality

    // -------- Functionality ----------


    protected virtual IEnumerable<string> ValidateParameterIdentifier(string identifier)
    {
        // Spaces
        if (identifier.Contains(' '))
        {
            yield return "No spaces allowed!";
        }
    }

    #endregion


    // ------------- Page -------------


    #region Event Handlers

    protected virtual async Task OnAddEffectAsync()
    {
        IDialogReference reference = await _dialogService.ShowAsync<CreateEffectDialog>("Create");

        DialogResult result = await reference.Result;

        if (result.Canceled)
        {
            return;
        }

        CreateEffectDialogModel model = result.Data.As<CreateEffectDialogModel>();

        // Creating the effect via a factory because of the template javascript..
        Effect effect = _EffectFactory.CreateEffect(model.EffectName);

        // Saving the effect and adding it to the list.
        await _effectManager.SaveEffectAsync(effect);
        Effects.Add(effect);

        // Notify the user that we have created the effect.
        _logger.LogInformation($"Created effect {effect.Name}.");
        _snackbar.AddSuccess("Created effect!");
    }


    protected virtual async Task OnSaveAsync()
    {
        // Selected page effect.
        Effect effect = Effects[SelectedEffectIndex];

        _logger.LogInformation($"Saving effect {effect.Name}.");
        await _effectManager.SaveEffectAsync(effect);

        _logger.LogInformation($"Effect {effect.Name} has been saved.");
        _snackbar.AddSuccess("Saved effect!");
    }


    protected virtual void OnEffectParameterRemoved(Effect effect, EffectParameter parameter)
    {
        effect.EffectParameters.Remove(parameter);
        StateHasChanged();
    }


    protected virtual async Task OnCommitEffectFileAsync(Effect selectedEffect, EffectFile selectedEffectFile)
    {
        // Create a new copy of the effect file.
        EffectFile newFile = new EffectFile(selectedEffectFile.Javascript);

        // Add the new file so that we create a copy of the latest file.
        selectedEffect.Files.Add(newFile);

        StateHasChanged();
    }


    protected virtual async Task OnEditorValueChanged(EffectFile selectedEffectFile, string content)
    {
        // Set the content.
        selectedEffectFile.Javascript = content;
    }


    protected virtual async Task OnAddParameterAsync(Effect effect)
    {
        // Create
        EffectParameter parameter = new EffectParameter();

        // Add the parameter.
        effect.EffectParameters.Add(parameter);

        StateHasChanged();
    }


    protected virtual async Task OnRemoveEffectParameterAsync(Effect effect, EffectParameter parameter)
    {
        effect.EffectParameters.Remove(parameter);

        StateHasChanged();
    }

    #endregion
}