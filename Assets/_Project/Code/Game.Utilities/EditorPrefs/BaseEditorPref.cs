namespace Code.Game.Utilities.EditorPrefs
{
    public abstract class BaseEditorPref<T>
    {
        protected readonly string key;
        protected readonly T defaultValue;

        public T Value
        {
            get => GetValue();
            set => SetValue(value);
        }

        protected BaseEditorPref(string key, T defaultValue = default)
        {
            this.key = key;
            this.defaultValue = defaultValue;
        }

        protected abstract void SetValue(T value);

        protected abstract T GetValue();
    }
}