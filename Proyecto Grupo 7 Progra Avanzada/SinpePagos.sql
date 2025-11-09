-- Script de tabla SINPE (si no usan migraciones)
IF OBJECT_ID('dbo.SinpePagos','U') IS NULL
BEGIN
    CREATE TABLE dbo.SinpePagos (
      IdSinpe INT IDENTITY(1,1) PRIMARY KEY,
      TelefonoOrigen VARCHAR(10) NOT NULL,
      NombreOrigen VARCHAR(200) NOT NULL,
      TelefonoDestinatario VARCHAR(10) NOT NULL,
      NombreDestinatario VARCHAR(200) NOT NULL,
      Monto DECIMAL(18,2) NOT NULL,
      FechaDeRegistro DATETIME NOT NULL DEFAULT(GETDATE()),
      Descripcion VARCHAR(50) NULL,
      Estado BIT NOT NULL DEFAULT(0)
    );
END
GO
