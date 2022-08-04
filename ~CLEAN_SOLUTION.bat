@echo off

FOR /d /r %%F IN (obj?) DO (
    echo Deleting folder: %%F
    @IF EXIST %%F RMDIR /S /Q "%%F"
)

FOR /d /r %%F IN (bin?) DO (
    echo Deleting folder: %%F
    @IF EXIST %%F RMDIR /S /Q "%%F"
)

FOR /d %%F IN (Build*) DO (
    echo Deleting folder: %%F
	@IF EXIST %%F RMDIR /S /Q "%%F"
)

FOR /d %%F IN (~Publish?) DO (
    echo Deleting folder: %%F
	@IF EXIST %%F RMDIR /S /Q "%%F"
)

FOR /d %%F IN (packages?) DO (
    echo Deleting folder: %%F
	@IF EXIST %%F RMDIR /S /Q "%%F"
)

RMDIR /S /Q ".vs"

@pause
