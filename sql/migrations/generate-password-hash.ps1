# Script para generar hash de password correcto
# El hash debe ser Base64 de SHA256

$password = "admin123"
$sha256 = [System.Security.Cryptography.SHA256]::Create()
$bytes = [System.Text.Encoding]::UTF8.GetBytes($password)
$hash = $sha256.ComputeHash($bytes)
$base64Hash = [Convert]::ToBase64String($hash)

Write-Host "Password: $password"
Write-Host "Hash (Base64): $base64Hash"
Write-Host ""
Write-Host "SQL UPDATE para corregir passwords:"
Write-Host "UPDATE Users SET PasswordHash = '$base64Hash' WHERE Email IN ('admin@techcorp.com', 'admin@demo.com');"
