using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

public interface IImmutableStack<T> : IEnumerable<T>
{
    int Count
    {
        [Pure] get;
    }

    [Pure]
    IImmutableStack<T> Push(T value);

    [Pure]
    IImmutableStack<T> Pop();

    [Pure]
    T Peek();
}

public class ImmutableStack<T> : IImmutableStack<T>
{
    private sealed class EmptyStack : IImmutableStack<T>
    {
        public int Count => 0;

        public IImmutableStack<T> Push(T value) => new ImmutableStack<T>(value, this);

        public IImmutableStack<T> Pop() { throw new InvalidOperationException("The stack is empty."); }

        public T Peek() { throw new InvalidOperationException("The stack is empty."); }

        public IEnumerator<T> GetEnumerator() { yield break; }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public static IImmutableStack<T> Empty { get; } = new EmptyStack();

    private readonly T _head;
    private readonly IImmutableStack<T> _tail;

    private ImmutableStack(T head, IImmutableStack<T> tail)
    {
        if (tail == null)
            throw new ArgumentNullException(nameof(tail));

        _head = head;
        _tail = tail;

        Count = tail.Count + 1;
    }

    public int Count { get; }

    public T Peek() => _head;

    public IImmutableStack<T> Pop() => _tail;

    public IImmutableStack<T> Push(T value) => new ImmutableStack<T>(value, this);

    public IEnumerator<T> GetEnumerator()
    {
        for (IImmutableStack<T> stack = this; stack.Count > 0; stack = stack.Pop())
        {
            yield return stack.Peek();
        }
    }

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
