using System;
using System.Linq;

using Borealis.Domain.Ledstrips;
using Borealis.Portal.Domain.Devices.Models;

using Microsoft.AspNetCore.Components;

using MudBlazor;



namespace Borealis.Portal.Web.Components.Devices;


public partial class EditDevicePortDialog : ComponentBase
{
    private byte[] _takenBusses;

    /// <summary>
    /// The device that we want to edit on.
    /// </summary>
    [Parameter]
    public Device Device { get; set; }

    /// <summary>
    /// The device port that we want to edit
    /// </summary>
    [Parameter]
    public DevicePort? Port { get; set; }


    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; }


    /// <summary>
    /// The device connection model used for configuring the device.
    /// </summary>
    public DeviceConnectionModel Model { get; set; }

    public List<Ledstrip> SelectableLedstrips { get; set; } = new List<Ledstrip>();


    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        SelectableLedstrips = new List<Ledstrip>(await _ledstripManager.GetUnoccupiedLedstripsAsync());

        Model = Port != null
            ? new DeviceConnectionModel
            {
                Bus = Port.Bus,
                Ledstrip = Port.Ledstrip
            }
            : new DeviceConnectionModel();

        _takenBusses = Device.Ports.Select(x => x.Bus).ToArray();

        await base.OnParametersSetAsync();
    }


    public void Cancel()
    {
        MudDialog.Cancel();
    }


    public void Save()
    {
        if (Port == null)
        {
            if (Model.Ledstrip == null)
            {
                Device.AddBus(Model.Bus);
            }
            else
            {
                Device.AttachLedstrip(Model.Bus, Model.Ledstrip);
            }
        }
        else
        {
            if (Model.Bus != Port.Bus)
            {
                Device.ChangeBusId(Port.Bus, Model.Bus);
            }

            if (Model.Ledstrip == null)
            {
                Device.DetachLedstrip(Model.Ledstrip!);
            }
            else
            {
                if (Port.Ledstrip != null)
                {
                    Device.DetachLedstrip(Port.Ledstrip);
                }

                Device.AttachLedstrip(Model.Bus, Model.Ledstrip);
            }
        }

        MudDialog.Close(Device);
    }


    /// <summary>
    /// Generates the parameters that are needed for this dialog.
    /// </summary>
    /// <param name="device"> The device that we want to edit a port on. </param>
    /// <param name="devicePort"> The device port that we want to edit or null if there is none. </param>
    /// <returns> A <see cref="DialogParameters" /> with the parameters set for the dialog. </returns>
    public static DialogParameters GenerateDialogParameters(Device device, DevicePort? devicePort = null)
    {
        return new DialogParameters
        {
            { nameof(Device), device },
            { nameof(Port), devicePort }
        };
    }


    private IEnumerable<string> ValidateBus(byte bus)
    {
        if (_takenBusses.Any(x => x == bus)) yield return "The bus is already taken.";
    }
}



public class DeviceConnectionModel
{
    public byte Bus { get; set; }

    public Ledstrip? Ledstrip { get; set; }
}