!include LogicLib.nsh
!include MUI2.nsh
!include WinVer.nsh
!include x64.nsh

!cd "..\BuildRelease"

!define MUI_COMPONENTSPAGE_SMALLDESC
!define MUI_ABORTWARNING
!define MUI_FINISHPAGE_SHOWREADME_NOTCHECKED
!define MUI_WELCOMEFINISHPAGE_BITMAP "..\TombEditor\Resources\misc\misc_InstallerSplash.bmp"
!define MUI_UNWELCOMEFINISHPAGE_BITMAP "..\TombEditor\Resources\misc\misc_InstallerSplash.bmp" 
!define MUI_ICON "..\TombEditor\Resources\Misc\tomb_editor.ico"
!define MUI_FINISHPAGE_SHOWREADME "Changes.txt"

!getdllversion "TombEditor.exe" Version_

!define MUI_WELCOMEPAGE_TEXT \
"You are ready to install Tomb Editor ${Version_1}.${Version_2}.${Version_3}. $\r$\n\
$\r$\n\
Please make sure your system complies with following system requirements: $\r$\n\
$\r$\n\
  ${U+2022} Windows 7 or later (preferably 64-bit) $\r$\n\
  ${U+2022} Installed .NET Framework 4.5 $\r$\n\
  ${U+2022} Videocard with DirectX 10 support $\r$\n\
  ${U+2022} At least 2 gigabytes of RAM $\r$\n\
$\r$\n\
Enjoy! $\r$\n\
Tomb Editor dev team."

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
  /x "TIDE" \
  /x "Configs" \
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
  /x "TombIDE*.*" \
  *.* \
  
  ; Add readme from installer folder
  File "..\Installer\Changes.txt"
  
  ; Choose 32-bit or 64-bit d3dcompiler dll based on system version
  ${If} ${RunningX64}
      Rename "$INSTDIR\Native\64 bit\d3dcompiler_43.dll" "$INSTDIR\d3dcompiler_43.dll"
  ${Else}
      Rename "$INSTDIR\Native\32 bit\d3dcompiler_43.dll" "$INSTDIR\d3dcompiler_43.dll"
  ${EndIf}
  
  RMDir /r "$INSTDIR\Native"
  
  ; Beautify assimp runtime folder
  Rename "$INSTDIR\runtimes" "$INSTDIR\Runtimes"
  
  ; Write the uninstall keys
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "DisplayName" "Tomb Editor"
  WriteRegStr HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "UninstallString" '"$INSTDIR\uninstall.exe"'
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoModify" 1
  WriteRegDWORD HKLM "Software\Microsoft\Windows\CurrentVersion\Uninstall\TombEditor" "NoRepair" 1
  
  ; Write uninstaller itself
  WriteUninstaller "uninstall.exe"
  
SectionEnd

Section "TombIDE" Section2

  SectionIn 1
  
  SetOutPath $INSTDIR
  File /r \
  /x "*.pdb" \
  /x "*.config" \
  /x "*.vshost.*" \
  /x "TombIDE*.xml" \
  TombIDE*.* 
  
  File /r "TIDE"
  File /r "Configs"
  
SectionEnd

Section "Start Menu Shortcuts" Section3

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

Section "Desktop Shortcuts" Section4

  SectionIn 1 2
  CreateShortcut "$DESKTOP\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$DESKTOP\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$DESKTOP\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0
  
  ${If} ${SectionIsSelected} ${Section2}
    CreateShortcut "$DESKTOP\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}

SectionEnd

