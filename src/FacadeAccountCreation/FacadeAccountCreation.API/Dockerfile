FROM defradigital/dotnetcore:dotnet10.0 AS base
USER root
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080
 
RUN apk add --no-cache icu-libs
ENV DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=0
 
FROM defradigital/dotnetcore-development:dotnet10.0 AS build
USER root
WORKDIR /src
COPY ["FacadeAccountCreation.API/FacadeAccountCreation.API.csproj", "FacadeAccountCreation.API/"]
COPY ["FacadeAccountCreation.Core/FacadeAccountCreation.Core.csproj", "FacadeAccountCreation.Core/"]
COPY ["NuGet.Config", ""]
 
ARG PAT=""
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config
 
RUN dotnet restore "FacadeAccountCreation.API/FacadeAccountCreation.API.csproj" --configfile "./NuGet.Config"
 
COPY FacadeAccountCreation.API/. ./FacadeAccountCreation.API/.
COPY FacadeAccountCreation.Core/. ./FacadeAccountCreation.Core/.
WORKDIR "/src/FacadeAccountCreation.API"

FROM build AS publish
RUN dotnet publish "FacadeAccountCreation.API.csproj" -c Release -o /app/publish

 
FROM base AS final
 
# Switch to the non-root user
USER dotnet
 
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FacadeAccountCreation.API.dll"]