using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Effects;
using Borealis.Portal.Web.Extensions;

using Microsoft.AspNetCore.Components;



namespace Borealis.Portal.Web.Components;


public partial class LedstripEmulator : ComponentBase, IDisposable
{
    private PeriodicTimer? _periodicTimer;


    /// <summary>
    /// The pixels of this ledstrip.
    /// </summary>
    protected PixelColor[] Pixels { get; set; } = default!;

    public string LogOutput { get; set; } = string.Empty;


    /// <summary>
    /// Is running indicating that we are running a effect on the animation currently.
    /// </summary>
    public bool IsRunning => _periodicTimer != null;


    /// <summary>
    /// Binding for the is running.
    /// </summary>
    public EventCallback<bool> IsRunningChanged { get; set; }


    /// <summary>
    /// The frequency in hertz.
    /// </summary>
    [Parameter]
    public double Frequency { get; set; } = 1;


    /// The
    /// <summary>
    /// The Length of the ledstrip.
    /// </summary>
    [Parameter]
    public int Length { get; set; } = 20;


    /// <summary>
    /// The color spectrum that we can use show
    /// </summary>
    [Parameter]
    public ColorSpectrum ColorSpectrum { get; set; }

    /// <summary>
    /// The effect we want to test on the ledstrip.
    /// </summary>
    [Parameter]
    public Effect Effect { get; set; } = default!;


    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        Pixels = new PixelColor[Length];

        base.OnParametersSet();
    }


    public async Task StartAsync()
    {
        if (_periodicTimer != null) return;

        EffectEngine effectEngine = _effectEngineFactory.CreateEffectEngine(Effect,
                                                                            Length,
                                                                            new EffectEngineOptions
                                                                            {
                                                                                WriteLog = s =>
                                                                                {
                                                                                    LogOutput += $"{DateTime.Now} - {s} {Environment.NewLine}";
                                                                                    InvokeAsync(StateHasChanged);
                                                                                }
                                                                            });

        try
        {
            await effectEngine.RunSetupAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error while playing effect.");
            _snackbar.AddError($"{e.Message}, Error while running the setup function.");
        }

        _ = Task.Run(async () =>
        {
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1 / Frequency * 1000));

            try
            {
                while (await _periodicTimer.WaitForNextTickAsync())
                {
                    ReadOnlyMemory<PixelColor> colors = await effectEngine.RunLoopAsync();

                    Pixels = colors.ToArray();
                    await InvokeAsync(StateHasChanged);

                    // HACK: Maybe need to add a StateHasChanged()
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while playing the loop function.");
                _snackbar.AddError("Error running loop function,");
            }
            finally
            {
                //_periodicTimer.Dispose(); aLREADY CALLING IT  
                effectEngine.Dispose();
            }
        });
    }


    public async Task StopAsync()
    {
        if (_periodicTimer == null) return;

        _periodicTimer?.Dispose();
        _periodicTimer = null;
    }


    protected virtual void ResetLedstrip()
    {
        // TODO: Validate length.

        Pixels = new PixelColor[Length];
        StateHasChanged();
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _periodicTimer?.Dispose();
    }
}