FROM microsoft/aspnetcore-build:2.0.0 AS build

WORKDIR /src

COPY . .

WORKDIR /src/src/ShtikLive.Questions

RUN dotnet restore

RUN dotnet publish --output /app/ --configuration Release

FROM microsoft/aspnetcore:2.0.0

COPY --from=build /app /app/

WORKDIR /app

ENTRYPOINT ["dotnet", "ShtikLive.Questions.dll"]