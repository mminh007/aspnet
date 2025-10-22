# Dockerfile.template ở Project root
ARG SERVICE_FOLDER="Gateway"  # ví dụ: backend.User, backend.Store, ...


# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG SERVICE_FOLDER

COPY ./${SERVICE_FOLDER}/API/API.csproj ./API/

RUN dotnet restore ./API/API.csproj

COPY ./${SERVICE_FOLDER} ./

RUN dotnet build ./API/API.csproj -c Release --no-restore

FROM build AS publish

RUN dotnet publish ./API/API.csproj \
	-c Release \
	-o /app/publish \
	/p:UseAppHost=false \ 
	--no-build

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser

# Tạo thư mục certs trước khi chown
RUN mkdir -p /app/logs /app/certs && chown -R appuser:appuser /app

USER appuser

# Tạo thư mục logs để Serilog ghi file (nếu cấu hình)
RUN mkdir -p /app/logs

# Copy app
COPY --from=publish /app/publish ./

# Kestrel listen HTTP:8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080


# (Tuỳ chọn) timezone khớp VN
# RUN apt-get update && apt-get install -y tzdata && ln -fs /usr/share/zoneinfo/Asia/Ho_Chi_Minh /etc/localtime && dpkg-reconfigure -f noninteractive tzdata

ENTRYPOINT ["dotnet", "API.dll"]
