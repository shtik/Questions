FROM microsoft/aspnetcore-build:2.0.3 AS build

WORKDIR /src

COPY . .

WORKDIR /src/src/ShtikLive.Questions

RUN dotnet publish --output /output --configuration Release

FROM microsoft/aspnetcore:2.0.3

COPY --from=build /output /app/

WORKDIR /app

ENTRYPOINT ["dotnet", "ShtikLive.Questions.dll"]