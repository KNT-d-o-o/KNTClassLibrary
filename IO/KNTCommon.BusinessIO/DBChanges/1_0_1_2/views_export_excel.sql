DROP VIEW IF EXISTS transactions_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactions_exp` AS
                            SELECT 
                                `transactions`.`TransactionId` AS `TransactionId`,
                                `transactions`.`DateAndTime` AS `DateAndTime`,
                                `r1`.`ResultDescription` AS `ResultDescription`,
                                `transactions`.`ResultValue` AS `ResultValue`,
                                `transactions`.`DmcInternal` AS `DmcInternal`,
                                `transactions`.`DmcCustomer` AS `DmcCustomer`,
                                `transactions`.`Counter` AS `Counter`,
                                `transactions`.`Etalon` AS `Etalon`,
                                `transactions`.`Msa` AS `Msa`,
                                `transactions`.`qualityCode` AS `qualityCode`,
                                `transactions`.`ItemId` AS `ItemId`,
                                `items`.`ItemName` AS `ItemName`,
                                `transactions`.`EmployeeId` AS `EmployeeId`,
                                `transactions`.`OrderId` AS `OrderId`,
                                `transactions`.`PlcId` AS `PlcId`
                            FROM
                                ((`transactions`
                                LEFT JOIN `results` `r1` ON ((`transactions`.`ResultId` = `r1`.`ResultId`)))
                                LEFT JOIN `items` ON ((`transactions`.`ItemId` = `items`.`ItemId`)));

DROP VIEW IF EXISTS transactiondetails_exp;
                    CREATE 
                        ALGORITHM = UNDEFINED 
                        DEFINER = `KNT`@`%` 
                        SQL SECURITY DEFINER
                    VIEW `transactiondetails_exp` AS
                        SELECT 
                            `transactiondetails`.`TransactionId` AS `TransactionId`,
                            `transactiondetails`.`OperationId` AS `OperationId`,
                            `operations`.`OperationName` AS `OperationName`,
                            `transactiondetails`.`ProgramId` AS `ProgramId`,
                            `programs`.`ProgramName` AS `ProgramName`,
                            `transactiondetails`.`DateAndTime` AS `DateAndTime`,
                            `r2`.`ResultDescription` AS `ResultDescriptionDetail`,
                            `transactiondetails`.`ResultValue` AS `ResultValue`,
                            `transactiondetails`.`MachineStation` AS `MachineStation`,
                            `transactiondetails`.`MachineSubStation` AS `MachineSubStation`,
                            `transactiondetails`.`ErrorId` AS `ErrorId`,
                            `transactiondetails`.`MeasurementNumber` AS `MeasurementNumber`,
                            `transactiondetails`.`MeasurementType` AS `MeasurementType`,
                            `transactiondetails`.`Note` AS `Note`,
                            `transactiondetails`.`MesErp` AS `MesErp`,
                            `transactions`.`Etalon` AS `Etalon`
                        FROM
                            (((`transactiondetails`
                            JOIN `transactions` ON ((`transactiondetails`.`TransactionId` = `transactions`.`TransactionId`))
                            LEFT JOIN `operations` ON ((`transactiondetails`.`OperationId` = `operations`.`OperationId`)))
                            LEFT JOIN `programs` ON ((`transactiondetails`.`ProgramId` = `programs`.`ProgramId`)))
                            LEFT JOIN `results` `r2` ON ((`transactiondetails`.`ResultId` = `r2`.`ResultId`)));

DROP VIEW IF EXISTS transactionprograms_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactionprograms_exp` AS
                            SELECT 
                                `transactions`.`TransactionId` AS `TransactionId`,
                                `transactions`.`DateAndTime` AS `DateAndTime`,
                                `transactions`.`DmcInternal` AS `DmcInternal`,
                                `transactions`.`DmcCustomer` AS `DmcCustomer`,
                                `transactions`.`ItemId` AS `ItemId`,
                                `items`.`ItemName` AS `ItemName`,
                                `transactions`.`qualityCode` AS `qualityCode`,
                                `transactions`.`PlcId` AS `PlcId`,
                                `transactions`.`ResultId` AS `ResultId`,
                                `r1`.`ResultDescription` AS `ResultDescription`,
                                `transactions`.`Etalon` AS `Etalon`
                            FROM
                                ((`transactions`
                                LEFT JOIN `results` `r1` ON ((`transactions`.`ResultId` = `r1`.`ResultId`)))
                                LEFT JOIN `items` ON ((`transactions`.`ItemId` = `items`.`ItemId`)));

