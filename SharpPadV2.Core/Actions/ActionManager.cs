using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SharpPadV2.Core.Actions {
    public class ActionManager {
        public static ActionManager Instance { get; }

        private readonly Dictionary<string, AnAction> actions;

        public ActionManager() {
            this.actions = new Dictionary<string, AnAction>();
        }

        static ActionManager() {
            Instance = new ActionManager();
        }

        public void Register(string id, AnAction action) {
            if (string.IsNullOrEmpty(id))
                throw new ArgumentException("Action cannot be null or empty", nameof(id));
            if (action == null)
                throw new ArgumentNullException(nameof(action));
            if (this.actions.TryGetValue(id, out AnAction existing))
                throw new Exception($"Action already registered with type '{id}': {existing.GetType()}");

            this.actions[id] = action;
        }

        public AnAction GetAction(string id) {
            return this.actions.TryGetValue(id, out AnAction action) ? action : null;
        }

        public async Task<bool> Execute(string id, object dataContext) {
            if (this.actions.TryGetValue(id, out AnAction action)) {
                return await action.Execute(new AnActionEvent(dataContext));
            }

            return false;
        }
    }
}