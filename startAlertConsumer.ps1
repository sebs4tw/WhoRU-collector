# example of an alert endpoint for debugging purposes
$http = [System.Net.HttpListener]::new()
$url = "http://localhost:8081/"

$http.Prefixes.Add($url)
$http.Start()

if($http.IsListening){
    Write-Output "WhoRU Collector Alert Consummer"
    Write-Output "HTTP Server Started ($url)"
}

try{
    while($http.IsListening){
        # async read to permit interruption (Ctrl + C)
        $contextTask = $http.GetContextAsync()
        while (-not $contextTask.AsyncWaitHandle.WaitOne(200)) { }
        $context = $contextTask.GetAwaiter().GetResult()

        $responseCode = 500
        $html = ""

        $method = $context.Request.HttpMethod
        $rawUrl = $context.Request.RawUrl
        Write-Output "Method: $method Url: $rawUrl"

        if($method -eq 'POST' -and $rawUrl -eq '/alert') {
            if($context.Request.HasEntityBody){
                # write body to console
                $streamReader = [System.IO.StreamReader]::new($context.Request.InputStream)
                $body = $streamReader.ReadToEnd()
                $streamReader.Dispose()

                Write-Output $body
                $responseCode = 200;
            }
            else {
                $responseCode = 400
                $html = "Bad Request"
            }
        }
        else {
            $responseCode = 404
            $html = "Not Found"
        }

        $context.Response.StatusCode = $responseCode
        $buffer = [System.Text.Encoding]::UTF8.GetBytes($html)
        $context.Response.ContentLength64 = $buffer.Length
        $context.Response.OutputStream.Write($buffer, 0, $buffer.Length)
        $context.Response.OutputStream.Close();
    }
}
finally{
    $http.Close();
}