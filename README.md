# WhoRU-collector
Advanced Homework - Respond team, by Sébastien Gagnon

## Prerequisites
- .NET 5 SDK: https://dotnet.microsoft.com/download/dotnet/5.0 (TODO: dockerize ?)
- Docker for Desktop

## How to Use
To start the WhoRU collector service, either use the 'startCollector.ps1' script (Windows only) or start the service using 'dotnet run' from inside the App/ folder. (TODO: dockerize ?)

## Scripts
These scripts are provided for ease of use:

**startEventGenerator.ps1**: starts the fake-event-generator in HTTP mode.

**startCollector.ps1**: starts the WhoRU-collector service.

**startAlertConsumer.ps1**: starts an example alert consumer script that will output the events produced by the WhoRU-collector service. *(This script is provided for debugging purposes.)*

## Run tests
dotnet test