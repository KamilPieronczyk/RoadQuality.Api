FROM mcr.microsoft.com/dotnet/sdk:5.0 as builder  
ENV DOTNET_CLI_TELEMETRY_OPTOUT 1

RUN mkdir -p /root/src/app  
WORKDIR /root/src/app  
COPY RoadQuality     RoadQuality  
WORKDIR /root/src/app/RoadQuality

RUN dotnet restore ./RoadQuality.csproj  
RUN dotnet publish -c release -o published -r linux-arm

FROM mcr.microsoft.com/dotnet/runtime:5.0.13-buster-slim-arm32v7

WORKDIR /root/  
COPY --from=builder /root/src/app/RoadQuality/published .

CMD ["dotnet", "./RoadQuality.dll"]