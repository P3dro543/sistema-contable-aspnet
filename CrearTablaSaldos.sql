--
-- Nueva tabla para almacenar los saldos historicos de cada cuenta
-- al final de un Periodo Contable (Historia ADM14: Mayorización)
--

CREATE TABLE `saldos_mensuales_cuenta` (
  `id_saldo` int NOT NULL AUTO_INCREMENT,
  `id_periodo` int NOT NULL,
  `id_cuenta` int NOT NULL,
  `saldo_inicial` decimal(14,2) NOT NULL DEFAULT '0.00',
  `debitos_mes` decimal(14,2) NOT NULL DEFAULT '0.00',
  `creditos_mes` decimal(14,2) NOT NULL DEFAULT '0.00',
  `saldo_final` decimal(14,2) NOT NULL DEFAULT '0.00',
  PRIMARY KEY (`id_saldo`),
  KEY `fk_saldos_periodo` (`id_periodo`),
  KEY `fk_saldos_cuenta` (`id_cuenta`),
  CONSTRAINT `fk_saldos_periodo` FOREIGN KEY (`id_periodo`) REFERENCES `periodos_contables` (`id_periodo`) ON DELETE RESTRICT ON UPDATE CASCADE,
  CONSTRAINT `fk_saldos_cuenta` FOREIGN KEY (`id_cuenta`) REFERENCES `cuentas_contables` (`id_cuenta`) ON DELETE RESTRICT ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_general_ci;
