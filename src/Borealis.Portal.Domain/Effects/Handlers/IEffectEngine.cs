using Borealis.Domain.Effects;
using Borealis.Portal.Domain.Common;
using Borealis.Portal.Domain.Exceptions;



namespace Borealis.Portal.Domain.Effects.Handlers;


public interface IEffectEngine : IDisposable
{
    /// <summary>
    /// The effect we want to run.
    /// </summary>
    Effect Effect { get; }

    /// <summary>
    /// The length of the ledstrip.
    /// </summary>
    int PixelCount { get; }

    /// <summary>
    /// A flag indicating that we have run the startup and ready to start sending out frames.
    /// </summary>
    bool Ready { get; }


    /// <summary>
    /// Initializes the engine with all the parameters to be able to run the effect.
    /// </summary>
    /// <param name="token"> </param>
    /// <returns> </returns>
    Task InitializeAsync(CancellationToken token = default);


    /// <summary>
    /// Runs the setup function in the javascript.
    /// </summary>
    /// <exception cref="EffectEngineRuntimeException"> Thrown when there is a problem running the javascript. </exception>
    /// <exception cref="OperationCanceledException"> When the operation has been cancelled by the token. </exception>
    /// <returns> </returns>
    Task RunSetupAsync(CancellationToken token = default);


    /// <summary>
    /// Runs the loop function in the javascript.
    /// </summary>
    /// <exception cref="EffectEngineRuntimeException"> Thrown when there is a problem running the javascript. </exception>
    /// <returns> A <see cref="ReadOnlyMemory{PixelColor}" /> of the colors that we should display on the ledstrip. </returns>
    ValueTask<ReadOnlyMemory<PixelColor>> RunLoopAsync();
}