DROP VIEW IF EXISTS transactionslog_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactionslog_exp` AS
                            SELECT 
                                `transactionslog`.`TransactionId` AS `TransactionId`,
                                `transactionslog`.`TransactionVer` AS `TransactionVer`,
                                `transactionslog`.`DateAndTime` AS `DateAndTime`,
                                `r1`.`ResultDescription` AS `ResultDescription`,
                                `transactionslog`.`ResultValue` AS `ResultValue`,
                                `transactionslog`.`DmcInternal` AS `DmcInternal`,
                                `transactionslog`.`DmcCustomer` AS `DmcCustomer`,
                                `transactionslog`.`Counter` AS `Counter`,
                                `transactionslog`.`Etalon` AS `Etalon`,
                                `transactionslog`.`qualityCode` AS `qualityCode`,
                                `transactionslog`.`ItemId` AS `ItemId`,
                                `items`.`ItemName` AS `ItemName`,
                                `transactionslog`.`EmployeeId` AS `EmployeeId`,
                                `transactionslog`.`OrderId` AS `OrderId`,
                                `transactionslog`.`PlcId` AS `PlcId`
                            FROM
                                ((`transactionslog`
                                LEFT JOIN `results` `r1` ON ((`transactionslog`.`ResultId` = `r1`.`ResultId`)))
                                LEFT JOIN `items` ON ((`transactionslog`.`ItemId` = `items`.`ItemId`)));

DROP VIEW IF EXISTS transactiondetailslog_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactiondetailslog_exp` AS
                            SELECT 
                                `transactiondetailslog`.`TransactionId` AS `TransactionId`,
                                `transactiondetailslog`.`TransactionVer` AS `TransactionVer`,
                                `transactiondetailslog`.`OperationId` AS `OperationId`,
                                `operations`.`OperationName` AS `OperationName`,
                                `transactiondetailslog`.`ProgramId` AS `ProgramId`,
                                `programs`.`ProgramName` AS `ProgramName`,
                                `transactiondetailslog`.`DateAndTime` AS `DateAndTime`,
                                `r2`.`ResultDescription` AS `ResultDescriptionDetail`,
                                `transactiondetailslog`.`ResultValue` AS `ResultValue`,
                                `transactiondetailslog`.`MachineStation` AS `MachineStation`,
                                `transactiondetailslog`.`MachineSubStation` AS `MachineSubStation`,
                                `transactiondetailslog`.`ErrorId` AS `ErrorId`,
                                `transactiondetailslog`.`MeasurementNumber` AS `MeasurementNumber`,
                                `transactiondetailslog`.`MeasurementType` AS `MeasurementType`,
                                `transactiondetailslog`.`Note` AS `Note`,
                                `transactiondetailslog`.`MesErp` AS `MesErp`,
                                `transactionslog`.`Etalon` AS `Etalon`
                            FROM
                                (((`transactiondetailslog`
                                JOIN `transactionslog` ON ((`transactiondetailslog`.`TransactionId` = `transactionslog`.`TransactionId`))
                                LEFT JOIN `operations` ON ((`transactiondetailslog`.`OperationId` = `operations`.`OperationId`)))
                                LEFT JOIN `programs` ON ((`transactiondetailslog`.`ProgramId` = `programs`.`ProgramId`)))
                                LEFT JOIN `results` `r2` ON ((`transactiondetailslog`.`ResultId` = `r2`.`ResultId`)));

