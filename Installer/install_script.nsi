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
!define MUI_ICON "..\Icons\ICO\TE.ico"
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
Var tideInstalled

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
  /x "Assets" \
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
  
  StrCpy $tideInstalled "yes"
  
SectionEnd

Section "Stock Assets" Section3

  SectionIn 1

  SetOutPath $INSTDIR
  File /r "Assets"
  
SectionEnd

Section "Start Menu Shortcuts" Section4

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

Section "Desktop Shortcuts" Section5

  SectionIn 1 2
  CreateShortcut "$DESKTOP\Tomb Editor.lnk" "$INSTDIR\TombEditor.exe" "" "$INSTDIR\TombEditor.exe" 0
  CreateShortcut "$DESKTOP\SoundTool.lnk" "$INSTDIR\SoundTool.exe" "" "$INSTDIR\SoundTool.exe" 0
  CreateShortcut "$DESKTOP\WadTool.lnk" "$INSTDIR\WadTool.exe" "" "$INSTDIR\WadTool.exe" 0
  
  ${If} ${SectionIsSelected} ${Section2}
    CreateShortcut "$DESKTOP\TombIDE.lnk" "$INSTDIR\TombIDE.exe" "" "$INSTDIR\TombIDE.exe" 0
  ${EndIf}

SectionEnd

Section "Associate File Types" Section6
  
  SectionIn 1 2
  
  Call .registerExtensions

SectionEnd

LangString DESC_Section1 ${LANG_ENGLISH} "Basic Tomb Editor components. Includes WadTool and SoundTool."
LangString DESC_Section2 ${LANG_ENGLISH} "TombIDE. Also makes TombEditor not dependent on TRNG installation."
LangString DESC_Section3 ${LANG_ENGLISH} "Stock sound assets for TR1. Needed if you plan to build TR1 levels."
LangString DESC_Section4 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications in Start Menu."
LangString DESC_Section5 ${LANG_ENGLISH} "Shortcuts for Tomb Editor applications on Desktop."
LangString DESC_Section6 ${LANG_ENGLISH} "Associate file types with Tomb Editor, WadTool and TombIDE."

!insertmacro MUI_FUNCTION_DESCRIPTION_BEGIN
  !insertmacro MUI_DESCRIPTION_TEXT ${Section1} $(DESC_Section1)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section2} $(DESC_Section2)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section3} $(DESC_Section3)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section4} $(DESC_Section4)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section5} $(DESC_Section5)
  !insertmacro MUI_DESCRIPTION_TEXT ${Section6} $(DESC_Section6)
!insertmacro MUI_FUNCTION_DESCRIPTION_END

;--------------------------------

; Uninstaller

