using Borealis.Shared.Eventing;



namespace Borealis.Shared.Extensions;


public static class AsyncEventHandlerExtensions
{
	public static async Task InvokeAsync(this AsyncEventHandler handler, object? sender, EventArgs e)
	{
		Delegate[] delegates = handler.GetInvocationList();

		// Making sure we have delegates to invoke.
		if (delegates.Length == 0) return;

		// Getting and running the tasks.
		IEnumerable<Task> tasks = delegates.Cast<AsyncEventHandler>().Select(x => x.Invoke(sender, e));
		await Task.WhenAll(tasks);
	}


	public static async Task InvokeAsync(this AsyncCancelableEventHandler handler, object? sender, EventArgs e, CancellationToken token)
	{
		Delegate[] delegates = handler.GetInvocationList();

		// Making sure we have delegates to invoke.
		if (delegates.Length == 0) return;

		// Getting and running the tasks.
		IEnumerable<Task> tasks = delegates.Cast<AsyncCancelableEventHandler>().Select(x => x.Invoke(sender, e, token));
		await Task.WhenAll(tasks);
	}


	public static async Task InvokeAsync<TEventArgs>(this AsyncEventHandler<TEventArgs> handler, object? sender, TEventArgs e) where TEventArgs : EventArgs
	{
		Delegate[] delegates = handler.GetInvocationList();

		// Making sure we have delegates to invoke.
		if (delegates.Length == 0) return;

		// Getting and running the tasks.
		IEnumerable<Task> tasks = delegates.Cast<AsyncEventHandler<TEventArgs>>().Select(x => x.Invoke(sender, e));
		await Task.WhenAll(tasks);
	}


	public static async Task InvokeAsync<TEventArgs>(this AsyncCancelableEventHandler<TEventArgs> handler, object? sender, TEventArgs e, CancellationToken token) where TEventArgs : EventArgs
	{
		Delegate[] delegates = handler.GetInvocationList();

		// Making sure we have delegates to invoke.
		if (delegates.Length == 0) return;

		// Getting and running the tasks.
		IEnumerable<Task> tasks = delegates.Cast<AsyncCancelableEventHandler<TEventArgs>>().Select(x => x.Invoke(sender, e, token));
		await Task.WhenAll(tasks);
	}
}