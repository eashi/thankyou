FROM mcr.microsoft.com/dotnet/runtime:3.1-bionic AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:3.1-bionic AS build
WORKDIR /src
COPY ["src/ThankYou.csproj", "src/"]
RUN dotnet restore "src/ThankYou.csproj"
COPY . .
WORKDIR "/src/src"
RUN dotnet build "ThankYou.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ThankYou.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ThankYou.dll"]
