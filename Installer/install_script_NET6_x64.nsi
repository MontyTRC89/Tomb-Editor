!include LogicLib.nsh
!include MUI2.nsh
!include WinVer.nsh
!include x64.nsh

!cd "..\BuildRelease (x64)\net6.0-windows"

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_WELCOMEFINISHPAGE_BITMAP "..\..\TombEditor\Resources\misc\misc_InstallerSplashTEN.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "..\..\TombEditor\Resources\misc\misc_InstallerSplashTEN.bmp" 
!define MUI_ICON "..\..\Icons\ICO\TE.ico"
!define MUI_FINISHPAGE_SHOWREADME "Changes.txt"

!define DOT_MAJOR "6"
!define DOT_MINOR "0"

!define MUI_WELCOMEPAGE_TEXT \
"You are ready to install Tomb Editor ${Version_1}.${Version_2}.${Version_3}. $\r$\n\
$\r$\n\
Please make sure your system complies with following system requirements: $\r$\n\
$\r$\n\
  ${U+2022} Windows 7 or later (64-bit) $\r$\n\
  ${U+2022} Installed .NET 6 or later (64-bit)$\r$\n\
  ${U+2022} Videocard with DirectX 10 support $\r$\n\
  ${U+2022} At least 2 gigabytes of RAM $\r$\n\
$\r$\n\
This package includes a TIDE template to build Tomb Engine (TEN) levels. $\r$\n\
$\r$\n\
Enjoy! $\r$\n\
Tomb Editor dev team."

!getdllversion "TombEditor.exe" Version_

;--------------------------------

SetCompressor lzma
Unicode true
Name "Tomb Editor"
OutFile "TombEditorInstall.exe"
InstallDir "C:\Tomb Editor"
  
;--------------------------------

InstType "Standard"
InstType "Basic components only"

!insertmacro MUI_PAGE_WELCOME
!insertmacro MUI_PAGE_COMPONENTS
!insertmacro MUI_PAGE_DIRECTORY
!insertmacro MUI_PAGE_INSTFILES
!insertmacro MUI_PAGE_FINISH

!insertmacro MUI_UNPAGE_CONFIRM
!insertmacro MUI_UNPAGE_INSTFILES

!insertmacro MUI_LANGUAGE "English"

;--------------------------------

Section "Tomb Editor" Section1

  SectionIn RO ; Always install this section
  
  SetOutPath $INSTDIR
  File /r \
  /x "TombEditorLog*.txt" \
  /x "WadToolLog*.txt" \
  /x "TombIDELog*.txt" \
  /x "*.prj2" \
  /x "*.pdb" \
  /x "*.so" \
  /x "*.vshost.*" \
  /x "install_script.nsi" \
  /x "TombEditorInstall.exe" \
  /x "TombEditorConfiguration.xml" \
  /x "SoundToolConfiguration.xml" \
  /x "WadToolConfiguration.xml" \
  *.* \
  
  ; Add readme from installer folder
  File "..\..\Installer\Changes.txt"
  
  ; Add resources folder if not
  File /r "Resources"
  
  ; Choose 32-bit or 64-bit d3dcompiler dll based on system version
  Rename "$INSTDIR\Native\64 bit\d3dcompiler_43.dll" "$INSTDIR\d3dcompiler_43.dll"
  
  ; Write the uninstall keys
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "DisplayName" "Tomb Editor"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoRepair" 1
  
  ; Write uninstaller itself
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "Start Menu Shortcuts" Section2

  SectionIn 1 2

  CreateDirectory "$SMPROGRAMS\Tomb Editor"
  CreateShortcut "$SMPROGRAMS\Tomb Editor\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$SMPROGRAMS\Tomb Editor\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$SMPROGRAMS\Tomb Editor\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0

  ${If} ${SectionIsSelected} ${Section2}
	CreateShortcut "$SMPROGRAMS\Tomb Editor\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}
  
  CreateShortcut "$SMPROGRAMS\Tomb Editor\Uninstall.lnk" "$INSTDIR\uninstall.exe" "" "$INSTDIR\uninstall.exe" 0
  
