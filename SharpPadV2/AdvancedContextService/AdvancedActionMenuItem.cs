using System.Threading;
using System.Windows;
using System.Windows.Threading;
using SharpPadV2.Core.Actions;
using SharpPadV2.Core.AdvancedContextService;
using SharpPadV2.Core.AdvancedContextService.Actions;
using SharpPadV2.Core.AdvancedContextService.Commands;

namespace SharpPadV2.AdvancedContextService {
    public class AdvancedActionMenuItem : AdvancedMenuItem {
        public static readonly DependencyProperty ActionIdProperty = DependencyProperty.Register("ActionId", typeof(string), typeof(AdvancedActionMenuItem), new PropertyMetadata(null));
        public static readonly DependencyProperty InvokeActionAfterCommandProperty = DependencyProperty.Register("InvokeActionAfterCommand", typeof(bool), typeof(AdvancedActionMenuItem), new PropertyMetadata(default(bool)));

        public string ActionId {
            get => (string) this.GetValue(ActionIdProperty);
            set => this.SetValue(ActionIdProperty, value);
        }

        public bool InvokeActionAfterCommand {
            get => (bool) this.GetValue(InvokeActionAfterCommandProperty);
            set => this.SetValue(InvokeActionAfterCommandProperty, value);
        }

        private volatile int isExecuting;

        public AdvancedActionMenuItem() {

        }

        protected override void OnClick() {
            object contex = this.DataContext;
            // context should not be an instance of CommandContextEntry... but just in case
            if (contex is CommandContextEntry || contex is ActionContextEntry) {
                base.OnClick(); // clicking is handled in the entry
                return;
            }

            if (Interlocked.CompareExchange(ref this.isExecuting, 1, 0) == 1) {
                return;
            }

            string id = this.ActionId;
            if (string.IsNullOrEmpty(id)) {
                base.OnClick();
                return;
            }

            if (this.InvokeActionAfterCommand) {
                base.OnClick();
                this.DispatchAction(id);
            }
            else {
                this.DispatchAction(id);
                base.OnClick();
            }
        }

        protected virtual void DispatchAction(string id) {
            IHasDataContext context;
            if (this.DataContext is IHasDataContext) {
                context = (IHasDataContext) this.DataContext;
            }
            else if (this is IHasDataContext) {
                context = (IHasDataContext) this;
            }
            else {
                context = new DefaultDataContext();
                context.AddContext(this.DataContext);
            }

            if (this.IsCheckable) {
                context.SetCustomData("IsChecked", this.IsChecked);
            }

            this.Dispatcher.InvokeAsync(async () => {
                try {
                    await ActionManager.Instance.Execute(id, context);
                }
                finally {
                    this.isExecuting = 0;
                }
            }, DispatcherPriority.Render);
        }
    }
}