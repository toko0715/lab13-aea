USE TallerLab13DB;
GO
IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioId int IDENTITY PRIMARY KEY,
        Correo nvarchar(120) NOT NULL CONSTRAINT UQ_Usuarios_Correo UNIQUE,
        ClaveHash nvarchar(60) NOT NULL,
        Rol nvarchar(30) NOT NULL CONSTRAINT DF_Usuarios_Rol DEFAULT N'Recepcionista',
        CONSTRAINT CK_Usuarios_Rol CHECK (Rol IN (N'Recepcionista', N'JefeTaller'))
    );
END;
GO
-- Los usuarios de prueba y hashes generados por la API se agregan debajo.
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo=N'recepcion.lab14@example.com')
    INSERT dbo.Usuarios(Correo,ClaveHash,Rol) VALUES(N'recepcion.lab14@example.com',N'$2a$12$GcbjlU3/M9FUzqfalWnv8eSb/7Ap3dbzCbqVTAHl1/CDa90TpN.PW',N'Recepcionista');
GO
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Correo=N'jefe.lab14@example.com')
    INSERT dbo.Usuarios(Correo,ClaveHash,Rol) VALUES(N'jefe.lab14@example.com',N'$2a$12$GZPwrNTaL9aaDSJ1cHXy5.QEckm3D.tByzjyK.XpaEAQeVP0pbhM2',N'JefeTaller');
GO
