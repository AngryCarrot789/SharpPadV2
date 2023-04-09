using SharpPadV2.Core.Views.Dialogs.FilePicking;

namespace SharpPadV2.Core.Utils {
    public static class Filters {
        public static string NBTDatFilter = Filter.Of().
                                                   AddFilter("DAT File", "dat", "dat_old").
                                                   AddFilter("NBT File", "nbt").
                                                   AddFilter("Schematic", "schematic").
                                                   AddFilter("MCR", "dat_mcr").
                                                   AddFilter("BTP", "bpt").
                                                   AddFilter("RC", "rc").
                                                   ToString();
        public static string NBTDatAndAllFilesFilter = new Filter(NBTDatFilter).AddAllFiles().ToString();
    }
}