DROP VIEW IF EXISTS leaktestermeasurements_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `leaktestermeasurements_exp` AS
                            SELECT 
                                `transactiondetails`.`TransactionId` AS `TransactionId`,
                                `transactiondetails`.`DateAndTime` AS `DateAndTime`,
                                `leaktestermeasurements`.`Result` AS `Result`,
                                `transactions`.`DmcInternal` AS `DmcInternal`,
                                `leaktestermeasurements`.`Leak` AS `Leak`,
                                `leaktestermeasurements`.`DeltaP` AS `DeltaP`,
                                `leaktestermeasurements`.`Thread` AS `Thread`,
                                `leaktestermeasurements`.`Error` AS `Error`,
                                `leaktestermeasurements`.`ProgramId` AS `ProgramId`,
                                `items`.`ItemName` AS `ItemName`,
                                `leaktestermeasurements`.`MeasurementNumber` AS `MeasurementNumber`,
                                `leaktestermeasurements`.`TransactionDetailsId` AS `TransactionDetailsId`,
                                `transactions`.`Etalon` AS `Etalon`
                            FROM
                                (((`transactiondetails`
                                JOIN `leaktestermeasurements` ON ((`transactiondetails`.`TransactionDetailsCounter` = `leaktestermeasurements`.`TransactionDetailsId`)))
                                JOIN `transactions` ON ((`transactions`.`TransactionId` = `leaktestermeasurements`.`TransactionId`)))
                                LEFT JOIN `items` ON ((`items`.`ItemId` = `transactions`.`ItemId`)));

DROP VIEW IF EXISTS leaktestermeasdetails_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `leaktestermeasdetails_exp` AS
                            SELECT 
                                `d`.`TransactionId` AS `TransactionId`,
                                `d`.`Phase` AS `Phase`,
                                `d`.`RecordedTime` AS `RecordedTime`,
                                `d`.`Sensor1` AS `Sensor1`,
                                `d`.`Sensor2` AS `Sensor2`,
                                `d`.`Output1` AS `Output1`,
                                `m`.`DateAndTime` AS `DateAndTime`,
                                `transactions`.`Etalon` AS `Etalon`
                            FROM
                                (`leaktestermeasurementsdetails` `d`
                                JOIN `transactions` ON ((`transactions`.`TransactionId` = `d`.`TransactionId`))
                                JOIN `leaktestermeasurements` `m` ON `d`.`TransactionId` = `m`.`TransactionId`);

DROP VIEW IF EXISTS heliummeasurement_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `heliummeasurement_exp` AS
                            SELECT 
                                `heliummeasurement`.`TransactionId` AS `TransactionId`,
                                `heliummeasurement`.`TransactionVer` AS `TransactionVer`,
                                `heliummeasurement`.`MeasurementNumber` AS `MeasurementNumber`,
                                `heliummeasurement`.`ProgramId` AS `ProgramId`,
                                `heliummeasurement`.`Leak` AS `Leak`,
                                `results`.`ResultDescription` AS `ResultDescription`,
                                `heliummeasurement`.`LeakMax` AS `LeakMax`,
                                `heliummeasurement`.`LeakAvg` AS `LeakAvg`,
                                `heliummeasurement`.`LeakMin` AS `LeakMin`,
                                `heliummeasurement`.`LeakLastPoint` AS `LeakLastPoint`,
                                `heliummeasurement`.`LeakNominalScrapMax` AS `LeakNominalScrapMax`,
                                `heliummeasurement`.`LeakNominalImpMin` AS `LeakNominalImpMin`,
                                `heliummeasurement`.`DateAndTime` AS `DateAndTime`,
                                `transactions`.`Etalon` AS `Etalon`
                            FROM
                                (`heliummeasurement`
                                JOIN `transactions` ON ((`transactions`.`TransactionId` = `heliummeasurement`.`TransactionId`))
                                JOIN `results` ON `heliummeasurement`.`ResultId` = `results`.`ResultId`);

