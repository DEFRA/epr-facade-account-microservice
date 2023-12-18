FROM defradigital/dotnetcore:dotnet6.0 AS base
ENV ASPNETCORE_URLS=http://*:8080
EXPOSE 8080

FROM defradigital/dotnetcore-development:dotnet6.0 AS build
COPY --chown=dotnet:dotnet ["FacadeAccountCreation.API/FacadeAccountCreation.API.csproj", "FacadeAccountCreation.API/"]
COPY --chown=dotnet:dotnet ["FacadeAccountCreation.Core/FacadeAccountCreation.Core.csproj", "FacadeAccountCreation.Core/"]
COPY --chown=dotnet:dotnet ["NuGet.Config", ""]

ARG PAT=""
RUN sed -i "s|</configuration>|<packageSourceCredentials><epr-packaging-common><add key=\"Username\" value=\"PAT\" /><add key=\"ClearTextPassword\" value=\"${PAT}\" /></epr-packaging-common></packageSourceCredentials></configuration>|" NuGet.Config

RUN dotnet restore "FacadeAccountCreation.API/FacadeAccountCreation.API.csproj" --configfile "./NuGet.Config"
COPY --chown=dotnet:dotnet FacadeAccountCreation.API/. ./FacadeAccountCreation.API/.
COPY --chown=dotnet:dotnet FacadeAccountCreation.Core/. ./FacadeAccountCreation.Core/.
WORKDIR "/home/dotnet/FacadeAccountCreation.API"
RUN dotnet build "FacadeAccountCreation.API.csproj" -c Release -o /home/dotnet/build

FROM build AS publish
RUN dotnet publish "FacadeAccountCreation.API.csproj" -c Release -o /home/dotnet/publish

FROM base AS final

# Switch to the non-root user
USER dotnet

COPY --from=publish /home/dotnet/publish .
ENTRYPOINT ["dotnet", "FacadeAccountCreation.API.dll"]
