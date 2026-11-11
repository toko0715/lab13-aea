IF DB_ID(N'TallerLab13DB') IS NULL CREATE DATABASE TallerLab13DB;
GO
USE TallerLab13DB;
GO
SET XACT_ABORT ON;
IF OBJECT_ID(N'dbo.Clientes', N'U') IS NULL
BEGIN
    BEGIN TRANSACTION;
    CREATE TABLE dbo.Clientes (
        ClienteId int IDENTITY PRIMARY KEY,
        DNI nvarchar(8) NOT NULL CONSTRAINT UQ_Clientes_DNI UNIQUE,
        Nombre nvarchar(100) NOT NULL,
        Email nvarchar(120) NOT NULL,
        Telefono nvarchar(20) NOT NULL
    );
    CREATE TABLE dbo.Vehiculos (
        VehiculoId int IDENTITY PRIMARY KEY,
        Placa nvarchar(7) NOT NULL CONSTRAINT UQ_Vehiculos_Placa UNIQUE,
        Marca nvarchar(40) NOT NULL,
        Modelo nvarchar(60) NOT NULL,
        Anio int NOT NULL CONSTRAINT CK_Vehiculos_Anio CHECK (Anio BETWEEN 1980 AND 2100),
        ClienteId int NOT NULL,
        CONSTRAINT FK_Vehiculos_Clientes FOREIGN KEY (ClienteId) REFERENCES dbo.Clientes(ClienteId)
    );
    CREATE INDEX IX_Vehiculos_ClienteId ON dbo.Vehiculos(ClienteId);
    INSERT dbo.Clientes (DNI,Nombre,Email,Telefono) VALUES
        (N'40000001',N'Ana Torres',N'ana@example.com',N'900000001'),
        (N'40000002',N'Bruno Rojas',N'bruno@example.com',N'900000002'),
        (N'40000003',N'Carla Medina',N'carla@example.com',N'900000003');
    INSERT dbo.Vehiculos (Placa,Marca,Modelo,Anio,ClienteId) VALUES
        (N'ABC-123',N'Toyota',N'Corolla',2022,1),
        (N'DEF-456',N'Kia',N'Rio',2021,1),
        (N'GHI-789',N'Hyundai',N'Accent',2023,2),
        (N'JKL-321',N'Nissan',N'Versa',2020,3);
    COMMIT TRANSACTION;
END;
GO
SELECT (SELECT COUNT(*) FROM dbo.Clientes) AS Clientes,
       (SELECT COUNT(*) FROM dbo.Vehiculos) AS Vehiculos;
