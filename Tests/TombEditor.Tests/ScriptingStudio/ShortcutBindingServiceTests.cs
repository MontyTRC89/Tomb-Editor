using System.Collections.Generic;
using System.Windows.Input;
using TombIDE.ScriptingStudio.Shortcuts;
using TombIDE.ScriptingStudio.UI;

namespace TombEditor.Tests.ScriptingStudio;

[TestClass]
public class ShortcutBindingServiceTests
{
    private static StudioCommandCatalog CreateCatalog()
    {
        return new StudioCommandCatalog([
            new StudioCommandDescriptor(UICommand.NewFile, nameof(UICommand.NewFile), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.N, ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.Save, nameof(UICommand.Save), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.S, ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.SaveAll, nameof(UICommand.SaveAll), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.S, ModifierKeys.Control | ModifierKeys.Shift)),
            new StudioCommandDescriptor(UICommand.Build, nameof(UICommand.Build), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.F9, ModifierKeys.None)),
            new StudioCommandDescriptor(UICommand.Exit, nameof(UICommand.Exit), isRemappable: false, isHostReserved: true,
                new ShortcutKey(Key.F4, ModifierKeys.Alt)),
            new StudioCommandDescriptor(UICommand.Undo, nameof(UICommand.Undo), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.Z, ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.Redo, nameof(UICommand.Redo), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.Y, ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.Find, nameof(UICommand.Find), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.F, ModifierKeys.Control),
                new ShortcutKey(Key.H, ModifierKeys.Control)),
            new StudioCommandDescriptor(UICommand.GoToDefinition, nameof(UICommand.GoToDefinition), isRemappable: true, isHostReserved: false,
                new ShortcutKey(Key.F12, ModifierKeys.None))
        ]);
    }

    private static IShortcutBindingService CreateService()
        => new ShortcutBindingService(CreateCatalog(), new ShortcutOverrideCollection(), _ => true);

    [TestMethod]
    public void TryGetCommand_KnownShortcut_ReturnsCorrectCommand()
    {
        var service = CreateService();

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.S, ModifierKeys.Control), out UICommand saveCommand));
        Assert.AreEqual(UICommand.Save, saveCommand);

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.Z, ModifierKeys.Control), out UICommand undoCommand));
        Assert.AreEqual(UICommand.Undo, undoCommand);

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.F9, ModifierKeys.None), out UICommand buildCommand));
        Assert.AreEqual(UICommand.Build, buildCommand);

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.F12, ModifierKeys.None), out UICommand gotoDefCommand));
        Assert.AreEqual(UICommand.GoToDefinition, gotoDefCommand);

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.F4, ModifierKeys.Alt), out UICommand exitCommand));
        Assert.AreEqual(UICommand.Exit, exitCommand);
    }

    [TestMethod]
    public void TryGetCommand_UnknownShortcut_ReturnsFalse()
    {
        var service = CreateService();

        Assert.IsFalse(service.TryGetCommand(new ShortcutKey(Key.A, ModifierKeys.None), out _));
        Assert.IsFalse(service.TryGetCommand(new ShortcutKey(Key.X, ModifierKeys.Control | ModifierKeys.Shift | ModifierKeys.Alt), out _));
    }

    [TestMethod]
    public void GetBindings_Save_ReturnsCtrlS()
    {
        var service = CreateService();

        IReadOnlyList<ShortcutKey> bindings = service.GetBindings(UICommand.Save);

        Assert.AreEqual(1, bindings.Count);
        Assert.AreEqual(new ShortcutKey(Key.S, ModifierKeys.Control), bindings[0]);
    }

    [TestMethod]
    public void GetBindings_Find_ReturnsTwoBindings()
    {
        var service = CreateService();

        IReadOnlyList<ShortcutKey> bindings = service.GetBindings(UICommand.Find);

        Assert.AreEqual(2, bindings.Count);
        Assert.IsTrue(bindings.Contains(new ShortcutKey(Key.F, ModifierKeys.Control)));
        Assert.IsTrue(bindings.Contains(new ShortcutKey(Key.H, ModifierKeys.Control)));
    }

    [TestMethod]
    public void TryGetCommand_Find_AlsoRespondsToSecondaryBindingCtrlH()
    {
        var service = CreateService();

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.F, ModifierKeys.Control), out UICommand primaryCommand));
        Assert.AreEqual(UICommand.Find, primaryCommand);

        Assert.IsTrue(service.TryGetCommand(new ShortcutKey(Key.H, ModifierKeys.Control), out UICommand secondaryCommand));
        Assert.AreEqual(UICommand.Find, secondaryCommand);
    }

    [TestMethod]
    public void GetBindings_None_ReturnsEmpty()
    {
        var service = CreateService();

        IReadOnlyList<ShortcutKey> bindings = service.GetBindings(UICommand.None);

        Assert.AreEqual(0, bindings.Count);
    }

    [TestMethod]
    public void GetDisplayText_KnownCommand_ReturnsNonEmptyText()
    {
        var service = CreateService();

        string displayText = service.GetDisplayText(UICommand.Save);

        Assert.IsFalse(string.IsNullOrEmpty(displayText));
    }

    [TestMethod]
    public void GetDisplayText_Find_ShowsBothBindings()
    {
        var service = CreateService();

        string displayText = service.GetDisplayText(UICommand.Find);

        Assert.IsTrue(displayText.Contains('/'));
    }

    [TestMethod]
    public void GetDisplayText_UnknownCommand_ReturnsFallback()
    {
        var service = CreateService();

        string displayText = service.GetDisplayText(UICommand.None, "Fallback");

        Assert.AreEqual("Fallback", displayText);
    }

    [TestMethod]
    public void Validate_HostReserved_ReturnsReserved()
    {
        var service = CreateService();

        ShortcutValidationResult result = service.Validate(UICommand.Exit, [new ShortcutKey(Key.X, ModifierKeys.Control)]);

        Assert.AreEqual(ShortcutValidationResult.Reserved, result);
    }

    [TestMethod]
    public void Reset_RemovesOverrideAndRestoresDefault()
    {
        var overrides = new ShortcutOverrideCollection();
        overrides.Overrides.Add(new ShortcutOverrideEntry
        {
            CommandId = nameof(UICommand.Save),
            Bindings = [new ShortcutBindingSettings { KeyName = "X", Modifiers = (int)ModifierKeys.Control }]
        });

        bool saved = false;
        var service = new ShortcutBindingService(CreateCatalog(), overrides, o => { saved = true; return true; });

        // Initially, Save should be Ctrl+X (override).
        IReadOnlyList<ShortcutKey> bindingsBefore = service.GetBindings(UICommand.Save);
        Assert.AreEqual(1, bindingsBefore.Count);
        Assert.AreEqual(new ShortcutKey(Key.X, ModifierKeys.Control), bindingsBefore[0]);

        // Reset.
        service.Reset(UICommand.Save);

        // After reset, Save should be back to Ctrl+S (default).
        IReadOnlyList<ShortcutKey> bindingsAfter = service.GetBindings(UICommand.Save);
        Assert.AreEqual(1, bindingsAfter.Count);
        Assert.AreEqual(new ShortcutKey(Key.S, ModifierKeys.Control), bindingsAfter[0]);
        Assert.IsTrue(saved);
    }

    [TestMethod]
    public void ResetAll_RemovesAllOverrides()
    {
        var overrides = new ShortcutOverrideCollection();
        overrides.Overrides.Add(new ShortcutOverrideEntry
        {
            CommandId = nameof(UICommand.Save),
            Bindings = [new ShortcutBindingSettings { KeyName = "X", Modifiers = (int)ModifierKeys.Control }]
        });

        bool saved = false;
        var service = new ShortcutBindingService(CreateCatalog(), overrides, o => { saved = true; return true; });

        service.ResetAll();

        // After reset all, Save should be back to Ctrl+S (default).
        IReadOnlyList<ShortcutKey> bindings = service.GetBindings(UICommand.Save);
        Assert.AreEqual(1, bindings.Count);
        Assert.AreEqual(new ShortcutKey(Key.S, ModifierKeys.Control), bindings[0]);
        Assert.IsTrue(saved);
    }

    [TestMethod]
    public void BindingsChanged_FiresOnReset()
    {
        var service = CreateService();
        bool fired = false;
        service.BindingsChanged += (_, _) => fired = true;

        service.Reset(UICommand.Save);

        Assert.IsTrue(fired);
    }

    [TestMethod]
    public void ShortcutKey_GetDisplayText_IncludesModifiers()
    {
        var shortcut = new ShortcutKey(Key.S, ModifierKeys.Control | ModifierKeys.Shift);

        string displayText = shortcut.GetDisplayText();

        Assert.IsTrue(displayText.Contains("Ctrl+"));
        Assert.IsTrue(displayText.Contains("Shift+"));
        Assert.IsTrue(displayText.Contains("S"));
    }
}
