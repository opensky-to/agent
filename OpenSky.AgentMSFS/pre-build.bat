copy /Y "%1\app.config.%2" "%1\app.config"
xcopy "%1\..\SoundPacks\FemaleUS" "%1\bin\%2\SoundPacks\FemaleUS" /E /I
xcopy "%1\..\SoundPacks\MaleUK" "%1\bin\%2\SoundPacks\MaleUK" /E /I