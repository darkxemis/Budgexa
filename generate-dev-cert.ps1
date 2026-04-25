# Script para generar certificado de desarrollo para ASP.NET Core en Docker
# Ejecuta esto en PowerShell desde la raíz del proyecto

$certPassword = "BudgexaDev123!"
$certPath = "backend/BudgexaApi/https/aspnetcore-dev.pfx"

# Generar certificado
if (!(Test-Path $certPath)) {
    dotnet dev-certs https -ep $certPath -p $certPassword
    Write-Host "Certificado generado en $certPath"
} else {
    Write-Host "El certificado ya existe en $certPath"
}
