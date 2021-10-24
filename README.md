# WhoRU-collector
Advanced Homework - Respond team, by Sébastien Gagnon

## Prerequisites
- .NET 5 SDK: https://dotnet.microsoft.com/download/dotnet/5.0)
- Docker for Desktop
- PowerShell (optional)

## How to Use
To start the WhoRU collector service, either use the 'startCollector.ps1' script or start the service using 'dotnet run' from inside the App/ folder.

The software dependencies (TimescaleDB and RabbitMQ) are pre-configured and can be lauched using 'docker-compose up'.

## Scripts
These scripts are provided for ease of use:

**startEventGenerator.ps1**: starts the fake-event-generator in HTTP mode. (Note: you must configure the path of your 'fake-event-generator' binary.)

**startCollector.ps1**: starts the WhoRU-collector service. The collector will consume events over HTTP and output notifications to a RabbitMQ queue.

**startNotificatonConsumer.ps1**: starts a notification consumer script that will consume and output events from the RabbitMQ queue.

## Running tests
From the prompt: dotnet test