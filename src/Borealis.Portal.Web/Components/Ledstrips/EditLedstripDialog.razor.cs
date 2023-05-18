using Borealis.Domain.Ledstrips;

using Microsoft.AspNetCore.Components;

using MudBlazor;



namespace Borealis.Portal.Web.Components.Ledstrips;


public partial class EditLedstripDialog : ComponentBase
{
    /// <summary>
    /// The ledstrip that we are editing.
    /// </summary>
    [Parameter]
    public Ledstrip? Ledstrip { get; set; }


    [CascadingParameter]
    public MudDialogInstance MudDialog { get; set; }


    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        Ledstrip ??= new Ledstrip();

        base.OnParametersSet();
    }


    protected virtual void Save()
    {
        // Send back the updated ledstrip.
        MudDialog.Close(Ledstrip);
    }


    protected virtual void Cancel()
    {
        MudDialog.Cancel();
    }


    /// <summary>
    /// Generates the dialog parameters for showing the dialog.
    /// </summary>
    /// <param name="ledstrip"> The ledstrip that we want to edit in this dialog. </param>
    /// <returns> A <see cref="DialogParameters" /> that contains the ledstrip parameter. </returns>
    public static DialogParameters GenerateDialogParameters(Ledstrip? ledstrip = null)
    {
        return new DialogParameters
        {
            { nameof(Ledstrip), ledstrip }
        };
    }
}