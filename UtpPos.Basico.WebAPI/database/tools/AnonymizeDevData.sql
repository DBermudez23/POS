/*
  Anonimiza los datos personales de una BD ANTES de generar un .bak para compartir con el equipo.
  Ejecutalo sobre una COPIA, nunca sobre una BD real. Es idempotente.

  Que hace:
    - Usuarios: nombre, correo, documento y telefono ficticios; TODAS las claves quedan con la clave de
      desarrollo 'UtpPos#Dev2026' (hash BCrypt factor 12). Los hashes reales se descartan.
    - Terceros y negocios: nombre, documento/NIT, direccion y telefono ficticios.
    - Sesiones: se borran (IPs, user-agent, hashes de refresh token).
    - Productos: se quitan las URLs de imagen (apuntaban a una cuenta externa de Cloudinary).

  Despues de ejecutarlo, reconstruye las tablas y reduce el archivo para no dejar restos de los datos
  viejos en paginas libres (ver README, seccion "Generar un .bak nuevo").
*/
SET NOCOUNT ON;
SET XACT_ABORT ON;
BEGIN TRAN;

DECLARE @DevHash VARCHAR(100) = '$2a$12$WflFewBrISoMmsBesYp.le0cV9HtG8wXvbI5ID.dT335mXs1zAvc2';

-- Usuarios (correos de los 3 roles base legibles; el resto, genericos)
UPDATE u SET
    USR_NOMBRE           = CASE u.USR_ID WHEN 1 THEN 'Admin Global' WHEN 2 THEN 'Vendedor Demo' WHEN 1002 THEN 'Admin Negocio Demo'
                                ELSE 'Usuario Demo ' + CAST(u.USR_ID AS VARCHAR(10)) END,
    USR_CORREO           = CASE u.USR_ID WHEN 1 THEN 'admin.global@utppos.local' WHEN 2 THEN 'vendedor@utppos.local' WHEN 1002 THEN 'admin.negocio@utppos.local'
                                ELSE 'usuario' + CAST(u.USR_ID AS VARCHAR(10)) + '@utppos.local' END,
    USR_NUMERO_DOCUMENTO = CAST(1000000000 + u.USR_ID AS VARCHAR(20)),
    USR_TELEFONO         = '300' + RIGHT('0000000' + CAST(u.USR_ID AS VARCHAR(7)), 7),
    USR_PASSWORD         = @DevHash
FROM dbo.TBL_USUARIOS u;

-- Terceros
UPDATE dbo.TBL_TERCEROS SET
    TER_NOMBRE           = 'Tercero Demo ' + CAST(TER_ID AS VARCHAR(10)),
    TER_NUMERO_DOCUMENTO = CAST(2000000000 + TER_ID AS VARCHAR(20)),
    TER_DIRECCION        = CASE WHEN TER_DIRECCION IS NULL THEN NULL ELSE 'Calle 1 # 1-' + CAST(TER_ID AS VARCHAR(10)) END,
    TER_TELEFONO         = '310' + RIGHT('0000000' + CAST(TER_ID AS VARCHAR(7)), 7);

-- Negocios
UPDATE dbo.TBL_NEGOCIO SET
    NEG_NIT       = CAST(900000000 + NEG_ID AS VARCHAR(20)),
    NEG_NOMBRE    = 'UTP POS Sede ' + CAST(NEG_ID AS VARCHAR(10)),
    NEG_DIRECCION = 'Carrera 1 # 1-' + CAST(NEG_ID AS VARCHAR(10)),
    NEG_TELEFONO  = '320' + RIGHT('0000000' + CAST(NEG_ID AS VARCHAR(7)), 7);

-- Sesiones y productos
DELETE FROM dbo.TBL_SESIONES;
UPDATE dbo.TBL_PRODUCTOS SET PROD_IMAGEN = '' WHERE PROD_IMAGEN <> '';

COMMIT;
GO
