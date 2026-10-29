/*
  Siembra de permisos RBAC del Dashboard (idempotente: se puede ejecutar varias veces).

  El codigo de permiso que usa la API es  <MOD_NOMBRE>.<ACC_NOMBRE>:
    dashboard.ver | dashboard-finanzas.ver | dashboard-global.ver

  Asignacion por defecto a roles (ajustala a tu gusto desde Console.Tools o la UI de RBAC):
    ADMIN GLOBAL  -> dashboard.ver, dashboard-finanzas.ver, dashboard-global.ver
    ADMIN NEGOCIO -> dashboard.ver, dashboard-finanzas.ver
    VENDEDOR      -> dashboard.ver
*/
SET NOCOUNT ON;
BEGIN TRAN;

-- Modulos
INSERT INTO dbo.TBL_MODULOS (MOD_CODIGO, MOD_NOMBRE, MOD_DESCRIPCION, MOD_ES_SISTEMA, MOD_ESTADO)
SELECT v.cod, v.nom, v.dsc, 'S', 'A'
FROM (VALUES ('dsh',     'dashboard',          'Modulo de dashboard'),
             ('dsh-fin', 'dashboard-finanzas', 'Modulo de dashboard financiero'),
             ('dsh-glo', 'dashboard-global',   'Modulo de dashboard global (seleccion de negocio)')) v(cod, nom, dsc)
WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_MODULOS m WHERE m.MOD_NOMBRE = v.nom);

-- Accion
INSERT INTO dbo.TBL_ACCIONES (ACC_CODIGO, ACC_NOMBRE, ACC_DESCRIPCION, ACC_ES_SISTEMA, ACC_ESTADO)
SELECT 'ver', 'ver', 'Accion ver', 'S', 'A'
WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_ACCIONES WHERE ACC_NOMBRE = 'ver');

-- Permisos
INSERT INTO dbo.TBL_PERMISOS (PER_MOD_ID, PER_ACC_ID, PER_DESCRIPCION, PER_ESTADO)
SELECT m.MOD_ID, a.ACC_ID, 'Permite ver ' + m.MOD_NOMBRE, 'A'
FROM dbo.TBL_MODULOS m
CROSS JOIN dbo.TBL_ACCIONES a
WHERE m.MOD_NOMBRE IN ('dashboard', 'dashboard-finanzas', 'dashboard-global')
  AND a.ACC_NOMBRE = 'ver'
  AND NOT EXISTS (SELECT 1 FROM dbo.TBL_PERMISOS p WHERE p.PER_MOD_ID = m.MOD_ID AND p.PER_ACC_ID = a.ACC_ID);

-- Asignacion a roles (por nombre de rol, no por id)
INSERT INTO dbo.TBL_ROL_PERMISOS (ROLPER_ROL_ID, ROLPER_PER_ID, ROLPER_ASIGNADO_POR_USUARIO_ID, ROLPER_ESTADO)
SELECT r.ROL_ID, p.PER_ID, (SELECT MIN(USR_ID) FROM dbo.TBL_USUARIOS), 'A'
FROM (VALUES ('ADMIN GLOBAL',  'dashboard'), ('ADMIN GLOBAL',  'dashboard-finanzas'), ('ADMIN GLOBAL', 'dashboard-global'),
             ('ADMIN NEGOCIO', 'dashboard'), ('ADMIN NEGOCIO', 'dashboard-finanzas'),
             ('VENDEDOR',      'dashboard')) x(rol, modulo)
JOIN dbo.TBL_ROLES r    ON r.ROL_NOMBRE = x.rol
JOIN dbo.TBL_MODULOS m  ON m.MOD_NOMBRE = x.modulo
JOIN dbo.TBL_ACCIONES a ON a.ACC_NOMBRE = 'ver'
JOIN dbo.TBL_PERMISOS p ON p.PER_MOD_ID = m.MOD_ID AND p.PER_ACC_ID = a.ACC_ID
WHERE NOT EXISTS (SELECT 1 FROM dbo.TBL_ROL_PERMISOS rp WHERE rp.ROLPER_ROL_ID = r.ROL_ID AND rp.ROLPER_PER_ID = p.PER_ID);

COMMIT;
GO
