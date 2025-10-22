# Dockerfile.template ở Project root
ARG SERVICE_FOLDER="Backend.Authentication"  # ví dụ: backend.User, backend.Store, ...
ARG API_PROJECT="Auth"                   # ví dụ: User.API, Store.API (hoặc Worker)

# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ARG SERVICE_FOLDER
ARG API_PROJECT

# Copy toàn bộ solution của service (chỉ service Auth)
COPY ./${SERVICE_FOLDER}/${API_PROJECT}.API/${API_PROJECT}.API.csproj ./${API_PROJECT}.API/
COPY ./${SERVICE_FOLDER}/${API_PROJECT}.BLL/${API_PROJECT}.BLL.csproj ./${API_PROJECT}.BLL/
COPY ./${SERVICE_FOLDER}/${API_PROJECT}.DAL/${API_PROJECT}.DAL.csproj ./${API_PROJECT}.DAL/
COPY ./${SERVICE_FOLDER}/${API_PROJECT}.Common/${API_PROJECT}.Common.csproj ./${API_PROJECT}.Common/

# Restore & build
RUN dotnet restore ./${API_PROJECT}.API/${API_PROJECT}.API.csproj

COPY ./${SERVICE_FOLDER} ./

RUN dotnet build ./${API_PROJECT}.API/${API_PROJECT}.API.csproj -c Release --no-restore

FROM build AS publish
ARG API_PROJECT

RUN dotnet publish ./${API_PROJECT}.API/${API_PROJECT}.API.csproj \
	-c Release \
	-o /app/publish \
	/p:UseAppHost=false \ 
	--no-build

# ---- Runtime stage ----
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
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

ARG API_PROJECT
ENV APP_DLL=${API_PROJECT}.API.dll
ENTRYPOINT ["/bin/sh", "-c", "dotnet \"$APP_DLL\""]
