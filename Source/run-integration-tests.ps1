#Requires -PSEdition Core

$ErrorActionPreference = "Stop"
$ProgressPreference = "SilentlyContinue"

Write-Output "***********************************************************"
Write-Output "MockRegister integration tests"
Write-Output "***********************************************************"


# Run integration tests
docker-compose -f docker-compose.IntegrationTests.yml up --build --abort-on-container-exit --exit-code-from mock-register-integration-tests
$_lastExitCode = $LASTEXITCODE

# Stop containers
docker-compose -f docker-compose.IntegrationTests.yml down

if ($_lastExitCode -eq 0) {
    Write-Output "***********************************************************"
    Write-Output "✔ SUCCESS: MockRegister integration tests passed"
    Write-Output "***********************************************************"
    exit 0
}
else {
    Write-Output "***********************************************************"
    Write-Output "❌ FAILURE: MockRegister integration tests failed"
    Write-Output "***********************************************************"
    exit 1
}
