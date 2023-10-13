
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Web/Web.csproj", "Web/"]
COPY ["Core/Core.csproj", "Core/"]
COPY ["Parsers/Parsers.csproj", "Parsers/"]
RUN dotnet restore "Web/Web.csproj"
COPY . .
WORKDIR "/src/Web"
RUN dotnet build "Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
RUN mkdir db
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Web.dll"]