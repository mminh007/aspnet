# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution của service (chỉ service Auth)
COPY ./Backend.Authentication/Auth.API/Auth.API.csproj ./Auth.API/
COPY ./Backend.Authentication/Auth.BLL/Auth.BLL.csproj ./Auth.BLL/
COPY ./Backend.Authentication/Auth.DAL/Auth.DAL.csproj ./Auth.DAL/
COPY ./Backend.Authentication/Auth.Common/Auth.Common.csproj ./Auth.Common/

# Restore & build
RUN dotnet restore ./Auth.API/Auth.API.csproj

COPY ./Backend.Authentication ./

RUN dotnet build ./Auth.API/Auth.API.csproj -c Release --no-restore

FROM build AS publish

RUN dotnet publish ./Auth.API/Auth.API.csproj \
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

ENTRYPOINT ["dotnet", "Auth.API.dll"]
