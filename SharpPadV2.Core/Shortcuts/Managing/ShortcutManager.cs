namespace SharpPadV2.Core.Shortcuts.Managing {
    public abstract class ShortcutManager {
        public ShortcutGroup Root { get; set; }

        public ShortcutManager() {
            this.Root = ShortcutGroup.CreateRoot();
        }

        public ShortcutGroup FindGroupByPath(string path) {
            return this.Root.GetGroupByPath(path);
        }

        public GroupedShortcut FindShortcutByPath(string path) {
            return this.Root.GetShortcutByPath(path);
        }

        public abstract ShortcutProcessor NewProcessor();
    }
}