using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.Native.Mac
{
	/// <summary>
	/// Native Mac runtime methods for accessing NSxxx methods and objects
	/// Ported in part from: https://github.com/xamarin/xamarin-macios
	/// </summary>
	internal static class MacRuntime
	{
		internal static class Messaging
		{
			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern void Void_objc_msgSend(IntPtr receiver, IntPtr selector);

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend(IntPtr receiver, IntPtr selector);

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend_IntPtr_ref_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1, ref IntPtr arg2);

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern IntPtr IntPtr_objc_msgSend_IntPtr(IntPtr receiver, IntPtr selector, IntPtr arg1);

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern ulong UInt64_objc_msgSend(IntPtr receiver, IntPtr selector);

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			public static extern double Double_objc_msgSend(IntPtr receiver, IntPtr selector);
		}

		internal static class Selector
		{
			private const string Alloc = "alloc";

			internal static IntPtr AllocHandle = GetHandle("alloc");

			private const string Release = "release";

			internal static IntPtr ReleaseHandle = GetHandle("release");

			private const string Autorelease = "autorelease";

			internal static IntPtr AutoreleaseHandle = GetHandle("autorelease");

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "sel_registerName")]
			public static extern IntPtr GetHandle(string name);
		}

		internal static class Class
		{
			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_getClass")]
			private static extern IntPtr GetObjcClass(string name);

			public static IntPtr GetHandle(string name)
			{
				return GetObjcClass(name);
			}
		}

		public const string LIBOBJC_DYLIB = "/usr/lib/libobjc.dylib";
	}
}
