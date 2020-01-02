[![Build Status](https://johnwatson484.visualstudio.com/John%20D%20Watson/_apis/build/status/Power%20Lines%20Data%20Service?branchName=master)](https://johnwatson484.visualstudio.com/John%20D%20Watson/_build/latest?definitionId=32&branchName=master)

# Power Lines Data Service
Import base data for analysis

# Prerequisites
- Docker
- Docker Compose

# Running the application
The application is intended to be run and developed within a container.  A set of docker-compose files exist to support this.

## Run application in container

The application will be accessible on port `8000`.

```
docker-compose build
docker-compose up
```

## Develop application in container
The application will be accessible on port `5000`.

```
docker-compose -f docker-compose.yaml -f docker-compose.development.yaml build
docker-compose -f docker-compose.yaml -f docker-compose.development.yaml up
```

## Debug application in container
Running the above development container configuration will include a remote debugger that can be connected to using the below example VS Code debug configuration.

```
{
  "name": ".NET Core Docker Attach",
  "type": "coreclr",
  "request": "attach",
  "processId": "${command:pickRemoteProcess}",
  "pipeTransport": {
    "pipeProgram": "docker",
    "pipeArgs": [
      "exec",
      "-i",
      "power-lines-data-service"
    ],
    "debuggerPath": "/vsdbg/vsdbg",
    "pipeCwd": "${workspaceRoot}",
    "quoteArgs": false
  },
  "sourceFileMap": {
    "/PowerLinesDataService": "${workspaceFolder}"
  }
}
```
The `${command:pickRemoteProcess}` will prompt for which process to connect to within the container.  

Note that if this will not work if there is a space in the filepath to the VS Code extensions.  In that scenario the process Id should be manually added to the debug config.  The process Id can be found by running the below command.

`docker exec -i power-lines-data-service sh -s < "C:\Users\<USERNAME>\.vscode\extensions\ms-vscode.csharp-1.21.9\scripts\remoteProcessPickerScript"`

Local changes to code files will automatically trigger a rebuild and restart of the application within the container.

### Running service with arguments
A script, `./scripts/run-service.sh`, is provided to run the containerised service with arguments.

Accepted arguements:  
`-fixtures` import all available fixtures.  
`-results` import available results.  
`-all` import result data from all seasons.  If not supplied then only current season results are imported.

## Run tests
Unit tests are written in NUnit.

```
docker-compose -f docker-compose.test.yaml build
docker-compose -f docker-compose.test.yaml up
```

The test container will automatically close when all tests have been completed.  There is also the option to run the test container using a file watch to aide local development.

```
docker-compose -f docker-compose.test.yaml -f docker-compose.development.test.yaml build
docker-compose -f docker-compose.test.yaml -f docker-compose.development.test.yaml up
```
