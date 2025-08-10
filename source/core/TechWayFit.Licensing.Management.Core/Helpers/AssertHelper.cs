using System;
using TechWayFit.Licensing.Management.Core.Models.Product;

namespace TechWayFit.Licensing.Management.Core.Helpers;


public class Ensure
{
    public static Ensure<T> NotNull<T>(T value, string paramName) where T : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
        return new Ensure<T>(value, paramName);
    }

    public static Ensure<string> NotEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", paramName);
        }
        return new Ensure<string>(value, paramName);
    }

    public static Ensure<T> NotDefault<T>(T value, string paramName) where T : struct
    {
        if (EqualityComparer<T>.Default.Equals(value, default(T)))
        {
            throw new ArgumentException("Value cannot be the default value.", paramName);
        }
        return new Ensure<T>(value, paramName);
    }
}

public class Ensure<T>
{
    public T Value { get; private set; }
    public string ParamName { get; private set; }

    public Ensure(T value, string paramName)
    {
        Value = value;
        ParamName = paramName;
    }
    public Ensure<TSub> NotNull<TSub>(TSub value, string paramName) where TSub : class
    {
        if (value == null)
        {
            throw new ArgumentNullException(paramName);
        }
        return new Ensure<TSub>(value, paramName);
    }

    public Ensure<string> NotEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty or whitespace.", paramName);
        }
        return new Ensure<string>(value, paramName);
    }

    public Ensure<TSub> NotDefault<TSub>(TSub value, string paramName) where TSub : struct
    {
        if (EqualityComparer<TSub>.Default.Equals(value, default(TSub)))
        {
            throw new ArgumentException("Value cannot be the default value.", paramName);
        }
        return new Ensure<TSub>(value, paramName);
    }
}

public static class StringEnsure
{
    public static Ensure<string> MinLength(this Ensure<string> ensure, int minLength)
    {
        if (ensure.Value == null || ensure.Value.Length < minLength)
        {
            throw new ArgumentException($"String must be at least {minLength} characters long.", ensure.ParamName);
        }
        return ensure;

    }
    public static Ensure<string> MaxLength(this Ensure<string> ensure, int maxLength)
    {
        if (ensure.Value != null && ensure.Value.Length > maxLength)
        {
            throw new ArgumentException($"String must not exceed {maxLength} characters.", ensure.ParamName);
        }
        return ensure;
    }
    public static Ensure<string> MatchRegex(this Ensure<string> ensure, string pattern)
    {
        if (ensure.Value == null || !System.Text.RegularExpressions.Regex.IsMatch(ensure.Value, pattern))
        {
            throw new ArgumentException($"String does not match the required pattern: {pattern}.", ensure.ParamName);
        }
        return ensure;
    }
}
public static class GuidEnsure
{
    public static Ensure<Guid> NotEmpty(this Ensure<Guid> ensure)
    {
        if (ensure.Value == Guid.Empty)
        {
            throw new ArgumentException("Guid cannot be empty.", ensure.ParamName);
        }
        return ensure;
    }

    public static Ensure<Guid?> NotNullOrEmpty(this Ensure<Guid?> ensure, Guid? value, string paramName)
    {
        if (!value.HasValue || value.Value == Guid.Empty)
        {
            throw new ArgumentException("Guid cannot be null or empty.", paramName);
        }
        return ensure;
    }
}
public static class NumberEnsure
{
    public static Ensure<int> Positive(this Ensure<int> ensure)
    {
        if (ensure.Value <= 0)
        {
            throw new ArgumentException("Number must be positive.", ensure.ParamName);
        }
        return ensure;
    }
    public static Ensure<int> Between(this Ensure<int> ensure, int min, int max)
    {
        if (ensure.Value < min || ensure.Value > max)
        {
            throw new ArgumentOutOfRangeException(ensure.ParamName, $"Number must be between {min} and {max}.");
        }
        return ensure;
    }
    public static Ensure<decimal> Positive(this Ensure<decimal> ensure)
    {
        if (ensure.Value <= 0)
        {
            throw new ArgumentException("Number must be positive.", ensure.ParamName);
        }
        return ensure;
    }
    public static Ensure<decimal> NotNegative(this Ensure<decimal> ensure)
    {
        if (ensure.Value < 0)
        {
            throw new ArgumentException("Number cannot be negative.", ensure.ParamName);
        }
        return ensure;
    }
    public static Ensure<decimal> Between(this Ensure<decimal> ensure, decimal min, decimal max)
    {
        if (ensure.Value < min || ensure.Value > max)
        {
            throw new ArgumentOutOfRangeException(ensure.ParamName, $"Number must be between {min} and {max}.");
        }
        return ensure;
    }

}
public static class DateTimeEnsure
{
    public static Ensure<DateTime> NotInFuture(this Ensure<DateTime> ensure)
    {
        if (ensure.Value > DateTime.Now)
        {
            throw new ArgumentException("DateTime cannot be in the future.", ensure.ParamName);
        }
        return ensure;
    }

    public static Ensure<DateTime> NotInPast(this Ensure<DateTime> ensure)
    {
        if (ensure.Value < DateTime.Now)
        {
            throw new ArgumentException("DateTime cannot be in the past.", ensure.ParamName);
        }
        return ensure;
    }
}
public static class MoneyEnsure
{
    public static Ensure<Money> Positive(this Ensure<Money> ensure)
    {
        if (ensure.Value.Amount <= 0)
        {
            throw new ArgumentException("Amount must be positive.", ensure.ParamName);
        }
        return ensure;
    }

    public static Ensure<Money> NotNegative(this Ensure<Money> ensure)
    {
        if (ensure.Value.Amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative.", ensure.ParamName);
        }
        return ensure;
    }
}

