using System;

namespace Sitecore.Courier
{
    [Flags]
    public enum AllowedOperations
    {
        None = 0,
        Create = 1,
        Delete = 2,
        Update= 4
    }
}