using System;
using System.Collections.Generic;

namespace SharpPadV2.Core.Actions {
    public class DefaultActionGroup : ActionGroup {
        private readonly List<AnAction> actions;

        public DefaultActionGroup(List<AnAction> actions, Func<string> header, Func<string> description, Func<string> inputGestureText = null) : base(header, description, inputGestureText) {
            this.actions = actions;
        }

        public override List<AnAction> GetChildren() {
            return this.actions;
        }
    }
}