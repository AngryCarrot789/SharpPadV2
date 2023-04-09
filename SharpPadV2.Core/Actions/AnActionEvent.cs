namespace SharpPadV2.Core.Actions {
    public class AnActionEvent {
        public object DataContext { get; }

        public AnActionEvent(object dataContext) {
            this.DataContext = dataContext;
        }

        public T GetContext<T>() {
            return this.DataContext is T t ? t : default;
        }

        public bool TryGetContext<T>(out T value) {
            if (this.DataContext is T t) {
                value = t;
                return true;
            }

            value = default;
            return false;
        }
    }
}