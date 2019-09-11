using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace TombIDE
{
	public class Registration
	{
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi, ExactSpelling = true, SetLastError = true)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern IntPtr LoadLibrary(string lpFileName);

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool FreeLibrary(IntPtr hModule);

		private delegate uint PointerToMethod();

		public static void RegisterCom(string comDll)
		{
			IntPtr hLib = IntPtr.Zero;

			try
			{
				hLib = LoadComDll(comDll);
				CallPointerMethod(hLib, "DllRegisterServer");
				FreeComDll(hLib);
			}
			catch
			{
				if (hLib != IntPtr.Zero)
					FreeComDll(hLib);
			}
		}

		public static void UnregisterCom(string comDll)
		{
			IntPtr hLib = IntPtr.Zero;

			try
			{
				hLib = LoadComDll(comDll);
				CallPointerMethod(hLib, "DllUnregisterServer");
				FreeComDll(hLib);
			}
			catch
			{
				if (hLib != IntPtr.Zero)
					FreeComDll(hLib);
			}
		}

		private static IntPtr LoadComDll(string comDll)
		{
			IntPtr hLib = LoadLibrary(comDll);

			if (IntPtr.Zero == hLib)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			return hLib;
		}

		private static void CallPointerMethod(IntPtr hLib, string methodName)
		{
			IntPtr dllEntryPoint = GetProcAddress(hLib, methodName);

			if (dllEntryPoint == IntPtr.Zero)
				throw new Win32Exception(Marshal.GetLastWin32Error());

			PointerToMethod functionDelegate = (PointerToMethod)Marshal.GetDelegateForFunctionPointer(dllEntryPoint, typeof(PointerToMethod));
			uint result = functionDelegate();

			if (result != 0)
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}

		private static void FreeComDll(IntPtr hLib)
		{
			if (!FreeLibrary(hLib))
				throw new Win32Exception(Marshal.GetLastWin32Error());
		}
	}
}
