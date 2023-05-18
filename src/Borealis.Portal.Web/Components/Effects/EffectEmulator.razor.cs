using System;
using System.Diagnostics;

using Borealis.Portal.Web.Extensions;

using System.Linq;

using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Devices.Models;
using Borealis.Portal.Domain.Effects.Handlers;
using Borealis.Portal.Domain.Exceptions;

using Microsoft.AspNetCore.Components;

using MudBlazor;



namespace Borealis.Portal.Web.Components.Effects;


public partial class EffectEmulator : ComponentBase, IDisposable
{
    private bool _framesDialogVisible;

    private CancellationTokenSource? _cts;


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
    public bool IsRunning => _cts != null;


    /// <summary>
    /// The latest frames that where rendered.
    /// </summary>
    public List<ReadOnlyMemory<PixelColor>> Frames { get; set; } = new List<ReadOnlyMemory<PixelColor>>();


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
    public int Length { get; set; } = 50;

    /// <summary>
    /// How many iterations we should have have of the emulation test.
    /// </summary>
    public int Iterations { get; set; } = 200;

    /// <summary>
    /// The amount of frames we wait before we take a sample.
    /// </summary>
    public int SampleRate { get; set; } = 5;


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
        if (_cts != null)
        {
            return;
        }

        _logger.LogInformation($"Testing effect {Effect.Name}.");
        _logger.LogDebug("Creating engine.");

        IEffectEngine? effectEngine = default!;

        try
        {
            // Creating engine.
            effectEngine = await _effectEngineFactory.CreateEffectEngineAsync(Effect,
                                                                              Length,
                                                                              new EffectEngineOptions
                                                                              {
                                                                                  WriteLog = WriteLog
                                                                              });

            _cts = new CancellationTokenSource();

            // Run the setup check if we it works.
            // Running the setup. This will also do a general test and run the loop function once.
            Stopwatch stopwatch = Stopwatch.StartNew();
            await effectEngine.RunSetupAsync(_cts.Token);

            // Displaying the result.
            stopwatch.Stop();
            WriteLog($"Effect setup done in {stopwatch.Elapsed}ms.");

            Frames.Clear();
            WriteLog("Start running loop.");
            stopwatch = Stopwatch.StartNew();

            // Runs the loop function 1000 times and calculates the time taken and if it works by sending out 
            for (int i = 0; i < Iterations; i++)
            {
                _cts.Token.ThrowIfCancellationRequested();

                if (i % SampleRate == 0)
                {
                    Frames.Add(await effectEngine.RunLoopAsync());
                }
                else
                {
                    await effectEngine.RunLoopAsync();
                }
            }

            // Displaying the result.
            stopwatch.Stop();
            WriteLog($"Effect setup done in {stopwatch.Elapsed}ms.");
        }
        catch (ArgumentNullException argumentNullException)
        {
            // Handle that Javascript is empty
            _logger.LogError(argumentNullException, "The Javascript is empty.");
            _snackbar.AddError("The Javascript is empty.");
        }
        catch (EffectEngineRuntimeException effectEngineRuntimeException)
        {
            // Handle a javascript runtime exception.
            _logger.LogError(effectEngineRuntimeException, "Error while playing effect.");
            _snackbar.AddError($"Error while running the javascript. {effectEngineRuntimeException.Message}.");
        }
        catch (EffectEngineException effectEngineException)
        {
            _logger.LogError(effectEngineException, "The Javascript is not valid.");
            _snackbar.AddError("The Javascript is not valid.");
        }
        catch (OperationCanceledException operationCanceledException)
        {
            _logger.LogError(operationCanceledException, "The operation was cancelled.");
            _snackbar.AddError("The operation was cancelled..");
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Unknown error.");
            _snackbar.AddError("Unknown error.");

            throw;
        }
        finally
        {
            // Cleanup the effect engine.
            effectEngine?.Dispose();
            _cts = null;
        }
    }


    /// ---------- Dialog ------------
    /// <summary>
    /// Opens the add new animation dialog.
    /// </summary>
    protected virtual void OpenDialog()
    {
        _framesDialogVisible = true;
    }


    /// <summary>
    /// Closes the add new animation dialog.
    /// </summary>
    protected virtual void CloseDialog()
    {
        _framesDialogVisible = false;
    }


    private DialogOptions GetDialogOptions()
    {
        return new DialogOptions
        {
            CloseOnEscapeKey = true,
            Position = DialogPosition.Center,
            FullWidth = true
        };
    }


    //  ------------- Logging --------------------


    /// <summary>
    /// Writes a message to the log.
    /// </summary>
    /// <param name="message"> The message we want to log </param>
    private void WriteLog(string message)
    {
        LogOutput += $"{DateTime.Now} - {message} {Environment.NewLine}";
        _logger.LogInformation(message);
        InvokeAsync(StateHasChanged);
    }


    /// <summary>
    /// Stops the emulation of effect.
    /// </summary>
    public void Stop()
    {
        if (_cts == null) return;
        _logger.LogInformation("Stopping emulation of effect script.");

        // Stopping and cleaning the task ang timer.
        _cts?.Dispose();
        _cts = null;
        StateHasChanged();

        // Informing the user.
        _logger.LogInformation("Stopped emulation.");
        _snackbar.AddError($"Stopping the emulation on effect {Effect.Name}.");
    }


    /// <inheritdoc />
    public void Dispose()
    {
        _cts?.Dispose();
        _cts = null;
    }
}