DROP VIEW IF EXISTS heliummeasuredpoints_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `heliummeasuredpoints_exp` AS
                            SELECT 
                                `p`.`TransactionId` AS `TransactionId`,
                                `p`.`TransactionVer` AS `TransactionVer`,
                                `p`.`Pos` AS `Pos`,
                                `p`.`Phase` AS `Phase`,
                                `p`.`HeliumLeak` AS `HeliumLeak`,
                                `p`.`HeliumPressurePart` AS `HeliumPressurePart`,
                                `p`.`HeliumAbsolutePressurePart` AS `HeliumAbsolutePressurePart`,
                                `p`.`HeliumAbsolutePressureChamber` AS `HeliumAbsolutePressureChamber`,
                                `p`.`HeliumVacuumPressureChamber` AS `HeliumVacuumPressureChamber`,
                                `p`.`Time` AS `Time`,
                                `m`.`DateAndTime` AS `DateAndTime`,
                                `transactions`.`Etalon` AS `Etalon`
                            FROM
                                (`heliummeasuredpoints` `p`
                                JOIN `transactions` ON ((`transactions`.`TransactionId` = `p`.`TransactionId`))
                                JOIN `heliummeasurement` `m` ON `p`.`MeasurementNumber` = `m`.`MeasurementNumber`);

DROP VIEW IF EXISTS forcepath_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `forcepath_exp` AS
                            SELECT 
                                `t`.`TransactionId` AS `TransactionId`,
                                `td`.`DateAndTime` AS `DateAndTime`,
                                `results`.`ResultDescription` AS `ResultDescription`,
                                `fp`.`ForcePathProgramId` AS `ForcePathProgramId`,
                                `fpp`.`ProgramName` AS `ProgramName`,
                                `t`.`DmcInternal` AS `DmcInternal`,
                                `fp`.`TransactionDetailsId` AS `TransactionDetailsId`,
                                `t`.`Etalon` AS `Etalon`
                            FROM
                                (((((`forcepath` `fp`
                                LEFT JOIN `forcepathwindows` `w` ON ((`fp`.`TransactionDetailsId` = `w`.`TransactionDetailsId`)))
                                JOIN `transactiondetails` `td`)
                                JOIN `results`)
                                JOIN `forcepathprograms` `fpp`)
                                JOIN `transactions` `t`)
                            WHERE
                                ((`fp`.`TransactionDetailsId` = `td`.`TransactionDetailsCounter`)
                                    AND (`td`.`ResultId` = `results`.`ResultId`)
                                    AND (`fp`.`ForcePathProgramId` = `fpp`.`ProgramId`)
                                    AND (`td`.`TransactionId` = `t`.`TransactionId`));

DROP VIEW IF EXISTS forcepathdetails_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `forcepathdetails_exp` AS
                            SELECT 
                                `fd`.`TransactionId` AS `TransactionId`,
                                `fd`.`PointNum` AS `PointNum`,
                                `fd`.`Force_` AS `Force_`,
                                `fd`.`Path` AS `Path`,
                                `fd`.`Time` AS `Time`,
                                `fd`.`TransactionDetailsId` AS `TransactionDetailsId`,
                                `t`.`DateAndTime` AS `DateAndTime`,
                                `t`.`Etalon` AS `Etalon`
                            FROM
                                (`forcepathdetails` `fd`
                                JOIN `transactions` `t`)
                            WHERE
                                (`fd`.`TransactionId` = `t`.`TransactionId`);

