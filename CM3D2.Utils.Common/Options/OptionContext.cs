// --------------------------------------------------
// CM3D2.Utils.Common - OptionContext.cs
// --------------------------------------------------

namespace CM3D2.Utils.Common.Options
{
    public class OptionContext
    {
        public Option Option { get; set; }

        public int OptionIndex { get; set; }

        public string OptionName { get; set; }

        public OptionSet OptionSet { get; }

        public OptionValueCollection OptionValues { get; }

        public OptionContext(OptionSet set)
        {
            OptionSet = set;
            OptionValues = new OptionValueCollection(this);
        }
    }
}
