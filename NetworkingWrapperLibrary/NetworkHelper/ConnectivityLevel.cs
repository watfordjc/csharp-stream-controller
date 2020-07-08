using System;
using System.Collections.Generic;
using System.Text;

namespace NetworkingWrapperLibrary
{
    //
    // Summary:
    //     Defines the level of connectivity currently available.
    public enum NetworkConnectivityLevel2
    {
        //
        // Summary:
        //     No connectivity.
        None = 0,
        //
        // Summary:
        //     Local network access only.
        LocalAccess = 1,
        //
        // Summary:
        //     Limited internet access.
        ConstrainedInternetAccess = 2,
        //
        // Summary:
        //     Local and Internet access.
        InternetAccess = 3
    }
}
