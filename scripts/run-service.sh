#!/bin/sh

docker exec -it power-lines-data-service /bin/sh -c "dotnet run --project ./PowerLinesDataService $@"
