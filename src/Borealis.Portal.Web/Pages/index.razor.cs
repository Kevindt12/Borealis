using Borealis.Domain.Effects;
using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Web.Components.Common;
using Borealis.Portal.Web.Components.Effects;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Extensions;



namespace Borealis.Portal.Web.Pages;


public partial class Index : ComponentBase
{
    /// <summary>
    /// The devices that we have in the application.
    /// </summary>
    protected IReadOnlyList<Device> Devices { get; set; } = new List<Device>();


    /// <summary>
    /// The effects that we know of.
    /// </summary>
    protected IReadOnlyList<Effect> Effects { get; set; } = new List<Effect>();


    #region Page Handlers

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        _logger.LogInformation("Showing index page.");
        Devices = new List<Device>(await _deviceManager.GetDevicesAsync());

        await base.OnParametersSetAsync();
    }

    #endregion


    #region Event Handlers

    protected virtual async Task OnDevicePortEffectChangedAsync(DevicePort port, Effect effect)
    {
        // Asking the user that there sure they would want to switch animation on the ledstrip.
        bool areYouSureQuestion = await _dialogService.ShowMessageBox("Switching Effect", "Are you sure you would want to switch effects on this ledstrip?") ?? false;

        if (areYouSureQuestion) return;

        // Attach the new animation to the ledstrip.
        await _ledstripService.AttachEffectToLedstripAsync(port.Ledstrip!, effect);

        // Informing the page that the state has changed.
        StateHasChanged();
    }


    protected virtual async Task OnPauseLedstripAsync(DevicePort port)
    {
        // Stops the ledstrip from any action it was doing.
        await _ledstripService.StopLedstripAsync(port.Ledstrip!);

        // Informing the page that the state has changed.
        StateHasChanged();
    }


    protected virtual async Task OnStartAnimationOnLedstripAsync(DevicePort port)
    {
        // Starts playing the attached effect on the ledstrip.
        await _ledstripService.StartAnimationAsync(port.Ledstrip!);

        // Informing the page that the state has changed.
        StateHasChanged();
    }


    protected virtual async Task OnShowColorOnLedstripAsync(DevicePort port)
    {
        // Get the old color that was being displayed else create a new one.
        PixelColor color = _ledstripService.GetDisplayingColorOnLedstrip(port.Ledstrip!) ?? new PixelColor();

        // Show the color picker dialog.
        DialogResult colorPickerDialogResult = await (await _dialogService.ShowAsync<ColorPickerDialog>("Pick color", ColorPickerDialog.CreateParameters(color))).Result;

        if (colorPickerDialogResult.Canceled) return;

        // Stop the old color if there was one.
        if (_ledstripService.GetLedstripStatus(port.Ledstrip!) == LedstripStatus.DisplayingColor)
        {
            await _ledstripService.StopLedstripAsync(port.Ledstrip!);
        }

        // Show the new color on the ledstrip.
        await _ledstripService.SetSolidColorAsync(port.Ledstrip!, color);
    }


    protected virtual async Task OnChangeEffectParametersOnLedstripAsync(DevicePort port)
    {
        // Getting the attached effect.
        Effect attachedEffect = _ledstripService.GetAttachedEffect(port.Ledstrip!)!;

        // Opening and showing the dialog.
        IDialogReference dialogReference = await _dialogService.ShowAsync<EditEffectParametersDialog>("Edit Parameters", EditEffectParametersDialog.GenerateParameters(attachedEffect.EffectParameters));
        DialogResult dialogResult = await dialogReference.Result;

        // Set the new parameters.
        IReadOnlyList<EffectParameter> parameters = dialogResult.Data.As<IReadOnlyList<EffectParameter>>();
        attachedEffect.EffectParameters = new List<EffectParameter>(parameters);

        // Updating the page state.
        StateHasChanged();
    }

    #endregion


    #region Page Functonality

    protected virtual Effect? GetSelectedEffectByLedstrip(Ledstrip ledstrip)
    {
        return _ledstripService.GetAttachedEffect(ledstrip);
    }


    protected virtual bool IsDevicePortActive(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.PlayingAnimation or LedstripStatus.DisplayingColor;
    }


    protected virtual bool CanEffectBeChangedOnLedstrip(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.Idle or LedstripStatus.DisplayingColor;
    }


    protected virtual bool CanLedstripBePaused(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.PlayingAnimation or LedstripStatus.DisplayingColor;
    }


    protected virtual bool CanEffectBeStartedOnLedstrip(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.PausedAnimation or LedstripStatus.Idle;
    }


    protected virtual bool CanColorBeDisplayedOnLedstrip(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.Idle or LedstripStatus.DisplayingColor;
    }


    protected virtual bool CanEffectSettingsBeChangedOnLedstrip(DevicePort port)
    {
        if (!_deviceService.IsConnected(port.Device)) return false;

        LedstripStatus status = _ledstripService.GetLedstripStatus(port.Ledstrip!) ?? LedstripStatus.Idle;

        return status is LedstripStatus.PausedAnimation;
    }

    #endregion


    #region Functonality

    #endregion
}