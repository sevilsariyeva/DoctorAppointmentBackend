FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY DoctorAppointment/DoctorAppointment.csproj ./DoctorAppointment/
WORKDIR /app/DoctorAppointment

RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out .

ENV ASPNETCORE_URLS=http://+:10000

CMD ["dotnet", "DoctorAppointment.dll"]
