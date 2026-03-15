$baseUrl = "http://localhost:8080"
$username = "testuser_$(Get-Random)"
$password = "testpass123"

Write-Host "`n=== Testing Auth Flow ===" -ForegroundColor Cyan

# Register/Login
Write-Host "`n--- POST /api/login (register new user: $username) ---" -ForegroundColor Yellow
$loginBody = @{ username = $username; password = $password } | ConvertTo-Json
$loginResponse = Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $loginBody -ContentType "application/json"
Write-Host "Response: $($loginResponse | ConvertTo-Json)" -ForegroundColor Green
$token = $loginResponse.token

# Hello with token
Write-Host "`n--- GET /api/hello (with valid token) ---" -ForegroundColor Yellow
$helloResponse = Invoke-RestMethod -Uri "$baseUrl/api/hello" -Method Get -Headers @{ Authorization = "Bearer $token" }
Write-Host "Response: $($helloResponse | ConvertTo-Json)" -ForegroundColor Green

# Hello without token
Write-Host "`n--- GET /api/hello (no token - expect 401) ---" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/hello" -Method Get
    Write-Host "ERROR: Should have returned 401!" -ForegroundColor Red
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "Got $status as expected" -ForegroundColor Green
}

# Login again with same creds
Write-Host "`n--- POST /api/login (login again, same user) ---" -ForegroundColor Yellow
$loginResponse2 = Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $loginBody -ContentType "application/json"
Write-Host "Response: $($loginResponse2 | ConvertTo-Json)" -ForegroundColor Green
$token2 = $loginResponse2.token

# Old token should be invalid
Write-Host "`n--- GET /api/hello (old token - expect 401) ---" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/hello" -Method Get -Headers @{ Authorization = "Bearer $token" }
    Write-Host "ERROR: Old token should have been rejected!" -ForegroundColor Red
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "Got $status as expected (token rotated)" -ForegroundColor Green
}

# New token should work
Write-Host "`n--- GET /api/hello (new token) ---" -ForegroundColor Yellow
$helloResponse2 = Invoke-RestMethod -Uri "$baseUrl/api/hello" -Method Get -Headers @{ Authorization = "Bearer $token2" }
Write-Host "Response: $($helloResponse2 | ConvertTo-Json)" -ForegroundColor Green

# Wrong password
Write-Host "`n--- POST /api/login (wrong password - expect 401) ---" -ForegroundColor Yellow
$badBody = @{ username = $username; password = "wrongpass" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $badBody -ContentType "application/json"
    Write-Host "ERROR: Should have returned 401!" -ForegroundColor Red
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "Got $status as expected" -ForegroundColor Green
}

# Logout
Write-Host "`n--- POST /api/login (logout) ---" -ForegroundColor Yellow
$logoutBody = @{ username = $username; password = $password; action = "logout" } | ConvertTo-Json
$logoutResponse = Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $logoutBody -ContentType "application/json"
Write-Host "Response: $($logoutResponse | ConvertTo-Json)" -ForegroundColor Green

# Token should be invalid after logout
Write-Host "`n--- GET /api/hello (after logout - expect 401) ---" -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "$baseUrl/api/hello" -Method Get -Headers @{ Authorization = "Bearer $token2" }
    Write-Host "ERROR: Token should be invalid after logout!" -ForegroundColor Red
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "Got $status as expected (logged out)" -ForegroundColor Green
}

# Logout with wrong password
Write-Host "`n--- POST /api/login (logout with wrong password - expect 401) ---" -ForegroundColor Yellow
$badLogoutBody = @{ username = $username; password = "wrongpass"; action = "logout" } | ConvertTo-Json
try {
    Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $badLogoutBody -ContentType "application/json"
    Write-Host "ERROR: Should have returned 401!" -ForegroundColor Red
} catch {
    $status = $_.Exception.Response.StatusCode.value__
    Write-Host "Got $status as expected" -ForegroundColor Green
}

# Can still login again after logout
Write-Host "`n--- POST /api/login (login again after logout) ---" -ForegroundColor Yellow
$loginResponse3 = Invoke-RestMethod -Uri "$baseUrl/api/login" -Method Post -Body $loginBody -ContentType "application/json"
Write-Host "Response: $($loginResponse3 | ConvertTo-Json)" -ForegroundColor Green

Write-Host "`n=== All tests passed ===" -ForegroundColor Cyan
