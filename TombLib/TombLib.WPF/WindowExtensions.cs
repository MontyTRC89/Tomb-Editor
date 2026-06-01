using MvvmDialogs;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Interop;
using IWinFormsWindow = System.Windows.Forms.IWin32Window;

namespace TombLib.WPF;

public static class WindowExtensions
{
	private sealed class Win32WindowWrapper : IWinFormsWindow
	{
		public IntPtr Handle { get; }

		public Win32WindowWrapper(IntPtr handle)
		{
			Handle = handle;
		}
	}

	/// <summary>
	/// Sets the owner of the specified WPF <see cref="Window"/> to the provided WinForms window.
	/// </summary>
	/// <remarks>
	/// This method establishes ownership between a WPF window and a WinForms window, enabling proper
	/// behaviour for modal dialogs and window activation. Ensure that both the WPF window and the WinForms window are valid
	/// and initialized before calling this method.
	/// </remarks>
	/// <param name="window">The WPF <see cref="Window"/> whose owner is being set.</param>
	/// <param name="owner">The WinForms window that will act as the owner.</param>
	/// <returns>A <see cref="WindowInteropHelper"/> instance that links the WPF window to the specified owner.</returns>
	public static WindowInteropHelper SetOwner(this Window window, IWinFormsWindow owner)
		=> new(window) { Owner = owner.Handle };

	/// <summary>
	/// Retrieves an <see cref="IWinFormsWindow"/> representation of the specified WPF <see cref="Window"/>.
	/// </summary>
	/// <remarks>
	/// This method exposes the underlying Win32 window handle (HWND) of a WPF window,
	/// allowing it to be used with APIs and components that require an <see cref="IWinFormsWindow"/>,
	/// such as WinForms dialogs. The handle is obtained via <see cref="WindowInteropHelper"/>.
	/// If the window handle has not yet been created, it will be initialized.
	/// </remarks>
	/// <param name="window">The WPF <see cref="Window"/> instance.</param>
	/// <returns>An <see cref="IWinFormsWindow"/> wrapper for the window's underlying handle.</returns>
	public static IWinFormsWindow GetWin32Window(this Window window)
	{
		var helper = new WindowInteropHelper(window);

		if (helper.Handle == IntPtr.Zero)
			helper.EnsureHandle();

		return new Win32WindowWrapper(helper.Handle);
	}

	/// <summary>
	/// Closes <paramref name="window"/> when its <see cref="IModalDialogViewModel.DialogResult"/>
	/// transitions to a value, mirroring the auto-close that <c>IDialogService.ShowDialog</c>
	/// gives to MvvmDialogs-hosted dialogs. Manually-constructed dialogs
	/// (<c>new XxxWindow { DataContext = vm }.ShowDialog()</c>) don't get that wiring;
	/// calling this in the Window ctor restores it.
	/// </summary>
	public static void HookModalAutoClose(this Window window)
	{
		PropertyChangedEventHandler? handler = null;
		IModalDialogViewModel? attached = null;

		void detach()
		{
			if (attached is INotifyPropertyChanged old && handler is not null)
				old.PropertyChanged -= handler;
			attached = null;
		}

		void attach(IModalDialogViewModel vm)
		{
			detach();
			attached = vm;
			if (vm is not INotifyPropertyChanged npc)
				return;

			handler = (_, args) =>
			{
				if (args.PropertyName != nameof(IModalDialogViewModel.DialogResult) || vm.DialogResult is null)
					return;

				// For modal dialogs propagate to Window.DialogResult so ShowDialog() returns;
				// modeless windows raise InvalidOperationException on that setter, fall back to Close().
				if (window.IsLoaded)
				{
					try { window.DialogResult = vm.DialogResult; }
					catch (InvalidOperationException) { window.Close(); }
				}
				else
					window.Close();
			};
			npc.PropertyChanged += handler;
		}

		if (window.DataContext is IModalDialogViewModel initial)
			attach(initial);

		window.DataContextChanged += (_, args) =>
		{
			if (args.NewValue is IModalDialogViewModel vm)
				attach(vm);
		};

		window.Closed += (_, _) => detach();
	}
}