SectionEnd

Section "Desktop Shortcuts" Section3

  SectionIn 1 2
  CreateShortcut "$DESKTOP\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$DESKTOP\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$DESKTOP\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0
  
  ${If} ${SectionIsSelected} ${Section2}
    CreateShortcut "$DESKTOP\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}

SectionEnd

Section "Associate File Types" Section4
  
  SectionIn 1 2
  
  Call .registerExtensions

SectionEnd

LangString DESC_Section1 ${LANG_ENGLISH} "Basic Tomb Editor components. Includes WadTool and SoundTool."
LangString DESC_Section2 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications in Start Menu."
LangString DESC_Section3 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications on Desktop."
LangString DESC_Section4 ${LANG_ENGLISH} "Associate file types with Tomb Editor, WadTool and TombIDE."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Section1} $(DESC_Section1)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section2} $(DESC_Section2)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section3} $(DESC_Section3)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section4} $(DESC_Section4)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  Call un.registerExtensions
  
  ; Autogenerated by Unlist script from barebones BuildRelease folder
  ; https://nsis.sourceforge.io/mediawiki/images/9/9f/Unlist.zip
  ; PLEASE UPDATE THIS BLOCK IF BUILD FILE SET IS CHANGED!
    
  Delete "$INSTDIR\TIDE\Templates\Shared\TR4-TRNG Shared Files.zip"
  Delete "$INSTDIR\TIDE\Templates\Shared\TR4-TEN Shared Audio.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TRNG.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR4.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR3.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR2.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TR1.zip"
  Delete "$INSTDIR\TIDE\Templates\Presets\TEN.zip"
  Delete "$INSTDIR\TIDE\Templates\Extras\FLEP.zip"
  Delete "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources\uklogo.pak"
  Delete "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources\load.bmp"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TRNG.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR4.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR3.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR2.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TR1.ico"
  Delete "$INSTDIR\TIDE\Templates\Defaults\Game Icons\TombEngine.ico"
  Delete "$INSTDIR\TIDE\NGC\VGE\tools\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\sound\samples\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\sound\LevelSFX Creator\sounds.txt"
  Delete "$INSTDIR\TIDE\NGC\VGE\Script\SCRIPT.TXT"
  Delete "$INSTDIR\TIDE\NGC\VGE\Script\ENGLISH.TXT"
  Delete "$INSTDIR\TIDE\NGC\VGE\pix\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\graphics\wads\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\data\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\audio\keep.me"
  Delete "$INSTDIR\TIDE\NGC\VGE\tomb4.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\Tomb_NextGeneration.dll"
  Delete "$INSTDIR\TIDE\NGC\VGE\SCRIPT.DAT"
  Delete "$INSTDIR\TIDE\NGC\VGE\Objects.h"
  Delete "$INSTDIR\TIDE\NGC\VGE\ngle.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\NG_Tom2Pc.exe"
  Delete "$INSTDIR\TIDE\NGC\VGE\ENGLISH.DAT"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Tomb4.exe"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Tomb_NextGeneration.dll"
  Delete "$INSTDIR\TIDE\NGC\TRNG\Objects.h"
  Delete "$INSTDIR\TIDE\NGC\TRNG\ngle.exe"
  Delete "$INSTDIR\TIDE\NGC\TRNG\ng_Tom2Pc.exe"
  Delete "$INSTDIR\TIDE\NGC\NG_Center.exe"
  Delete "$INSTDIR\TIDE\NGC\help_old_script_command.txt"
  Delete "$INSTDIR\TIDE\NGC\help_new_script_command.txt"
  Delete "$INSTDIR\TIDE\NGC\center_settings.bin"
  Delete "$INSTDIR\TIDE\NGC\~~!DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\NGC\~!~DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\NGC\!~~DO_NOT_USE_THIS_NG_CENTER.txt"
  Delete "$INSTDIR\TIDE\GFL\gameflow.exe"
  Delete "$INSTDIR\TIDE\DOS\TR4\SCRIPT.EXE"
  Delete "$INSTDIR\TIDE\DOS\TR4\DOS4GW.EXE"
  Delete "$INSTDIR\TIDE\DOS\SDL_net.dll"
  Delete "$INSTDIR\TIDE\DOS\SDL.dll"
  Delete "$INSTDIR\TIDE\DOS\DOSBox.exe"
  Delete "$INSTDIR\Runtimes\win-x86\native\assimp.dll"
  Delete "$INSTDIR\Runtimes\win-x64\native\assimp.dll"
  Delete "$INSTDIR\Runtimes\win\lib\netcoreapp2.0\System.Management.dll"
  Delete "$INSTDIR\Resources\Localization\PL\TombIDE.xml"
  Delete "$INSTDIR\Resources\Localization\EN\TombIDE.xml"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\OLD Commands.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\OCBs.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\NEW Commands.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\Descriptions\Mnemonic Constants.rdda"
  Delete "$INSTDIR\Resources\ClassicScript\VariablePlaceholders.xml"
  Delete "$INSTDIR\Resources\ClassicScript\StaticObjectIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\SoundIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\OldCommandsList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\OCBList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\NewCommandsList.xml"
  Delete "$INSTDIR\Resources\ClassicScript\MoveableSlotIndices.xml"
  Delete "$INSTDIR\Resources\ClassicScript\MnemonicConstants.xml"
  Delete "$INSTDIR\Resources\ClassicScript\KeyboardScancodes.xml"
  Delete "$INSTDIR\Resources\ClassicScript\EnemyDamageValues.xml"
  Delete "$INSTDIR\Resources\wt_file.ico"
  Delete "$INSTDIR\Resources\tide_file.ico"
  Delete "$INSTDIR\Resources\te_file.ico"
  Delete "$INSTDIR\Rendering\Legacy\Solid.fx"
  Delete "$INSTDIR\Rendering\Legacy\RoomGeometry.fx"
  Delete "$INSTDIR\Rendering\Legacy\Model.fx"
  Delete "$INSTDIR\Rendering\DirectX11\TextShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\TextShaderPS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderPS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\RoomShaderVS.cso"
  Delete "$INSTDIR\Rendering\DirectX11\RoomShaderPS.cso"
  Delete "$INSTDIR\Native\64 bit\d3dcompiler_43.dll"
  Delete "$INSTDIR\Native\32 bit\d3dcompiler_43.dll"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\VS15.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\Obsidian.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\NG_Center.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main\Monokai.t1msch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Default.xml"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\VS15.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\Obsidian.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\NG_Center.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript\Monokai.gflsch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\VS15.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Obsidian.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\NG_Center.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Monokai.cssch"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR5_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR2_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TR1_Override.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR5.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR4.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR3.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR2.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_TR1.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_DEFAULT_GLOBAL_HARDCODED.xml"
  Delete "$INSTDIR\Catalogs\TEN Sound Catalogs\TEN_ALL_SOUNDS.xml"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Variables.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Text.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Statics.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Rooms.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Moveables.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Lara.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Inventory.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Flow.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Effects.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Creatures.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\Controls.lua"
  Delete "$INSTDIR\Catalogs\TEN Node Catalogs\_System.lua"
  Delete "$INSTDIR\Catalogs\TrCatalog.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr5.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr4.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr3.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr3.gold.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr2.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr2.gold.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr1.xml"
  Delete "$INSTDIR\Catalogs\NgCatalog.xml"
  Delete "$INSTDIR\Assets\Wads\TombEngine.wad2"
  Delete "$INSTDIR\WadTool.runtimeconfig.json"
  Delete "$INSTDIR\WadTool.exe"
  Delete "$INSTDIR\WadTool.dll.config"
  Delete "$INSTDIR\WadTool.dll"
  Delete "$INSTDIR\WadTool.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.Tomb1Main.dll"
  Delete "$INSTDIR\TombLib.Scripting.Tomb1Main.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.Lua.dll"
  Delete "$INSTDIR\TombLib.Scripting.Lua.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.GameFlowScript.dll"
  Delete "$INSTDIR\TombLib.Scripting.GameFlowScript.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.dll"
  Delete "$INSTDIR\TombLib.Scripting.deps.json"
  Delete "$INSTDIR\TombLib.Scripting.ClassicScript.dll"
  Delete "$INSTDIR\TombLib.Scripting.ClassicScript.deps.json"
  Delete "$INSTDIR\TombLib.Rendering.dll"
  Delete "$INSTDIR\TombLib.Rendering.deps.json"
  Delete "$INSTDIR\TombLib.Forms.dll"
  Delete "$INSTDIR\TombLib.Forms.deps.json"
  Delete "$INSTDIR\TombLib.dll"
  Delete "$INSTDIR\TombLib.deps.json"
  Delete "$INSTDIR\TombIDE.Shared.dll"
  Delete "$INSTDIR\TombIDE.Shared.deps.json"
  Delete "$INSTDIR\TombIDE.ScriptingStudio.dll"
  Delete "$INSTDIR\TombIDE.ScriptingStudio.deps.json"
  Delete "$INSTDIR\TombIDE.runtimeconfig.json"
  Delete "$INSTDIR\TombIDE.ProjectMaster.dll"
  Delete "$INSTDIR\TombIDE.ProjectMaster.deps.json"
  Delete "$INSTDIR\TombIDE.exe"
  Delete "$INSTDIR\TombIDE.dll"
  Delete "$INSTDIR\TombIDE.deps.json"
  Delete "$INSTDIR\TombIDE Library Registration.runtimeconfig.json"
  Delete "$INSTDIR\TombIDE Library Registration.exe"
  Delete "$INSTDIR\TombIDE Library Registration.dll"
  Delete "$INSTDIR\TombIDE Library Registration.deps.json"
  Delete "$INSTDIR\TombEditor.runtimeconfig.json"
  Delete "$INSTDIR\TombEditor.exe"
  Delete "$INSTDIR\TombEditor.dll.config"
  Delete "$INSTDIR\TombEditor.dll"
  Delete "$INSTDIR\TombEditor.deps.json"
  Delete "$INSTDIR\System.Management.dll"
  Delete "$INSTDIR\SoundTool.runtimeconfig.json"
  Delete "$INSTDIR\SoundTool.exe"
  Delete "$INSTDIR\SoundTool.dll"
  Delete "$INSTDIR\SoundTool.deps.json"
  Delete "$INSTDIR\SharpDX.Toolkit.Graphics.dll"
  Delete "$INSTDIR\SharpDX.Toolkit.dll"
  Delete "$INSTDIR\SharpDX.Toolkit.Compiler.dll"
  Delete "$INSTDIR\SharpDX.DXGI.dll"
  Delete "$INSTDIR\SharpDX.dll"
  Delete "$INSTDIR\SharpDX.Direct3D11.Effects.dll"
  Delete "$INSTDIR\SharpDX.Direct3D11.dll"
  Delete "$INSTDIR\SharpDX.D3DCompiler.dll"
  Delete "$INSTDIR\Pfim.dll"
  Delete "$INSTDIR\NVorbis.dll"
  Delete "$INSTDIR\NLog.dll"
  Delete "$INSTDIR\NCalc.dll"
  Delete "$INSTDIR\NAudio.WinMM.dll"
  Delete "$INSTDIR\NAudio.WinForms.dll"
  Delete "$INSTDIR\NAudio.Wasapi.dll"
  Delete "$INSTDIR\NAudio.Vorbis.dll"
  Delete "$INSTDIR\NAudio.Midi.dll"
  Delete "$INSTDIR\NAudio.Flac.dll"
  Delete "$INSTDIR\NAudio.dll"
  Delete "$INSTDIR\NAudio.Core.dll"
  Delete "$INSTDIR\NAudio.Asio.dll"
  Delete "$INSTDIR\MiniZ64.dll"
  Delete "$INSTDIR\MiniZ32.dll"
  Delete "$INSTDIR\MiniZ.Net.dll"
  Delete "$INSTDIR\MiniFileAssociation.dll"
  Delete "$INSTDIR\Microsoft.NET.StringTools.dll"
  Delete "$INSTDIR\Microsoft.Build.Utilities.Core.dll"
  Delete "$INSTDIR\Microsoft.Build.Tasks.Core.dll"
  Delete "$INSTDIR\Microsoft.Build.Framework.dll"
  Delete "$INSTDIR\ICSharpCode.AvalonEdit.dll"
  Delete "$INSTDIR\IconUtilities.dll"
  Delete "$INSTDIR\FlaUI.UIA2.dll"
  Delete "$INSTDIR\FlaUI.Core.dll"
  Delete "$INSTDIR\File Association.runtimeconfig.json"
  Delete "$INSTDIR\File Association.exe"
  Delete "$INSTDIR\File Association.dll"
  Delete "$INSTDIR\File Association.deps.json"
  Delete "$INSTDIR\DarkUI.dll"
  Delete "$INSTDIR\DarkUI.deps.json"
  Delete "$INSTDIR\CustomTabControl.dll"
  Delete "$INSTDIR\ColorThief.Netstandard.v20.dll"
  Delete "$INSTDIR\CH.SipHash.dll"
  Delete "$INSTDIR\bzPSD.dll"
  Delete "$INSTDIR\AssimpNet.dll"
  RMDir "$INSTDIR\TIDE\Templates\Defaults\TR4 Resources"
  RMDir "$INSTDIR\TIDE\Templates\Defaults\Game Icons"
  RMDir "$INSTDIR\TIDE\Templates\Shared"
  RMDir "$INSTDIR\TIDE\Templates\Presets"
  RMDir "$INSTDIR\TIDE\Templates\Extras"
  RMDir "$INSTDIR\TIDE\Templates\Defaults"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound\samples"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound\LevelSFX Creator"
  RMDir "$INSTDIR\TIDE\NGC\VGE\graphics\wads"
  RMDir "$INSTDIR\TIDE\NGC\VGE\tools"
  RMDir "$INSTDIR\TIDE\NGC\VGE\sound"
  RMDir "$INSTDIR\TIDE\NGC\VGE\Script"
  RMDir "$INSTDIR\TIDE\NGC\VGE\pix"
  RMDir "$INSTDIR\TIDE\NGC\VGE\graphics"
  RMDir "$INSTDIR\TIDE\NGC\VGE\data"
  RMDir "$INSTDIR\TIDE\NGC\VGE\audio"
  RMDir "$INSTDIR\TIDE\NGC\VGE"
  RMDir "$INSTDIR\TIDE\NGC\TRNG"
  RMDir "$INSTDIR\TIDE\DOS\TR4"
  RMDir "$INSTDIR\TIDE\Templates"
  RMDir "$INSTDIR\TIDE\NGC"
  RMDir "$INSTDIR\TIDE\GFL"
  RMDir "$INSTDIR\TIDE\DOS"
  RMDir "$INSTDIR\Runtimes\win-x86\native"
  RMDir "$INSTDIR\Runtimes\win-x64\native"
  RMDir "$INSTDIR\Runtimes\win\lib\netcoreapp2.0"
  RMDir "$INSTDIR\Runtimes\win\lib"
  RMDir "$INSTDIR\Runtimes\win-x86"
  RMDir "$INSTDIR\Runtimes\win-x64"
  RMDir "$INSTDIR\Runtimes\win"
  RMDir "$INSTDIR\Resources\Localization\PL"
  RMDir "$INSTDIR\Resources\Localization\EN"
  RMDir "$INSTDIR\Resources\ClassicScript\Descriptions"
  RMDir "$INSTDIR\Resources\Localization"
  RMDir "$INSTDIR\Resources\ClassicScript"
  RMDir "$INSTDIR\Rendering\Legacy"
  RMDir "$INSTDIR\Rendering\DirectX11"
  RMDir "$INSTDIR\Native\64 bit"
  RMDir "$INSTDIR\Native\32 bit"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Tomb1Main"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\GameFlowScript"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes"
  RMDir "$INSTDIR\Configs\TextEditors"
  RMDir "$INSTDIR\Catalogs\TEN Sound Catalogs"
  RMDir "$INSTDIR\Catalogs\TEN Node Catalogs"
  RMDir "$INSTDIR\Assets\Wads"
  RMDir "$INSTDIR\TIDE"
  RMDir "$INSTDIR\Runtimes"
  RMDir "$INSTDIR\Resources"
  RMDir "$INSTDIR\Rendering"
  RMDir "$INSTDIR\Native"
  RMDir "$INSTDIR\Configs"
  RMDir "$INSTDIR\Catalogs"
  RMDir "$INSTDIR\Assets"

  ; End of autogenerated Unlist block.

  ; Remove logs
  Delete "$INSTDIR\TombEditorLog.txt"
  Delete "$INSTDIR\TombEditorLog*.txt"
  Delete "$INSTDIR\TombIDELog.txt"
  Delete "$INSTDIR\TombIDELog*.txt"
  Delete "$INSTDIR\SoundToolLog.txt"
  Delete "$INSTDIR\SoundToolLog*.txt"
  Delete "$INSTDIR\WadToolLog.txt"
  Delete "$INSTDIR\WadToolLog*.txt"
  
  ; Remove configs
  Delete "$INSTDIR\Configs\TombEditorConfiguration.xml"
  Delete "$INSTDIR\Configs\SoundToolConfiguration.xml"
  Delete "$INSTDIR\Configs\WadToolConfiguration.xml"
  Delete "$INSTDIR\Configs\TombIDE*.xml"
  RMDir /r "$INSTDIR\Configs"
  
  ; Remove TIDE stuff
  
  RMDir /r "$INSTDIR\TIDE"

  ; Remove readme
  Delete "$INSTDIR\Changes.txt"
  
  ; Remove dlls which were externally copied
  Delete "$INSTDIR\d3dcompiler_43.dll"
  
  ; Remove uninstaller
  Delete "$INSTDIR\uninstall.exe"
  
  ; Remove settings
  RMDir /r "$LOCALAPPDATA\TombEditor"
  RMDir /r "$LOCALAPPDATA\SoundTool"
  RMDir /r "$LOCALAPPDATA\WadTool"

  ; Remove registry keys
  DeleteRegKey HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor"
  
  ; Remove shortcuts, if any
  Delete "$DESKTOP\Tomb Editor.lnk"
  Delete "$DESKTOP\SoundTool.lnk"
  Delete "$DESKTOP\WadTool.lnk"  
  Delete "$DESKTOP\TombIDE.lnk"
  
  Delete "$SMPROGRAMS\Tomb Editor\*.*"
  RMDir "$SMPROGRAMS\Tomb Editor"

  ; Only remove program dir if it's empty
  SetOutPath $TEMP
  Push $INSTDIR
  Call un.isEmptyDir
  Pop $0
  StrCmp $0 1 0 +2
    RMDir /r $INSTDIR
  StrCmp $0 0 0 +2
    MessageBox MB_OK \
    "Installation folder contains extra files. $\r$\n\
    Check if these files are important and remove folder manually."
     
