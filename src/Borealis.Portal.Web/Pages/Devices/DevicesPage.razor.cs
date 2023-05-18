using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Exceptions;
using Borealis.Portal.Web.Extensions;

using Microsoft.AspNetCore.Components;



namespace Borealis.Portal.Web.Pages.Devices;


public partial class DevicesPage : ComponentBase
{
    /// <summary>
    /// The devices we have in the application.
    /// </summary>
    public List<Device> Devices { get; set; } = new List<Device>();


    #region Page

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        _logger.LogTrace("Opening the devices page.");
        Devices = new List<Device>(await _deviceManager.GetDevicesAsync());

        await base.OnParametersSetAsync();
    }

    #endregion


    #region Navigation

    protected virtual void NavigateToNewDevicePage()
    {
        _logger.LogTrace("Navigating to new device page.");
        _NavigationManager.NavigateTo("devices/new");
    }


    protected virtual void NavigateToEditDevicePage(Device device)
    {
        _logger.LogTrace($"Navigating to edit device page for device {device}.");
        _NavigationManager.NavigateTo($"devices/edit/{device.Id}");
    }

    #endregion


    #region Functionality

    protected virtual async Task ConnectAsync(Device device)
    {
        _logger.LogInformation($"Connecting to device {device.Id}.");

        try
        {
            _snackbar.AddInfo($"Starting connection with {device.Name}");

            // Connect to the device.
            await _deviceService.ConnectAsync(device);

            // Informing the user.
            StateHasChanged();
            _snackbar.AddSuccess($"Connected to device {device.Name}!");
            _logger.LogInformation($"Connected to device {device.Id}.");
        }
        catch (InvalidOperationException invalidOperationException)
        {
            // When we are trying to connect to a device that is already connected.
            _snackbar.AddError("Already connected to this device.");
            _logger.LogError(invalidOperationException, $"Already connected to device {device.Id}.");
        }
        catch (NotImplementedException notImplementedException)
        {
            // Connection type is not implemented.

            _snackbar.AddError("The connection type we selected has not been implemented.");
            _logger.LogError(notImplementedException, "We have not implemented the selected connection type.");
        }
        catch (DeviceConnectionException connectionException)
        {
            // Handle connection errors.
            _snackbar.AddError("There was a problem with the connection see logs for details.");
            _logger.LogError(connectionException, "Connection problems see exception.");
        }
    }


    protected virtual async Task DisconnectAsync(Device device)
    {
        _logger.LogInformation($"Disconnecting to device {device.Name}.");

        try
        {
            _snackbar.AddInfo($"Disconnecting from {device.Name}");

            // Connect to the device.
            await _deviceService.DisconnectAsync(device);

            StateHasChanged();

            // Inform the user and log.
            _snackbar.AddSuccess($"Disconnected from {device.Name}.");
            _logger.LogInformation($"Disconnected from device {device.Name}.");
        }
        catch (InvalidOperationException invalidOperationException)
        {
            // When we are trying to connect to a device that is already connected.
            _snackbar.AddError("The device is not connected.");
            _logger.LogError(invalidOperationException, $"The device {device.Id} is not connected.");
        }
    }

    #endregion
}



///// ---------- Dialog ------------

///// <summary>
///// Opens the add new animation dialog.
///// </summary>
//protected virtual void OpenCreateDialog()
//{
//    _logger.LogDebug($"Opening the add new device dialog.");

//    // Make sure that the dialog is in edit mode.
//    _editMode = true;
//    StateHasChanged();

//    // Showing the dialog.
//    _editDeviceDialog = true;
//}

///// <summary>
///// Opens the dialog in a edit mode.
///// </summary>
///// <param name="device"></param>
//protected virtual void OpenEditDialog(Device device)
//{
//    _logger.LogDebug($"Opening edit device dialog for {device.Id}");

//    // Setting the fields in the dialog.
//    _selectedDevice = device;
//    _deviceName = device.Name;
//    _ipAddress = device.EndPoint.Address.ToString();
//    _port = device.EndPoint.Port;

//    // Make sure that the dialog is in edit mode.
//    _editMode = true;
//    StateHasChanged();

//    // Showing the dialog.
//    _editDeviceDialog = true;
//}

///// <summary>
///// Closes the add new animation dialog.
///// </summary>
//protected virtual void CloseDialog()
//{
//    _logger.LogDebug($"Closing the add new device dialog.");

//    // Closing the dialog.
//    _editDeviceDialog = false;
//}

///// <summary>
///// Clears and resets the fields in the Add new Animation Dialog.
///// </summary>
//protected virtual void ClearDialog()
//{
//    _logger.LogDebug($"Clearing the add new device dialog.");

//    // Resetting the fields in the dialog.
//    _selectedDevice = null;
//    _deviceName = string.Empty;
//    _ipAddress = String.Empty;
//    _port = 0;
//}

///// <summary>
///// Getting the options for the dialog.
///// </summary>
///// <returns>The dialog options.</returns>
//private DialogOptions GetDialogOptions()
//{
//    return new DialogOptions()
//    {
//        CloseOnEscapeKey = true,
//        Position = DialogPosition.Center,
//        FullWidth = true
//    };
//}