﻿using System;
using System.Runtime.InteropServices;

namespace uk.JohnCook.dotnet.NAudioWrapperLibrary.EarTrumpet.Interop
{
    static class NativeMethods
    {
        [DllImport("combase.dll", PreserveSig = false, BestFitMapping = false, CharSet = CharSet.Ansi, ThrowOnUnmappableChar = true)]
        public static extern void RoGetActivationFactory(
            [MarshalAs(UnmanagedType.HString)] string activatableClassId,
            [In] ref Guid iid,
            [Out, MarshalAs(UnmanagedType.IInspectable)] out Object factory);

        [DllImport("combase.dll", PreserveSig = false)]
        public static extern void WindowsCreateString(
            [MarshalAs(UnmanagedType.LPWStr)] string src,
            [In] uint length,
            [Out] out IntPtr hstring);
    }
}
