// --------------------------------------------------
// CM3D2.Utils.Common - OptionAction.cs
// --------------------------------------------------

namespace CM3D2.Utils.Common.Options
{
    public delegate void OptionAction<TKey, TValue>(TKey key, TValue value);
}
