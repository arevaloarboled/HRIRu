using System;
using System.Runtime.InteropServices;

namespace LibPDBinding.Native
{
	static class Defines
	{
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        public const string DllName = "pd";
#else
        public const string DllName = "libpdcsharp";
#endif
        public const CallingConvention CallingConvention = System.Runtime.InteropServices.CallingConvention.Cdecl;
	}
}