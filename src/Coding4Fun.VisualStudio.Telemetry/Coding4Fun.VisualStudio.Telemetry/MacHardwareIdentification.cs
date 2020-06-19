using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry
{
	internal static class MacHardwareIdentification
	{
		private enum CFStringEncoding : uint
		{
			kCFStringEncodingASCII = 1536u
		}

		private struct CFRange
		{
			public IntPtr location;

			public IntPtr length;
		}

		private const string CoreFoundationLibrary = "/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation";

		private static readonly IntPtr CoreFoundationLibraryHandle = dlopen("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation", 0);

		private static readonly IntPtr kCFTypeDictionaryKeyCallBacks = dlsym(CoreFoundationLibraryHandle, "kCFTypeDictionaryKeyCallBacks");

		private static readonly IntPtr kCFTypeDictionaryValueCallBacks = dlsym(CoreFoundationLibraryHandle, "kCFTypeDictionaryValueCallBacks");

		private static readonly IntPtr kCFBooleanTrue = Marshal.ReadIntPtr(dlsym(CoreFoundationLibraryHandle, "kCFBooleanTrue"));

		private static readonly IntPtr kCFAllocatorDefault = Marshal.ReadIntPtr(dlsym(CoreFoundationLibraryHandle, "kCFAllocatorDefault"));

		private const int KERN_SUCCESS = 0;

		private const string IOKitLibrary = "/System/Library/Frameworks/IOKit.framework/IOKit";

		private static readonly IntPtr IOKitLibraryHandle = dlopen("/System/Library/Frameworks/IOKit.framework/IOKit", 0);

		private const string kIOServicePlane = "IOService";

		private const string kIOEthernetInterface = "IOEthernetInterface";

		private const string kIOPrimaryInterface = "IOPrimaryInterface";

		private const string kIOPropertyMatchKey = "IOPropertyMatch";

		private const string kIOMACAddress = "IOMACAddress";

		private static readonly IntPtr kIOMasterPortDefault = Marshal.ReadIntPtr(dlsym(IOKitLibraryHandle, "kIOMasterPortDefault"));

		public static bool TryGetFirstPrimaryMacAddress(out string macAddress)
		{
			IntPtr interfaceIterator = default(IntPtr);
			if (!TryFindPrimaryEthernetInterfaces(interfaceIterator))
			{
				macAddress = null;
				return false;
			}
			return TryGetFirstPrimaryMacAddress(interfaceIterator, out macAddress);
		}

		private static bool TryGetFirstPrimaryMacAddress(IntPtr interfaceIterator, out string macAddress)
		{
			macAddress = null;
			bool flag = false;
			IntPtr intPtr;
			while (!flag && (intPtr = IOIteratorNext(interfaceIterator)) != IntPtr.Zero)
			{
				if (IORegistryEntryGetParentEntry(intPtr, "IOService", out IntPtr parent) != 0)
				{
					return false;
				}
				IntPtr intPtr2 = CFStringCreateWithCString(kCFAllocatorDefault, "IOMACAddress", CFStringEncoding.kCFStringEncodingASCII);
				IntPtr intPtr3 = IORegistryEntryCreateCFProperty(parent, intPtr2, kCFAllocatorDefault, 0u);
				if (intPtr3 != IntPtr.Zero)
				{
					byte[] array = new byte[6];
					CFDataGetBytes(intPtr3, new CFRange
					{
						length = new IntPtr(array.Length)
					}, array);
					macAddress = $"{array[0]:x2}:{array[1]:x2}:{array[2]:x2}:{array[3]:x2}:{array[4]:x2}:{array[5]:x2}";
					flag = true;
					CFRelease(intPtr3);
				}
				CFRelease(intPtr2);
				IOObjectRelease(parent);
				IOObjectRelease(intPtr);
			}
			return flag;
		}

		private static bool TryFindPrimaryEthernetInterfaces(IntPtr interfaceIterator)
		{
			IntPtr intPtr = IOServiceMatching("IOEthernetInterface");
			if (intPtr == IntPtr.Zero)
			{
				return false;
			}
			IntPtr intPtr2 = CFDictionaryCreateMutable(IntPtr.Zero, IntPtr.Zero, kCFTypeDictionaryKeyCallBacks, kCFTypeDictionaryValueCallBacks);
			if (intPtr == IntPtr.Zero)
			{
				return false;
			}
			IntPtr intPtr3 = default(IntPtr);
			IntPtr intPtr4 = default(IntPtr);
			try
			{
				intPtr3 = CFStringCreateWithCString(kCFAllocatorDefault, "IOPrimaryInterface", CFStringEncoding.kCFStringEncodingASCII);
				if (intPtr3 == IntPtr.Zero)
				{
					return false;
				}
				CFDictionarySetValue(intPtr2, intPtr3, kCFBooleanTrue);
				intPtr4 = CFStringCreateWithCString(kCFAllocatorDefault, "IOPropertyMatch", CFStringEncoding.kCFStringEncodingASCII);
				if (intPtr4 == IntPtr.Zero)
				{
					return false;
				}
				CFDictionarySetValue(intPtr, intPtr4, intPtr2);
				CFRelease(intPtr2);
			}
			finally
			{
				CFRelease(intPtr3);
				CFRelease(intPtr4);
			}
			return IOServiceGetMatchingServices(kIOMasterPortDefault, intPtr, ref interfaceIterator) == 0;
		}

		[DllImport("libc")]
		private static extern IntPtr dlopen(string path, int mode);

		[DllImport("libc")]
		private static extern IntPtr dlsym(IntPtr handle, string symbol);

		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern void CFRelease(IntPtr obj);

		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern IntPtr CFStringCreateWithCString(IntPtr alloc, [MarshalAs(UnmanagedType.LPStr)] string str, CFStringEncoding encoding);

		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern IntPtr CFDictionaryCreateMutable(IntPtr allocator, IntPtr capacity, IntPtr keyCallBacks, IntPtr valueCallBacks);

		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern void CFDictionarySetValue(IntPtr theDict, IntPtr key, IntPtr value);

		[DllImport("/System/Library/Frameworks/CoreFoundation.framework/CoreFoundation")]
		private static extern void CFDataGetBytes(IntPtr theData, CFRange range, byte[] buffer);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern IntPtr IOServiceMatching([MarshalAs(UnmanagedType.LPStr)] string serviceName);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern int IOServiceGetMatchingServices(IntPtr masterPort, IntPtr matching, ref IntPtr existing);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern void IOObjectRelease(IntPtr obj);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern IntPtr IOIteratorNext(IntPtr iter);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern int IORegistryEntryGetParentEntry(IntPtr entry, [MarshalAs(UnmanagedType.LPStr)] string plane, out IntPtr parent);

		[DllImport("/System/Library/Frameworks/IOKit.framework/IOKit")]
		private static extern IntPtr IORegistryEntryCreateCFProperty(IntPtr entry, IntPtr key, IntPtr allocator, uint options);
	}
}
