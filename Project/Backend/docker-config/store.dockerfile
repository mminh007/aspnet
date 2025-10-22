# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution của service (chỉ service Auth)
COPY ./Backend.Store/Store.API/Store.API.csproj ./Store.API/
COPY ./Backend.Store/Store.BLL/Store.BLL.csproj ./Store.BLL/
COPY ./Backend.Store/Store.DAL/Store.DAL.csproj ./Store.DAL/
COPY ./Backend.Store/Store.Common/Store.Common.csproj ./Store.Common/

# Restore & build
RUN dotnet restore ./Store.API/Store.API.csproj

COPY ./Backend.Store ./

RUN dotnet build ./Store.API/Store.API.csproj -c Release --no-restore

FROM build AS publish

RUN dotnet publish ./Store.API/Store.API.csproj \
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

ENTRYPOINT ["dotnet", "Store.API.dll"]
