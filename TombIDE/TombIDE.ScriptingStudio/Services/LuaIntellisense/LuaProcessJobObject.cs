#nullable enable

using NLog;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace TombIDE.ScriptingStudio.Services.LuaIntellisense;

/// <summary>
/// Wraps a Windows job object configured with <c>JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE</c> so that any
/// child processes assigned to it are forcibly terminated when the host (TombIDE) crashes or the
/// last handle to the job is released. This prevents stranded <c>lua-language-server.exe</c>
/// processes if the editor never gets a chance to run its disposal path.
/// </summary>
[SupportedOSPlatform("windows")]
internal static class LuaProcessJobObject
{
	private static readonly Logger Log = LogManager.GetCurrentClassLogger();
	private static readonly object SyncRoot = new();
	private static IntPtr _jobHandle = IntPtr.Zero;
	private static bool _initializationFailed;

	public static void TryAssignProcess(Process process)
	{
		if (process is null)
			return;

		if (!OperatingSystem.IsWindows())
			return;

		IntPtr jobHandle = EnsureJobHandle();

		if (jobHandle == IntPtr.Zero)
			return;

		try
		{
			if (!AssignProcessToJobObject(jobHandle, process.Handle))
			{
				int errorCode = Marshal.GetLastWin32Error();

				// 5 = ERROR_ACCESS_DENIED. Pre-Windows 8 hosts (which TombIDE no longer ships for)
				// or processes already inside an unbreakable job will return this; we just log and move on.
				Log.Debug("AssignProcessToJobObject failed with Win32 error {ErrorCode} for the Lua language server process.", errorCode);
			}
		}
		catch (Exception exception)
		{
			Log.Debug(exception, "Failed to assign the Lua language server process to the kill-on-close job object.");
		}
	}

	private static IntPtr EnsureJobHandle()
	{
		if (_jobHandle != IntPtr.Zero)
			return _jobHandle;

		if (_initializationFailed)
			return IntPtr.Zero;

		lock (SyncRoot)
		{
			if (_jobHandle != IntPtr.Zero)
				return _jobHandle;

			if (_initializationFailed)
				return IntPtr.Zero;

			IntPtr handle = CreateJobObject(IntPtr.Zero, lpName: null);

			if (handle == IntPtr.Zero)
			{
				_initializationFailed = true;
				Log.Debug("CreateJobObject returned NULL (Win32 error {ErrorCode}); the Lua language server will rely on graceful shutdown.", Marshal.GetLastWin32Error());
				return IntPtr.Zero;
			}

			JobObjectExtendedLimitInformation extendedLimit = default;
			extendedLimit.BasicLimitInformation.LimitFlags = JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE;

			int payloadSize = Marshal.SizeOf<JobObjectExtendedLimitInformation>();
			IntPtr payloadPointer = Marshal.AllocHGlobal(payloadSize);

			try
			{
				Marshal.StructureToPtr(extendedLimit, payloadPointer, fDeleteOld: false);

				if (!SetInformationJobObject(handle, JobObjectInformationClass.ExtendedLimitInformation, payloadPointer, (uint)payloadSize))
				{
					int errorCode = Marshal.GetLastWin32Error();
					CloseHandle(handle);
					_initializationFailed = true;
					Log.Debug("SetInformationJobObject failed with Win32 error {ErrorCode}; the Lua language server will rely on graceful shutdown.", errorCode);
					return IntPtr.Zero;
				}
			}
			finally
			{
				Marshal.FreeHGlobal(payloadPointer);
			}

			_jobHandle = handle;
			AppDomain.CurrentDomain.ProcessExit += (_, _) => CloseHandle(handle);
			return handle;
		}
	}

	private const uint JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE = 0x2000;

	private enum JobObjectInformationClass
	{
		ExtendedLimitInformation = 9
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct IoCounters
	{
		public ulong ReadOperationCount;
		public ulong WriteOperationCount;
		public ulong OtherOperationCount;
		public ulong ReadTransferCount;
		public ulong WriteTransferCount;
		public ulong OtherTransferCount;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct JobObjectBasicLimitInformation
	{
		public long PerProcessUserTimeLimit;
		public long PerJobUserTimeLimit;
		public uint LimitFlags;
		public UIntPtr MinimumWorkingSetSize;
		public UIntPtr MaximumWorkingSetSize;
		public uint ActiveProcessLimit;
		public UIntPtr Affinity;
		public uint PriorityClass;
		public uint SchedulingClass;
	}

	[StructLayout(LayoutKind.Sequential)]
	private struct JobObjectExtendedLimitInformation
	{
		public JobObjectBasicLimitInformation BasicLimitInformation;
		public IoCounters IoInfo;
		public UIntPtr ProcessMemoryLimit;
		public UIntPtr JobMemoryLimit;
		public UIntPtr PeakProcessMemoryUsed;
		public UIntPtr PeakJobMemoryUsed;
	}

	[DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	private static extern IntPtr CreateJobObject(IntPtr lpJobAttributes, string? lpName);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool SetInformationJobObject(IntPtr hJob, JobObjectInformationClass infoType, IntPtr lpJobObjectInfo, uint cbJobObjectInfoLength);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool AssignProcessToJobObject(IntPtr hJob, IntPtr hProcess);

	[DllImport("kernel32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool CloseHandle(IntPtr hObject);
}
