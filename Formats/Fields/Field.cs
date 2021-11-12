using System;

namespace pkuManager.Formats.Fields
{
    public abstract class Field<T>
    {
        protected Func<T, T> CustomGetter;

        protected Func<T, T> CustomSetter;

        protected Field(Func<T, T> getter = null, Func<T, T> setter = null)
        {
            CustomGetter = getter;
            CustomSetter = setter;
        }

        public static implicit operator T(Field<T> f) => f.Get();

        public T Get()
            => CustomGetter is null ? GetRaw() : CustomGetter(GetRaw());

        public void Set(T val)
        {
            if (CustomSetter is null)
                SetRaw(val);
            else
                SetRaw(CustomSetter(val));
        }

        protected abstract T GetRaw();

        protected abstract void SetRaw(T val);
    }
}
