﻿namespace FacadeAccountCreation.Core.Attributes;

public class NotDefaultAttribute() : ValidationAttribute(DefaultErrorMessage)
{
    private const string DefaultErrorMessage = "The {0} field must not have the default value";

    public override bool IsValid(object? value)
    {
        //NotDefault doesn't necessarily mean required
        if (value is null)
        {
            return true;
        }

        var type = value.GetType();
        if (type.IsValueType)
        {
            var defaultValue = Activator.CreateInstance(type);
            return !value.Equals(defaultValue);
        }

        // non-null ref type
        return true;
    }
}