$hostname = "localhost"
$user = "mquser"
$pass = "7BLB6qgjyrV4dIBxtk3M"
$queue = "security-notifications"

dotnet run --project securityconsumer $hostname $user $pass $queue