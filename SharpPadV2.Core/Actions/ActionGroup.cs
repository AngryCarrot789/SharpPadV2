using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpPadV2.Core.Actions {
    public abstract class ActionGroup : AnAction {
        protected ActionGroup(Func<string> header, Func<string> description, Func<string> inputGestureText = null) : base(header, description, inputGestureText) {

        }

        public abstract List<AnAction> GetChildren();

        public override Task<bool> Execute(AnActionEvent e) {
            return Task.FromResult(true);
        }
    }
}