LangString DESC_Section1 ${LANG_ENGLISH} "Basic Tomb Editor components. Includes WadTool and SoundTool."
LangString DESC_Section2 ${LANG_ENGLISH} "TombIDE. Also makes TombEditor not dependent on TRNG installation."
LangString DESC_Section3 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications in Start Menu."
LangString DESC_Section4 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications on Desktop."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Section1} $(DESC_Section1)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section2} $(DESC_Section2)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section3} $(DESC_Section3)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section4} $(DESC_Section4)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  ; Autogenerated by Unlist script from barebones BuildRelease folder
  ; https://nsis.sourceforge.io/mediawiki/images/9/9f/Unlist.zip
  ; PLEASE UPDATE THIS BLOCK IF BUILD FILE SET IS CHANGED!
  
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Defaults\uklogo.pak"
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Defaults\TRNG.ico"
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Defaults\TR4.ico"
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Defaults\load.bmp"
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Defaults\FLEP.ico"
	Delete "$INSTDIR\TIDE\Templates\TOMB4\Shared.zip"
	Delete "$INSTDIR\TIDE\Templates\Engines\TRNG.zip"
	Delete "$INSTDIR\TIDE\Templates\Engines\TRNG + FLEP.zip"
	Delete "$INSTDIR\TIDE\Templates\Engines\TR4.zip"
	Delete "$INSTDIR\TIDE\References\Descriptions\OLD Commands.rdda"
	Delete "$INSTDIR\TIDE\References\Descriptions\OCBs.rdda"
	Delete "$INSTDIR\TIDE\References\Descriptions\NEW Commands.rdda"
	Delete "$INSTDIR\TIDE\References\Descriptions\Mnemonic Constants.rdda"
	Delete "$INSTDIR\TIDE\References\Variable Placeholders.xml"
	Delete "$INSTDIR\TIDE\References\Static Object Indices.xml"
	Delete "$INSTDIR\TIDE\References\Sound SFX Indices.xml"
	Delete "$INSTDIR\TIDE\References\OLD Commands List.xml"
	Delete "$INSTDIR\TIDE\References\OCB List.xml"
	Delete "$INSTDIR\TIDE\References\NEW Commands List.xml"
	Delete "$INSTDIR\TIDE\References\Moveable Slot Indices.xml"
	Delete "$INSTDIR\TIDE\References\Mnemonic Constants.xml"
	Delete "$INSTDIR\TIDE\References\Keyboard Scancodes.xml"
	Delete "$INSTDIR\TIDE\References\Enemy Damage List.xml"
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
	Delete "$INSTDIR\runtimes\win-x86\native\assimp.dll"
	Delete "$INSTDIR\runtimes\win-x64\native\assimp.dll"
	Delete "$INSTDIR\runtimes\osx-x64\native\libassimp.dylib"
	Delete "$INSTDIR\runtimes\linux-x64\native\libassimp.so"
	Delete "$INSTDIR\Rendering\DirectX11\TextShaderVS.cso"
	Delete "$INSTDIR\Rendering\DirectX11\TextShaderPS.cso"
	Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderVS.cso"
	Delete "$INSTDIR\Rendering\DirectX11\SpriteShaderPS.cso"
	Delete "$INSTDIR\Rendering\DirectX11\RoomShaderVS.cso"
	Delete "$INSTDIR\Rendering\DirectX11\RoomShaderPS.cso"
	Delete "$INSTDIR\Editor\Shaders\StaticModel.fx"
	Delete "$INSTDIR\Editor\Shaders\Solid.fx"
	Delete "$INSTDIR\Editor\Shaders\RoomGeometry.fx"
	Delete "$INSTDIR\Editor\Shaders\Model.fx"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\VS15.luasch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Obsidian.luasch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Monokai.luasch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\VS15.cssch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Obsidian.cssch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\NG_Center.cssch"
	Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Monokai.cssch"
	Delete "$INSTDIR\Catalogs\TrCatalog.xml"
	Delete "$INSTDIR\Catalogs\Sounds.tr2.xml"
	Delete "$INSTDIR\Catalogs\Sounds.tr3.xml"
	Delete "$INSTDIR\Catalogs\Sounds.tr4.xml"
	Delete "$INSTDIR\Catalogs\NgCatalog.xml"
	Delete "$INSTDIR\WadTool.exe.config"
	Delete "$INSTDIR\WadTool.exe"
	Delete "$INSTDIR\TombLib.Scripting.TextEditors.pdb"
	Delete "$INSTDIR\TombLib.Scripting.TextEditors.dll"
	Delete "$INSTDIR\TombLib.Scripting.pdb"
	Delete "$INSTDIR\TombLib.Scripting.dll"
	Delete "$INSTDIR\TombLib.Rendering.dll"
	Delete "$INSTDIR\TombLib.Forms.dll"
	Delete "$INSTDIR\TombLib.dll"
	Delete "$INSTDIR\TombIDE.Shared.pdb"
	Delete "$INSTDIR\TombIDE.Shared.dll"
	Delete "$INSTDIR\TombIDE.ScriptEditor.pdb"
	Delete "$INSTDIR\TombIDE.ScriptEditor.dll"
	Delete "$INSTDIR\TombIDE.ProjectMaster.pdb"
	Delete "$INSTDIR\TombIDE.ProjectMaster.dll"
	Delete "$INSTDIR\TombIDE.pdb"
	Delete "$INSTDIR\TombIDE.exe.config"
	Delete "$INSTDIR\TombIDE.exe"
	Delete "$INSTDIR\TombIDE Library Registration.pdb"
	Delete "$INSTDIR\TombIDE Library Registration.exe.config"
	Delete "$INSTDIR\TombIDE Library Registration.exe"
	Delete "$INSTDIR\TombEditor.exe.config"
	Delete "$INSTDIR\TombEditor.exe"
	Delete "$INSTDIR\TestStack.White.xml"
	Delete "$INSTDIR\TestStack.White.pdb"
	Delete "$INSTDIR\TestStack.White.dll"
	Delete "$INSTDIR\System.Numerics.Vectors.dll"
	Delete "$INSTDIR\System.Drawing.PSD.dll"
	Delete "$INSTDIR\SoundTool.exe.config"
	Delete "$INSTDIR\SoundTool.exe"
	Delete "$INSTDIR\SharpDX.Toolkit.Graphics.dll"
	Delete "$INSTDIR\SharpDX.Toolkit.dll"
	Delete "$INSTDIR\SharpDX.Toolkit.Compiler.dll"
	Delete "$INSTDIR\SharpDX.DXGI.dll"
	Delete "$INSTDIR\SharpDX.dll"
	Delete "$INSTDIR\SharpDX.Direct3D11.Effects.dll"
	Delete "$INSTDIR\SharpDX.Direct3D11.dll"
	Delete "$INSTDIR\SharpDX.D3DCompiler.dll"
	Delete "$INSTDIR\SharpCompress.dll"
	Delete "$INSTDIR\Pfim.dll"
	Delete "$INSTDIR\NVorbis.dll"
	Delete "$INSTDIR\NLog.dll"
	Delete "$INSTDIR\NAudio.Vorbis.dll"
	Delete "$INSTDIR\NAudio.Flac.dll"
	Delete "$INSTDIR\NAudio.dll"
	Delete "$INSTDIR\MiniZ64.dll"
	Delete "$INSTDIR\MiniZ32.dll"
	Delete "$INSTDIR\MiniZ.Net.dll"
	Delete "$INSTDIR\ICSharpCode.AvalonEdit.dll"
	Delete "$INSTDIR\DarkUI.pdb"
	Delete "$INSTDIR\DarkUI.dll"
	Delete "$INSTDIR\CustomTabControl.dll"
	Delete "$INSTDIR\ColorThief.Desktop.v45.dll"
	Delete "$INSTDIR\CH.SipHash.dll"
	Delete "$INSTDIR\Castle.Core.xml"
	Delete "$INSTDIR\Castle.Core.dll"
	Delete "$INSTDIR\AssimpNet.dll"
	RMDir "$INSTDIR\TIDE\Templates\TOMB4\Defaults"
	RMDir "$INSTDIR\TIDE\Templates\TOMB4"
	RMDir "$INSTDIR\TIDE\Templates\Engines"
	RMDir "$INSTDIR\TIDE\References\Descriptions"
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
	RMDir "$INSTDIR\TIDE\Templates"
	RMDir "$INSTDIR\TIDE\References"
	RMDir "$INSTDIR\TIDE\NGC"
	RMDir "$INSTDIR\runtimes\win-x86\native"
	RMDir "$INSTDIR\runtimes\win-x64\native"
	RMDir "$INSTDIR\runtimes\osx-x64\native"
	RMDir "$INSTDIR\runtimes\linux-x64\native"
	RMDir "$INSTDIR\runtimes\win-x86"
	RMDir "$INSTDIR\runtimes\win-x64"
	RMDir "$INSTDIR\runtimes\osx-x64"
	RMDir "$INSTDIR\runtimes\linux-x64"
	RMDir "$INSTDIR\Rendering\DirectX11"
	RMDir "$INSTDIR\Editor\Shaders"
	RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua"
	RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript"
	RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes"
	RMDir "$INSTDIR\Configs\TextEditors"
	RMDir "$INSTDIR\TIDE"
	RMDir "$INSTDIR\runtimes"
	RMDir "$INSTDIR\Rendering"
	RMDir "$INSTDIR\Editor"
	RMDir "$INSTDIR\Configs"
	RMDir "$INSTDIR\Catalogs"

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
  Delete "$INSTDIR\TombEditorConfiguration.xml"
  Delete "$INSTDIR\SoundToolConfiguration.xml"
  Delete "$INSTDIR\WadToolConfiguration.xml"
  Delete "$INSTDIR\TombIDE*.xml"
  RMDir /r "$INSTDIR\Configs"

  ; Remove readme
  Delete "$INSTDIR\Changes.txt"
  
  ; Remove dlls which were externally copied
  Delete "$INSTDIR\d3dcompiler_43.dll"
  
  ; Remove uninstaller
  Delete "$INSTDIR\uninstall.exe"
  
  ; Remove settings
  RMDir /r "$LOCALAPPDATA\TombEditor"

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