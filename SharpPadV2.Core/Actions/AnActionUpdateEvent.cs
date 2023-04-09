namespace SharpPadV2.Core.Actions {
    public class AnActionUpdateEvent : AnActionEvent {
        public bool IsVisible { get; set; }

        public bool IsEnabled { get; set; }

        public AnActionUpdateEvent(object dataContext) : base(dataContext) {
            this.IsEnabled = true;
            this.IsVisible = true;
        }
    }
}