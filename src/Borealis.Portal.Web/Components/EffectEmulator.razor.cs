using Borealis.Domain.Devices;
using Borealis.Domain.Effects;
using Borealis.Portal.Core.Effects;
using Borealis.Portal.Core.Exceptions;
using Borealis.Portal.Web.Extensions;

using Microsoft.AspNetCore.Components;



namespace Borealis.Portal.Web.Components;


public partial class EffectEmulator : ComponentBase, IDisposable
{
    private PeriodicTimer? _periodicTimer;


    /// <summary>
    /// The pixels of this ledstrip.
    /// </summary>
    protected PixelColor[] Pixels { get; set; } = default!;


    /// <summary>
    /// The output log that we have.
    /// </summary>
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


    // TODO: Make it so we change the length it will refresh.
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

        _logger.LogInformation($"Testing effect {Effect.Name}.");
        _logger.LogDebug("Creating engine.");

        EffectEngine? effectEngine = default!;

        try
        {
            // Creating engine.
            effectEngine = _effectEngineFactory.CreateEffectEngine(Effect,
                                                                   Length,
                                                                   new EffectEngineOptions
                                                                   {
                                                                       WriteLog = WriteLog
                                                                   });

            // Running the setup. This will also do a general test and run the loop function once.
            await effectEngine.RunSetupAsync();

            // If it has ran oke then we will start the background task.
            StartTimerTask(effectEngine);
        }
        catch (ArgumentNullException argumentNullException)
        {
            // Cleanup the effect engine.
            effectEngine?.Dispose();

            // Handle that Javascript is empty
            _logger.LogError(argumentNullException, "The Javascript is empty.");
            _snackbar.AddError("The Javascript is empty.");
        }
        catch (EffectEngineRuntimeException effectEngineRuntimeException)
        {
            // Cleanup the effect engine.
            effectEngine?.Dispose();

            // Handle a javascript runtime exception.
            _logger.LogError(effectEngineRuntimeException, "Error while playing effect.");
            _snackbar.AddError($"Error while running the javascript. {effectEngineRuntimeException.Message}.");
        }
        catch (EffectEngineException effectEngineException)
        {
            // Cleanup the effect engine.
            effectEngine?.Dispose();

            _logger.LogError(effectEngineException, "The Javascript is not valid.");
            _snackbar.AddError("The Javascript is not valid.");
        }
    }


    /// <summary>
    /// Starts the task that runs the emulator.
    /// </summary>
    /// <param name="engine"> The <see cref="EffectEngine" /> we become owner of. </param>
    private void StartTimerTask(EffectEngine engine)
    {
        _ = Task.Run(async () =>
        {
            _logger.LogDebug("Starting the task for the loop.");
            _periodicTimer = new PeriodicTimer(TimeSpan.FromMilliseconds(1 / Frequency * 1000));

            try
            {
                while (await _periodicTimer.WaitForNextTickAsync())
                {
                    ReadOnlyMemory<PixelColor> colors = await engine.RunLoopAsync();

                    // Copping the values from the array we got to the array we got.
                    colors.CopyTo(Pixels);
                    ;

                    // TODO: This is not working. Its really slow. We need to find a better way of doing this maybe running the script in the browser.
                    // Updating the UI.
                    await InvokeAsync(StateHasChanged);
                }
            }
            catch (EffectEngineRuntimeException e)
            {
                _periodicTimer?.Dispose();
                _periodicTimer = null;
                StateHasChanged();

                // Handle a javascript runtime exception.
                _logger.LogError(e, "Error while playing the loop function.");
                _snackbar.AddError("Error running loop function,");
            }
            finally
            {
                engine.Dispose();
            }
        });
    }


    /// <summary>
    /// Writes a message to the log.
    /// </summary>
    /// <param name="message"> The message we want to log </param>
    private void WriteLog(string message)
    {
        LogOutput += $"{DateTime.Now} - {message} {Environment.NewLine}";
        InvokeAsync(StateHasChanged);
    }


    /// <summary>
    /// Stops the emulation of effect.
    /// </summary>
    public void Stop()
    {
        if (_periodicTimer == null) return;
        _logger.LogInformation("Stopping emulation of effect script.");

        // Stopping and cleaning the task ang timer.
        _periodicTimer?.Dispose();
        _periodicTimer = null;
        StateHasChanged();

        // Informing the user.
        _logger.LogInformation("Stopped emulation.");
        _snackbar.AddError($"Stopping the emulation on effect {Effect.Name}.");
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _periodicTimer?.Dispose();
        _periodicTimer = null;
    }
}