# ---- Build stage ----
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy toàn bộ solution của service (chỉ service Auth)
COPY ./Backend.Product/Product.API/Product.API.csproj ./Product.API/
COPY ./Backend.Product/Product.BLL/Product.BLL.csproj ./Product.BLL/
COPY ./Backend.Product/Product.DAL/Product.DAL.csproj ./Product.DAL/
COPY ./Backend.Product/Product.Common/Product.Common.csproj ./Product.Common/

# Restore & build
RUN dotnet restore ./Product.API/Product.API.csproj

COPY ./Backend.Product ./

RUN dotnet build ./Product.API/Product.API.csproj -c Release --no-restore

FROM build AS publish

RUN dotnet publish ./Product.API/Product.API.csproj \
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

ENTRYPOINT ["dotnet", "Product.API.dll"]