SectionEnd

;--------------------------------

; Helper functions

Function .onInit
  ${IfNot} ${AtLeastWin7}
    MessageBox MB_OK \
    "At least Windows 7 is required to use Tomb Editor. $\r$\n\
    The installer will now quit."
    Quit
  ${EndIf}
  
  ${IfNot} ${RunningX64}
    MessageBox MB_OK \
    "This version of Tomb Editor is only compatible with 64-bit systems. $\r$\n\
    The installer will now quit."
    Quit
  ${EndIf}
FunctionEnd

Function .onInstSuccess
  Call isDotNetInstalled
FunctionEnd

Function un.isEmptyDir
  Exch $0
  Push $1
  FindFirst $0 $1 "$0\*.*"
  strcmp $1 "." 0 _notempty
    FindNext $0 $1
    strcmp $1 ".." 0 _notempty
      ClearErrors
      FindNext $0 $1
      IfErrors 0 _notempty
        FindClose $0
        Pop $1
        StrCpy $0 1
        Exch $0
        goto _end
     _notempty:
       FindClose $0
       ClearErrors
       Pop $1
       StrCpy $0 0
       Exch $0
  _end:
FunctionEnd

Function .registerExtensions
  ExecShell "runas" "$INSTDIR\File Association.exe" '-111'    
