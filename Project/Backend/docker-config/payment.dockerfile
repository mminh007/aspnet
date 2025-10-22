# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution của service (chỉ service Auth)
COPY ./Backend.Payment/Payment.API/Payment.API.csproj ./Payment.API/
COPY ./Backend.Payment/Payment.BLL/Payment.BLL.csproj ./Payment.BLL/
COPY ./Backend.Payment/Payment.DAL/Payment.DAL.csproj ./Payment.DAL/
COPY ./Backend.Payment/Payment.Common/Payment.Common.csproj ./Payment.Common/

# Restore & build
RUN dotnet restore ./Payment.API/Payment.API.csproj

COPY ./Backend.Payment ./

RUN dotnet build ./Payment.API/Payment.API.csproj -c Release --no-restore

FROM build AS publish

RUN dotnet publish ./Payment.API/Payment.API.csproj \
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

ENTRYPOINT ["dotnet", "Payment.API.dll"]
