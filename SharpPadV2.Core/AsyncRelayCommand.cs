using System;
using System.Threading.Tasks;

namespace SharpPadV2.Core {
    /// <summary>
    /// A simple relay command, which does not take any parameters
    /// </summary>
    public class AsyncRelayCommand : BaseAsyncRelayCommand {
        private readonly Func<Task> execute;
        private readonly Func<bool> canExecute;

        public AsyncRelayCommand(Func<Task> execute, Func<bool> canExecute = null) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
        }

        public override bool CanExecute(object parameter) {
            return base.CanExecute(parameter) && (this.canExecute == null || this.canExecute());
        }

        protected override async Task ExecuteAsyncOverride(object parameter) {
            await this.execute();
        }
    }

    public class AsyncRelayCommand<T> : BaseAsyncRelayCommand {
        private readonly Func<T, Task> execute;
        private readonly Func<T, bool> canExecute;

        public bool ConvertParameter { get; set; }

        public AsyncRelayCommand(Func<T, Task> execute, Func<T, bool> canExecute = null, bool convertParameter = false) {
            if (execute == null) {
                throw new ArgumentNullException(nameof(execute), "Execute callback cannot be null");
            }

            this.execute = execute;
            this.canExecute = canExecute;
            this.ConvertParameter = convertParameter;
        }

        public override bool CanExecute(object parameter) {
            if (base.CanExecute(parameter)) {
                if (this.ConvertParameter) {
                    parameter = GetConvertedParameter<T>(parameter);
                }

                return (parameter == null || parameter is T) && this.canExecute((T) parameter);
            }

            return false;
        }

        protected override async Task ExecuteAsyncOverride(object parameter) {
            if (this.ConvertParameter) {
                parameter = GetConvertedParameter<T>(parameter);
            }

            if (parameter is T t) {
                await this.execute(t);
            }
        }
    }
}