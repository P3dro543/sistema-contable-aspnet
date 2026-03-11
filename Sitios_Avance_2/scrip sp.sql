
use sistema_contable
--sp utili
DELIMITER $$

CREATE PROCEDURE sp_obtener_pantallas_por_rol (
    IN idRol INT
)
BEGIN
    SELECT 
        pantallas.id_pantalla,
        pantallas.nombre,
        pantallas.ruta
    FROM rol_pantalla
    JOIN pantallas 
        ON rol_pantalla.id_pantalla = pantallas.id_pantalla
    WHERE rol_pantalla.id_rol = idRol;
END$$

DELIMITER ;
-- Agregar a pantallas:
call sp_obtener_pantallas_por_rol(1)



