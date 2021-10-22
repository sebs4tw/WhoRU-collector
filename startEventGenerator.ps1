# expects the fake-event-generator to be installed at this location
# change to reflect the true installation path
$genPath = "../fake-event-generator/dist/amd64-windows/event-gen.exe"
$genParams = "http -H localhost -p 8080"

$seed = Get-Random -Minimum 1 -Maximum 1000000
$command = "$genPath $genParams --seed $seed"
Invoke-Expression -Command $command