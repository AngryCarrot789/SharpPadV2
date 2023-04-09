using System;
using System.Threading.Tasks;

namespace SharpPadV2.Core.Actions {
    public abstract class AnAction {
        private static readonly Func<string> ProvideNullString = () => null;

        public Func<string> Header { get; }

        public Func<string> Description { get; }

        public Func<string> InputGestureText { get; }

        protected AnAction(Func<string> header, Func<string> description, Func<string> inputGestureText = null) {
            this.Header = header ?? ProvideNullString;
            this.Description = description ?? ProvideNullString;
            this.InputGestureText = inputGestureText ?? ProvideNullString;
        }

        public static AnAction Lambda(Func<AnActionEvent, Task<bool>> action, Func<string> header = null, Func<string> description = null, Func<string> inputGestureText = null) {
            return new LambdaAction(header, description, inputGestureText, action);
        }

        public abstract Task<bool> Execute(AnActionEvent e);

        private class LambdaAction : AnAction {
            public Func<AnActionEvent, Task<bool>> MyAction { get; }

            public LambdaAction(Func<string> header, Func<string> description, Func<string> inputGestureText, Func<AnActionEvent, Task<bool>> action) : base(header, description, inputGestureText) {
                this.MyAction = action ?? throw new ArgumentNullException(nameof(action), "Action function cannot be null");
            }

            public override Task<bool> Execute(AnActionEvent e) {
                return this.MyAction(e);
            }
        }
    }
}