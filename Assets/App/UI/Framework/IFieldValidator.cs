using System;

namespace Miyo.UI
{
    public interface IFieldValidator<T> where T : IComparable<T>
    {
        bool Validate(T value);
    }


    public interface IStringFieldValidator : IFieldValidator<string>
    {
    }

    public interface IIntFieldValidator : IFieldValidator<int>
    {
    }

    public interface IFloatFieldValidator : IFieldValidator<float>
    {
    }   
}