Section "Uninstall"
  
  Call un.registerExtensions
  
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
  Delete "$INSTDIR\Runtimes\win-x86\native\assimp.dll"
  Delete "$INSTDIR\Runtimes\win-x64\native\assimp.dll"
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
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\VS15.luasch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Obsidian.luasch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua\Monokai.luasch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\VS15.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Obsidian.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\NG_Center.cssch"
  Delete "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript\Monokai.cssch"
  Delete "$INSTDIR\Catalogs\TrCatalog.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr5.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr4.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr3.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr2.xml"
  Delete "$INSTDIR\Catalogs\Sounds.tr1.xml"
  Delete "$INSTDIR\Catalogs\NgCatalog.xml"
  Delete "$INSTDIR\Assets\Samples\TR1\uzi_fr.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\uw_swt.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\usekey.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\useitem.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\undwatr.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trx_roar.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trx_foot.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trx_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trx_atk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trapd_op.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\trapd_cl.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_move.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_hit.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_ground.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_atk2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_atk1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\torso_arm.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\thunder.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\takehit2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\takehit1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\switch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\swim.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\swch_up.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\splash.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Slipping.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\slide_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\slam_open.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\slam_close.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_stop.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_start.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_shoot.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_move.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_hit.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_ground.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\skate_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\shot_gun.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\setup.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\secret.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\sand_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rolling.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rokfall3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rokfall2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rokfall1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rico_02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rico_01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\reload.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rat_foot.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rat_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rat_chirp.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rat_atk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rapt_roar.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rapt_ft2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rapt_ft1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\rapt_atk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\push_blk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\powerup_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\pierre_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\pierre_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\pendulum.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\p&p05.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\p&p04.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\p&p03.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\p&p02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\p&p01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\natla_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\natla_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\mum_grwl.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\metald_open.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\metald_close.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\medi_fix.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\magnum.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_spinout.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_spinin.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_select.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_rotat.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_passport.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_guns.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_controls.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_compass.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\m_choose.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lion_roar.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lion_hurt.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lion_death.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lion_atk2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lion_atk1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lava_fountain.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lars_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lars_rico.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lars_fire.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lars_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lara_no.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lar_spks.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lar_jmp.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lar_die3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lar_die2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\lar_die1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\landing.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_wtfall_b.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_wtfall.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_water.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_rumb.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_lava.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_fire.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_conveyor.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\l_altar.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\hut_lower.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\hut_ground.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\hols_out.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\hols_in.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_25.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_24.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_23.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_22.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_21.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_20.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_19.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_18.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_17.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_16.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_15.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_14.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_13.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_12.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_11.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_10.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_09.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_08.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_07.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_06.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_05.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_04.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_03.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gym_01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gr_gr4.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gr_gr3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gr_gr2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gr_gr1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gorl_pant.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gorl_grnt.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gorl_foot.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gorl_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\go_watr.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\gen_door.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\foot04.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\foot03.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\foot02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\foot01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\floatswm.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\f2f_scrm.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\f2f_hitg.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\explos_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\explode.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\emp_gun.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\drill_start.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\drill.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_jat2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_jat1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_hwl.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_hit.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_d2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_d1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_at2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dog_at1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\dart.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\damocles.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\d_eagle.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\ct_ft4.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\ct_ft3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\ct_ft2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\ct_ft1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\croc_foot.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\croc_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\croc_at2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\croc_at1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\cowboy_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\cowboy_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\cogs.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\clsl_03.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\clsl_02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\clsl_01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Clim_up3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Clim_up2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Clim_up1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\chn_up.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\chn_down.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\chain_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\cent_roar.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bul_flsh.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bubbles.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\breath.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\body_sl2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\body_sl1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\block_fx.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\blanding.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\black_spch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\black_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bft04.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bft03.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bft02.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bft01.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_snrl2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_snrl1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_hit.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_grwl.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_feet.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bear_atk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bat_sqk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\bat_flp.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Back_jm3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\Back_jm2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\back_jm1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_wing.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_sneak2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_sneak1.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_needle.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_jatk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_hatch.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_expld.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_egg.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_die.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_ball.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\atlan_atk.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\at_ft4.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\at_ft3.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\at_ft2.wav"
  Delete "$INSTDIR\Assets\Samples\TR1\at_ft1.wav"
  Delete "$INSTDIR\WadTool.exe.config"
  Delete "$INSTDIR\WadTool.exe"
  Delete "$INSTDIR\TombLib.Scripting.Lua.dll"
  Delete "$INSTDIR\TombLib.Scripting.dll"
  Delete "$INSTDIR\TombLib.Scripting.ClassicScript.dll"
  Delete "$INSTDIR\TombLib.Rendering.dll"
  Delete "$INSTDIR\TombLib.Forms.dll"
  Delete "$INSTDIR\TombLib.dll"
  Delete "$INSTDIR\TombIDE.Shared.dll"
  Delete "$INSTDIR\TombIDE.ScriptingStudio.dll"
  Delete "$INSTDIR\TombIDE.ProjectMaster.dll"
  Delete "$INSTDIR\TombIDE.exe.config"
  Delete "$INSTDIR\TombIDE.exe"
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
  Delete "$INSTDIR\MiniFileAssociation.dll"
  Delete "$INSTDIR\ICSharpCode.AvalonEdit.dll"
  Delete "$INSTDIR\IconInjector.dll"
  Delete "$INSTDIR\IconExtractor.dll"
  Delete "$INSTDIR\File Association.exe.config"
  Delete "$INSTDIR\File Association.exe"
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
  RMDir "$INSTDIR\TIDE\NGC"
  RMDir "$INSTDIR\Runtimes\win-x86\native"
  RMDir "$INSTDIR\Runtimes\win-x64\native"
  RMDir "$INSTDIR\Runtimes\win-x86"
  RMDir "$INSTDIR\Runtimes\win-x64"
  RMDir "$INSTDIR\Resources\Localization\PL"
  RMDir "$INSTDIR\Resources\Localization\EN"
  RMDir "$INSTDIR\Resources\ClassicScript\Descriptions"
  RMDir "$INSTDIR\Resources\Localization"
  RMDir "$INSTDIR\Resources\ClassicScript"
  RMDir "$INSTDIR\Rendering\Legacy"
  RMDir "$INSTDIR\Rendering\DirectX11"
  RMDir "$INSTDIR\Native\64 bit"
  RMDir "$INSTDIR\Native\32 bit"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\Lua"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes\ClassicScript"
  RMDir "$INSTDIR\Configs\TextEditors\ColorSchemes"
  RMDir "$INSTDIR\Configs\TextEditors"
  RMDir "$INSTDIR\Assets\Samples\TR1"
  RMDir "$INSTDIR\Assets\Samples"
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

Function un.registerExtensions
  
  ExecShell "runas" "$INSTDIR\File Association.exe" '-d'
  Sleep 1000
    
FunctionEnd

Function .registerExtensions
  
  ${If} tideInstalled == "yes"
    ExecShell "runas" "$INSTDIR\File Association.exe" '-111'
  ${Else}
    ExecShell "runas" "$INSTDIR\File Association.exe" '-110'
  ${EndIf}
    
FunctionEnd

