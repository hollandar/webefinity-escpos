using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace EscPos.Commands;

public class ReceiptTemplateContext
{
    private readonly Dictionary<string, object?> _variables = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stack<Dictionary<string, object?>> _scopeStack = new();

    public ReceiptTemplateContext()
    {
    }

    public ReceiptTemplateContext(object model)
    {
        if (model is null)
            throw new ArgumentNullException(nameof(model));

        AddFromObject(model);
    }

    public void Add(string key, object? value)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("Key cannot be null or whitespace.", nameof(key));

        _variables[key] = value;
    }

    public void AddRange(IDictionary<string, object?> variables)
    {
        if (variables is null)
            throw new ArgumentNullException(nameof(variables));

        foreach (var kvp in variables)
        {
            _variables[kvp.Key] = kvp.Value;
        }
    }

    public void AddFromObject(object model, string? prefix = null)
    {
        if (model is null)
            return;

        var type = model.GetType();
        var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            if (!prop.CanRead)
                continue;

            var key = string.IsNullOrEmpty(prefix) ? prop.Name : $"{prefix}.{prop.Name}";
            var value = prop.GetValue(model);

            _variables[key] = value;
        }
    }

    public object? GetValue(string key)
    {
        if (_variables.TryGetValue(key, out var value))
            return value;

        if (key.Contains('.'))
        {
            return GetNestedValue(key);
        }

        return null;
    }

    private object? GetNestedValue(string path)
    {
        var parts = path.Split('.');
        if (parts.Length == 0)
            return null;

        if (!_variables.TryGetValue(parts[0], out var current))
            return null;

        for (var i = 1; i < parts.Length && current is not null; i++)
        {
            var propertyName = parts[i];
            var type = current.GetType();
            var property = type.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

            if (property is null || !property.CanRead)
                return null;

            current = property.GetValue(current);
        }

        return current;
    }

    public string Substitute(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        return Regex.Replace(text, @"\$\{([^}]+)\}", match =>
        {
            var variableName = match.Groups[1].Value.Trim();
            var value = GetValue(variableName);

            if (value is null)
                return string.Empty;

            if (value is IFormattable formattable)
            {
                var formatParts = variableName.Split(':');
                if (formatParts.Length > 1)
                {
                    var format = formatParts[1];
                    return formattable.ToString(format, null);
                }
            }

            return value.ToString() ?? string.Empty;
        });
    }

    public static ReceiptTemplateContext FromDictionary(IDictionary<string, object?> variables)
    {
        var context = new ReceiptTemplateContext();
        context.AddRange(variables);
        return context;
    }

    public static ReceiptTemplateContext FromAnonymous(object anonymousObject)
    {
        if (anonymousObject is null)
            throw new ArgumentNullException(nameof(anonymousObject));

        return new ReceiptTemplateContext(anonymousObject);
    }

    public void PushScope()
    {
        var snapshot = new Dictionary<string, object?>(_variables, _variables.Comparer);
        _scopeStack.Push(snapshot);
    }

    public void PopScope()
    {
        if (_scopeStack.Count == 0)
            throw new InvalidOperationException("No scope to pop.");

        var previousScope = _scopeStack.Pop();
        _variables.Clear();
        foreach (var kvp in previousScope)
        {
            _variables[kvp.Key] = kvp.Value;
        }
    }

    public void SetLoopVariable(string variableName, object? value)
    {
        _variables[variableName] = value;
    }
}
