using System.Net;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Web.Components.Devices;
using Borealis.Portal.Web.Extensions;

using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Extensions;



namespace Borealis.Portal.Web.Pages.Devices;


public partial class EditDevicePage : ComponentBase
{
    public bool Connected => _deviceService.IsConnected(Device);

    private string? _ipAddressProxy;


    private readonly IMask _ipv4Mask = RegexMask.IPv4();


    [Parameter]
    public Guid DeviceId { get; set; }

    /// <summary>
    /// The device that we wnat to edit.
    /// </summary>
    public Device Device { get; set; } = new Device();


    public string IPAddressProxy
    {
        get => _ipAddressProxy ??= Device.EndPoint.Address.ToString();
        set
        {
            _ipAddressProxy = value;

            if (!IPAddress.TryParse(value, out IPAddress? address)) return;
            Device.EndPoint.Address = address!;
        }
    }


    #region Lists

    // All the onoccupied ledstrips that we know of.
    public List<Ledstrip> UnoccupiedLedstrips { get; set; } = new List<Ledstrip>();

    #endregion


    #region Page

    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        // Loading device.
        _logger.LogTrace($"Loading edit device page for {DeviceId}.");
        Device = await _deviceManager.GetDeviceById(DeviceId).ConfigureAwait(true) ?? new Device();

        // Loading all the lists
        UnoccupiedLedstrips = new List<Ledstrip>(await _ledstripManager.GetUnoccupiedLedstripsAsync());

        await base.OnParametersSetAsync();
    }

    #endregion


    #region EventHandlers

    protected virtual async Task OnAddDevicePortAsync()
    {
        IDialogReference reference = await _dialogService.ShowAsync<EditDevicePortDialog>("Add", EditDevicePortDialog.GenerateDialogParameters(Device));
        DialogResult result = await reference.Result;

        if (result.Canceled) return;

        _logger.LogDebug("Dialog returned success updating the device.");
        Device = result.Data.As<Device>();
    }


    protected virtual async Task OnEditDevicePortAsync(DevicePort model)
    {
        _logger.LogInformation($"Editing the device port of {model.Id}");

        IDialogReference reference = await _dialogService.ShowAsync<EditDevicePortDialog>("Edit", EditDevicePortDialog.GenerateDialogParameters(Device, model));
        DialogResult result = await reference.Result;

        if (result.Canceled) return;

        _logger.LogDebug("Dialog returned success updating the device.");
        Device = result.Data.As<Device>();
        StateHasChanged();
    }


    protected virtual async Task OnDeleteDevicePortAsync(DevicePort model)
    {
        Device.RemoveBus(model.Bus);
    }


    protected virtual async Task OnSaveAsync()
    {
        _logger.LogInformation($"Saving device configuration {Device.Name}.");

        await _deviceManager.SaveDeviceAsync(Device);

        _logger.LogInformation("Saved device configuration.");
        _snackbar.AddSuccess("Device configuration saved.");
    }


    protected virtual async Task OnUploadAsync()
    {
        _logger.LogInformation($"Uploading device configuration {Device.Name}.");

        if (!_deviceService.IsConnected(Device))
        {
            _logger.LogError("Cannot upload device configuration disconnected.");
            _snackbar.AddError("Not connected to device!");
        }

        try
        {
            await _deviceService.UploadDeviceConfigurationAsync(Device);

            _logger.LogInformation("Device configuration uploaded.");
            _snackbar.AddSuccess("Device configuration uploaded.");
        }
        catch (Exception e)
        {
            // TODO: Handle exceptions
        }
    }


    protected virtual async Task OnConnectAsync()
    {
        _logger.LogInformation($"Validating device configuration {Device.Name}.");

        try
        {
            await _deviceService.ConnectAsync(Device);
        }
        catch (Exception e)
        {
            // TODO: Handle exceptions.
        }
    }


    protected virtual async Task OnDisconnectAsync()
    {
        _logger.LogInformation($"Validating device configuration {Device.Name}.");

        try
        {
            await _deviceService.DisconnectAsync(Device);
        }
        catch (Exception e)
        {
            // TODO: Handle exceptions.
        }
    }

    #endregion


    #region Functonality

    ///// <summary>
    ///// Saves the device to the application.
    ///// </summary>
    ///// <param name="name"> The name of the device, </param>
    ///// <param name="ipAddress"> The Ip Address of the device. </param>
    ///// <param name="port"> The port of the device. </param>
    ///// <param name="device"> The device that we are editing. <c> null </c> when we are creating a device. </param>
    //protected virtual async Task SaveDeviceAsync(string name, string ipAddress, int port, Pages.Device? device)
    //{
    //    _logger.LogDebug($"Saving device {name}");

    //    try
    //    {
    //        // Checking and getting the endpoint.
    //        IPAddress address = IPAddress.Parse(ipAddress);
    //        IPEndPoint endpoint = new IPEndPoint(address, port);

    //        // Checking if we need to edit a existing one or create a new one.
    //        if (_selectedDevice != null)
    //        {
    //            // Making sure that we have a name.
    //            if (String.IsNullOrWhiteSpace(name)) throw new ArgumentNullException(nameof(name), "Name is empty or null");

    //            // Setting then name and endpoints.
    //            device!.Name = name;
    //            device.EndPoint = endpoint;
    //        }
    //        else
    //        {
    //            // Creating new device. | Note that the factory has its own exception validation.
    //            device = _deviceFactory.CreateDevice(name, endpoint);
    //        }

    //        // Saving the device.
    //        _logger.LogDebug("Validated device input saving the device.");
    //        await _deviceManager.SaveDeviceAsync(device);

    //        //// Refreshing and informing the user if the action.
    //        //StateHasChanged();
    //        //_snackbar.AddSuccess($"Device {device.Name} saved!");
    //        //_logger.LogInformation($"Device {device.Id}, has been saved. ");
    //    }
    //    catch (ArgumentNullException argumentNullException)
    //    {
    //        // Handle no name.

    //        _snackbar.AddError("The name was null or invalid.");
    //        _logger.LogError(argumentNullException, "The name was null or empty.");
    //    }
    //    catch (FormatException formatException)
    //    {
    //        // Handle invalid IP Address.

    //        _snackbar.AddError("The IP Address was not valid.");
    //        _logger.LogError(formatException, "The IP Address was invalid.");
    //    }
    //    catch (ArgumentOutOfRangeException argumentOutOfRangeException)
    //    {
    //        // Handle invalid Port.

    //        _snackbar.AddError("The port is out of range.");
    //        _logger.LogError(argumentOutOfRangeException, "The IP Address was invalid.");
    //    }

    //    //catch (DbUpdateException dbUpdateException)
    //    //{
    //    //    // Handle unable to save.

    //    //    _snackbar.AddError("Unable to save device to the database.");
    //    //    _logger.LogError(dbUpdateException, "Unable to save the device to the database.");
    //    //}
    //}

    #endregion
}