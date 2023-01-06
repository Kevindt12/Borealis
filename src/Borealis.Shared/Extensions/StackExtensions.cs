namespace Borealis.Shared.Extensions;


public static class StackExtensions
{
    /// <summary>
    /// Pushes the elements of the specified enumeration onto the stack in the order they are enumerated.
    /// </summary>
    /// <typeparam name="T"> The type of the elements in the stack. </typeparam>
    /// <param name="stack"> The stack to which the items should be pushed. </param>
    /// <param name="items"> The enumeration of items to push onto the stack. </param>
    public static void PushRange<T>(this Stack<T> stack, IEnumerable<T> items)
    {
        T[] array = items.ToArray();
        Array.Reverse(array);

        for (int i = 0; i < array.Length; i++)
        {
            stack.Push(array[i]);
        }
    }
}