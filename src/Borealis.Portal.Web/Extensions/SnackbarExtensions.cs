using MudBlazor;



namespace Borealis.Portal.Web.Extensions;


public static class SnackbarExtensions
{
	public static void AddSuccess(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Success);
	}


	public static void AddWarning(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Warning);
	}


	public static void AddNormal(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message);
	}


	public static void AddError(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Error);
	}


	public static void AddInfo(this ISnackbar snackbar, string message)
	{
		snackbar.Add(message, Severity.Info);
	}
}