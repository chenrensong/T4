using System;
using System.Runtime.InteropServices;

namespace Coding4Fun.VisualStudio.Telemetry.Native.Mac
{
	/// <summary>
	/// Native Mac runtime methods for CoreGraphics and other classes
	/// Ported in part from: https://github.com/xamarin/xamarin-macios
	/// </summary>
	internal static class MacFoundation
	{
		public static class NSBundle
		{
			private static readonly IntPtr class_ptr = MacRuntime.Class.GetHandle("NSBundle");

			private const string selMainBundle = "mainBundle";

			private static readonly IntPtr selMainBundleHandle = MacRuntime.Selector.GetHandle("mainBundle");

			private const string selInfoDictionary = "infoDictionary";

			private static readonly IntPtr selInfoDictionaryHandle = MacRuntime.Selector.GetHandle("infoDictionary");

			public static string GetVersion()
			{
				IntPtr mainBundle = GetMainBundle();
				if (mainBundle == IntPtr.Zero)
				{
					return null;
				}
				IntPtr infoDictionary = GetInfoDictionary(mainBundle);
				if (infoDictionary == IntPtr.Zero)
				{
					return null;
				}
				IntPtr intPtr = NSString.CreateNative("CFBundleShortVersionString", false);
				IntPtr usrhandle = NSDictionary.ObjectForKey(infoDictionary, intPtr);
				NSString.ReleaseNative(intPtr);
				return NSString.FromHandle(usrhandle);
			}

			public static string GetBundleName()
			{
				IntPtr mainBundle = GetMainBundle();
				if (mainBundle == IntPtr.Zero)
				{
					return null;
				}
				IntPtr infoDictionary = GetInfoDictionary(mainBundle);
				if (infoDictionary == IntPtr.Zero)
				{
					return null;
				}
				IntPtr intPtr = NSString.CreateNative("CFBundleName", false);
				IntPtr usrhandle = NSDictionary.ObjectForKey(infoDictionary, intPtr);
				NSString.ReleaseNative(intPtr);
				return NSString.FromHandle(usrhandle);
			}

			private static IntPtr GetMainBundle()
			{
				return MacRuntime.Messaging.IntPtr_objc_msgSend(class_ptr, selMainBundleHandle);
			}

			private static IntPtr GetInfoDictionary(IntPtr bundle)
			{
				return MacRuntime.Messaging.IntPtr_objc_msgSend(bundle, selInfoDictionaryHandle);
			}
		}

		public static class CoreGraphics
		{
			public struct DisplayInformation
			{
				public uint DisplayCount;

				public uint MainDisplayHeight;

				public uint MainDisplayWidth;
			}

			private const string CoreGraphicsLib = "/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics";