FunctionEnd

Function un.registerExtensions
  
  ExecShell "runas" "$INSTDIR\File Association.exe" '-d'
  Sleep 1000
    
FunctionEnd

Function openLinkNewWindow
  Push $3
  Exch
  Push $2
  Exch
  Push $1
  Exch
  Push $0
  Exch
 
  ReadRegStr $0 HKCR "http\shell\open\command" ""
# Get browser path
    DetailPrint $0
  StrCpy $2 '"'
  StrCpy $1 $0 1
  StrCmp $1 $2 +2 # if path is not enclosed in " look for space as final char
    StrCpy $2 ' '
  StrCpy $3 1
  loop:
    StrCpy $1 $0 1 $3
    DetailPrint $1
    StrCmp $1 $2 found
    StrCmp $1 "" found
    IntOp $3 $3 + 1
    Goto loop
 
  found:
    StrCpy $1 $0 $3
    StrCmp $2 " " +2
      StrCpy $1 '$1"'
 
  Pop $0
  Exec '$1 $0'
  Pop $0
  Pop $1
  Pop $2
  Pop $3
FunctionEnd

; Usage
; Define in your script two constants:
;   DOT_MAJOR "(Major framework version)"
;   DOT_MINOR "{Minor framework version)"
; 
; Call isDotNetInstalled
; This function will abort the installation if the required version 
; or higher version of the .NET Framework is not installed.  Place it in
; either your .onInit function or your first install section before 
; other code.
Function isDotNetInstalled
 
  StrCpy $0 "0"
  StrCpy $1 "SOFTWARE\Microsoft\ASP.NET Core" ;registry entry to look in.
  StrCpy $2 0
 
  StartEnum:
    ;Enumerate the versions installed.
    EnumRegKey $3 HKLM "$1\Shared Framework" $2
 
    ;If we don't find any versions installed, it's not here.
    StrCmp $3 "" noDotNet notEmpty
 
    ;We found something.
    notEmpty:
      ;Find out if the RegKey starts with 'v'.  
      ;If it doesn't, goto the next key.
      StrCpy $4 $3 1 0
      StrCmp $4 "v" +1 goNext
      StrCpy $4 $3 1 1
 
      ;It starts with 'v'.  Now check to see how the installed major version
      ;relates to our required major version.
      ;If it's equal check the minor version, if it's greater, 
      ;we found a good RegKey.
      IntCmp $4 ${DOT_MAJOR} +1 goNext yesDotNet
      ;Check the minor version.  If it's equal or greater to our requested 
      ;version then we're good.
      StrCpy $4 $3 1 3
      IntCmp $4 ${DOT_MINOR} yesDotNet goNext yesDotNet
 
    goNext:
      ;Go to the next RegKey.
      IntOp $2 $2 + 1
      goto StartEnum
	  
  noDotNet:
	StrCpy $0 "You need .NET "
	StrCpy $0 "$0${DOT_MAJOR}.${DOT_MINOR}"
	StrCpy $0 "$0 installed. Do you want to download it now? $\r$\n\
	If you do, select $\"Run Desktop Apps > Download x64$\" version."
	
    ;Nope, something went wrong along the way.  Looks like the 
    ;proper .NET Framework isn't installed. Ask user to install it.
    MessageBox MB_YESNO $0 IDYES doInstall IDNO doNotInstall
	doInstall:
	  StrCpy $0 "https://dotnet.microsoft.com/en-us/download/dotnet/"
	  StrCpy $0 "$0${DOT_MAJOR}.${DOT_MINOR}"
	  StrCpy $0 "$0/runtime"
	  Push $0
	  Call openLinkNewWindow
	doNotInstall:
 
  yesDotNet:
    ;Everything checks out.  Go on with the rest of the installation.
 
FunctionEnd

