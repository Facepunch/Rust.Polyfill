using System;

public struct ItemContainerId : IEquatable<ItemContainerId>
{
    public ulong Value;

    public bool IsValid => Value != 0;

    // Reserved value to return "don't move item" instead of adding struct to code
    public static readonly ItemContainerId Invalid = new ItemContainerId(ulong.MaxValue);

    public ItemContainerId(ulong value)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString("G");
    }

    public bool Equals(ItemContainerId other)
    {
        return Value == other.Value;
    }

    public override bool Equals(object obj)
    {
        return obj is ItemContainerId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public static bool operator ==(ItemContainerId left, ItemContainerId right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(ItemContainerId left, ItemContainerId right)
    {
        return !left.Equals(right);
    }
}
