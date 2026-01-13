@echo off
echo Limpando projeto...
dotnet clean

echo Buildando Release...
dotnet build -c Release

echo Gerando pacote NuGet...
dotnet pack -c Release -o C:\NugetLocal

echo Pacote gerado com sucesso!
pause