			[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
			private static extern uint CGMainDisplayID();

			[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
			private static extern uint CGDisplayPixelsHigh(uint id);

			[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
			private static extern uint CGDisplayPixelsWide(uint id);

			[DllImport("/System/Library/Frameworks/CoreGraphics.framework/CoreGraphics")]
			private static extern int CGGetActiveDisplayList(uint max, IntPtr activeDisplays, IntPtr displayCount);

			public static void GetDisplayInfo(DisplayInformation info)
			{
				uint id = CGMainDisplayID();
				info.MainDisplayHeight = CGDisplayPixelsHigh(id);
				info.MainDisplayWidth = CGDisplayPixelsWide(id);
				IntPtr intPtr = Marshal.AllocHGlobal(4);
				try
				{
					Marshal.WriteInt32(intPtr, 0);
					if (CGGetActiveDisplayList(0u, IntPtr.Zero, intPtr) == 0)
					{
						info.DisplayCount = (uint)Marshal.ReadInt32(intPtr);
					}
				}
				finally
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}

		public static class NSFileManager
		{
			public class NSFileSystemAttributes
			{
				public ulong Size
				{
					get;
					internal set;
				}

				public ulong FreeSize
				{
					get;
					internal set;
				}

				internal NSFileSystemAttributes()
				{
				}

				internal static NSFileSystemAttributes FromDictionary(IntPtr dict)
				{
					if (dict == IntPtr.Zero)
					{
						return null;
					}
					NSFileSystemAttributes nSFileSystemAttributes = new NSFileSystemAttributes();
					IntPtr intPtr = NSString.CreateNative("NSFileSystemSize", false);
					nSFileSystemAttributes.Size = (Fetch_ulong(dict, intPtr) ?? 0);
					NSString.ReleaseNative(intPtr);
					intPtr = NSString.CreateNative("NSFileSystemFreeSize", false);
					nSFileSystemAttributes.FreeSize = (Fetch_ulong(dict, intPtr) ?? 0);
					NSString.ReleaseNative(intPtr);
					return nSFileSystemAttributes;
				}

				internal static ulong? Fetch_ulong(IntPtr dict, IntPtr key)
				{
					IntPtr intPtr = NSDictionary.ObjectForKey(dict, key);
					return (intPtr == IntPtr.Zero) ? null : new ulong?(NSNumber.UInt64Value(intPtr));
				}
			}

			private static readonly IntPtr class_ptr = MacRuntime.Class.GetHandle("NSFileManager");

			private static readonly IntPtr selDefaultManagerHandle = MacRuntime.Selector.GetHandle("defaultManager");

			private static readonly IntPtr selAttributesOfFileSystemForPath_Error_Handle = MacRuntime.Selector.GetHandle("attributesOfFileSystemForPath:error:");

			public static NSFileSystemAttributes GetFileSystemAttributesForRoot()
			{
				IntPtr fileSystemAttributes = GetFileSystemAttributes(GetDefaultFileManager(), "/");
				NSFileSystemAttributes result = null;
				if (fileSystemAttributes != IntPtr.Zero)
				{
					result = NSFileSystemAttributes.FromDictionary(fileSystemAttributes);
				}
				return result;
			}

			private static IntPtr GetDefaultFileManager()
			{
				return MacRuntime.Messaging.IntPtr_objc_msgSend(class_ptr, selDefaultManagerHandle);
			}

			private static IntPtr GetFileSystemAttributes(IntPtr nsFileManagerHandle, string path, out IntPtr error)
			{
				if (nsFileManagerHandle == IntPtr.Zero)
				{
					throw new ArgumentNullException("nsFileManagerHandle");
				}
				if (path == null)
				{
					throw new ArgumentNullException("path");
				}
				error = IntPtr.Zero;
				IntPtr intPtr = NSString.CreateNative(path, false);
				IntPtr result = MacRuntime.Messaging.IntPtr_objc_msgSend_IntPtr_ref_IntPtr(nsFileManagerHandle, selAttributesOfFileSystemForPath_Error_Handle, intPtr, ref error);
				NSString.ReleaseNative(intPtr);
				return result;
			}

			private static IntPtr GetFileSystemAttributes(IntPtr nsFileManagerHandle, string path)
			{
				IntPtr error = IntPtr.Zero;
				IntPtr fileSystemAttributes = GetFileSystemAttributes(nsFileManagerHandle, path, out error);
				if (!(error == IntPtr.Zero))
				{
					return IntPtr.Zero;
				}
				return fileSystemAttributes;
			}
		}

		private static class NSNumber
		{
			private const string selUnsignedLongLongValue = "unsignedLongLongValue";

			private static readonly IntPtr selUnsignedLongLongValueHandle = MacRuntime.Selector.GetHandle("unsignedLongLongValue");

			public static ulong UInt64Value(IntPtr handle)
			{
				return MacRuntime.Messaging.UInt64_objc_msgSend(handle, selUnsignedLongLongValueHandle);
			}
		}

		private static class NSDictionary
		{
			private const string selObjectForKey_ = "objectForKey:";

			private static readonly IntPtr selObjectForKey_Handle = MacRuntime.Selector.GetHandle("objectForKey:");

			public static IntPtr ObjectForKey(IntPtr dict, IntPtr key)
			{
				if (dict == IntPtr.Zero)
				{
					throw new ArgumentNullException("dict");
				}
				if (key == IntPtr.Zero)
				{
					throw new ArgumentNullException("key");
				}
				return MacRuntime.Messaging.IntPtr_objc_msgSend_IntPtr(dict, selObjectForKey_Handle, key);
			}
		}

		private static class NSObject
		{
			internal static void DangerousAutorelease(IntPtr handle)
			{
				MacRuntime.Messaging.Void_objc_msgSend(handle, MacRuntime.Selector.AutoreleaseHandle);
			}

			internal static void DangerousRelease(IntPtr handle)
			{
				if (!(handle == IntPtr.Zero))
				{
					MacRuntime.Messaging.Void_objc_msgSend(handle, MacRuntime.Selector.ReleaseHandle);
				}
			}
		}

		private static class NSString
		{
			private static readonly IntPtr class_ptr = MacRuntime.Class.GetHandle("NSString");

			private const string selInitWithCharactersLength = "initWithCharacters:length:";

			private static IntPtr selInitWithCharactersLengthHandle = MacRuntime.Selector.GetHandle("initWithCharacters:length:");

			private const string selUTF8String = "UTF8String";

			private static IntPtr selUTF8StringHandle = MacRuntime.Selector.GetHandle("UTF8String");

			[DllImport("/usr/lib/libobjc.dylib", EntryPoint = "objc_msgSend")]
			private static extern IntPtr IntPtr_objc_msgSend_IntPtr_nint(IntPtr receiver, IntPtr selector, IntPtr arg1, long arg2);

			public static IntPtr CreateNative(string str, bool autorelease)
			{
				if (str == null)
				{
					return IntPtr.Zero;
				}
				return CreateWithCharacters(MacRuntime.Messaging.IntPtr_objc_msgSend(class_ptr, MacRuntime.Selector.AllocHandle), str, autorelease);
			}

			public static void ReleaseNative(IntPtr handle)
			{
				NSObject.DangerousRelease(handle);
			}

			public static string FromHandle(IntPtr usrhandle)
			{
				if (usrhandle == IntPtr.Zero)
				{
					return null;
				}
				return Marshal.PtrToStringAuto(MacRuntime.Messaging.IntPtr_objc_msgSend(usrhandle, selUTF8StringHandle));
			}

			private unsafe static IntPtr CreateWithCharacters(IntPtr handle, string str, bool autorelease = false)
			{
				fixed (char* value = str)
				{
					handle = IntPtr_objc_msgSend_IntPtr_nint(handle, selInitWithCharactersLengthHandle, (IntPtr)(void*)value, str.Length);
					if (autorelease)
					{
						NSObject.DangerousAutorelease(handle);
					}
					return handle;
				}
			}
		}
	}
}