DROP VIEW IF EXISTS transactiondetailsprogramresults_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactiondetailsprogramresults_exp` AS
                            SELECT 
                                `transactiondetails`.`TransactionId` AS `TransactionId`,
                                `transactiondetails`.`ProgramId` AS `ProgramId`,
                                `programs`.`ProgramName` AS `ProgramName`,
                                CONCAT(`transactiondetails`.`ProgramId`,
                                        '; ',
                                        `programs`.`ProgramName`) AS `ProgramIdAndName`,
                                CONCAT(`transactiondetails`.`ResultValue`,
                                        '/|',
                                        `results`.`ResultDescription`) AS `ResultProgram`,
                                CONCAT(`transactiondetails`.`DateAndTime`,
                                        '/|',
                                        `transactiondetails`.`ResultValue`,
                                        '/|',
                                        `results`.`ResultDescription`,
                                        '/|',
                                        `transactiondetails`.`MeasurementNumber`) AS `ResultProgramExtended`,
                                `transactiondetails`.`DateAndTime` AS `DateAndTime`
                            FROM
                                ((`transactiondetails`
                                JOIN `programs` ON ((`transactiondetails`.`ProgramId` = `programs`.`ProgramId`)))
                                LEFT JOIN `results` ON ((`transactiondetails`.`ResultId` = `results`.`ResultId`)));

DROP VIEW IF EXISTS transactiondetailsprogramresults_exp_dsc;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactiondetailsprogramresults_exp_dsc` AS
                            SELECT 
                                `transactiondetails`.`TransactionId` AS `TransactionId`,
                                `transactiondetails`.`ProgramId` AS `ProgramId`,
                                `programs`.`ProgramName` AS `ProgramName`,
                                CONCAT(`transactiondetails`.`ProgramId`,
                                        '; ',
                                        `programs`.`ProgramName`) AS `ProgramIdAndName`,
                                CONCAT(CONVERT( REPLACE(`transactiondetails`.`ResultValue`,
                                            '.',
                                            ',') USING UTF8),
                                        '/|',
                                        `results`.`ResultDescription`) AS `ResultProgram`,
                                CONCAT(`transactiondetails`.`DateAndTime`,
                                        '/|',
                                        CONVERT( REPLACE(`transactiondetails`.`ResultValue`,
                                            '.',
                                            ',') USING UTF8),
                                        '/|',
                                        `results`.`ResultDescription`,
                                        '/|',
                                        `transactiondetails`.`MeasurementNumber`) AS `ResultProgramExtended`,
                                `transactiondetails`.`DateAndTime` AS `DateAndTime`
                            FROM
                                ((`transactiondetails`
                                JOIN `programs` ON ((`transactiondetails`.`ProgramId` = `programs`.`ProgramId`)))
                                LEFT JOIN `results` ON ((`transactiondetails`.`ResultId` = `results`.`ResultId`)));

DROP VIEW IF EXISTS transactiondetailsforcepathwindows_exp;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactiondetailsforcepathwindows_exp` AS
                            SELECT 
                                `transactiondetails`.`TransactionId` AS `TransactionId`,
                                `transactiondetails`.`DateAndTime` AS `DateAndTime`,
                                `forcepathwindows`.`WindowNum` AS `WindowNum`,
                                CONCAT('Area: ',
                                        (`forcepathwindows`.`WindowNum` + 1),
                                        ' - Force Min; Force Max') AS `AreaForceMinMax`,
                                CONCAT(CONVERT( COALESCE(`forcepathwindows`.`ForceMin`, '') USING UTF8),
                                        '/|',
                                        CONVERT( COALESCE(`forcepathwindows`.`ForceMax`, '') USING UTF8)) AS `ForceMinMax`
                            FROM
                                (`transactiondetails`
                                JOIN `forcepathwindows` ON ((`forcepathwindows`.`TransactionDetailsId` = `transactiondetails`.`TransactionDetailsCounter`)));

DROP VIEW IF EXISTS transactiondetailsforcepathwindows_exp_dsc;
                        CREATE 
                            ALGORITHM = UNDEFINED 
                            DEFINER = `KNT`@`%` 
                            SQL SECURITY DEFINER
                        VIEW `transactiondetailsforcepathwindows_exp_dsc` AS
                            SELECT 
                                `transactiondetails`.`TransactionId` AS `TransactionId`,
                                `transactiondetails`.`DateAndTime` AS `DateAndTime`,
                                `forcepathwindows`.`WindowNum` AS `WindowNum`,
                                CONCAT('Area: ',
                                        (`forcepathwindows`.`WindowNum` + 1),
                                        ' - Force Min; Force Max') AS `AreaForceMinMax`,
                                CONCAT(CONVERT( REPLACE(COALESCE(`forcepathwindows`.`ForceMin`, ''),
                                            '.',
                                            ',') USING UTF8),
                                        '/|',
                                        CONVERT( REPLACE(COALESCE(`forcepathwindows`.`ForceMax`, ''),
                                            '.',
                                            ',') USING UTF8)) AS `ForceMinMax`
                            FROM
                                (`transactiondetails`
                                JOIN `forcepathwindows` ON ((`forcepathwindows`.`TransactionDetailsId` = `transactiondetails`.`TransactionDetailsCounter`)));
