-- MySQL dump 10.13  Distrib 8.0.34, for Win64 (x86_64)
--
-- Host: localhost    Database: edn_knt_machinemanagement
-- ------------------------------------------------------
-- Server version	5.7.44-log

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `iotasks`
--

DROP TABLE IF EXISTS `iotasks`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iotasks` (
    `IoTaskId` int(10) unsigned NOT NULL AUTO_INCREMENT,
    `IoTaskName` varchar(100) COLLATE utf8_bin DEFAULT NULL,
    `IoTaskType` tinyint(4) DEFAULT '0',
    `IoTaskMode` tinyint(4) DEFAULT '0',
    `Priority` tinyint(4) DEFAULT '0',
    `Par1` varchar(100) COLLATE utf8_bin DEFAULT NULL,
    `TimeCriteria` varchar(50) COLLATE utf8_bin DEFAULT NULL,
    `ExecuteDateAndTime` datetime DEFAULT NULL,
    `Status` int(11) DEFAULT '0',
    `Info` varchar(500) COLLATE utf8_bin DEFAULT NULL,
    PRIMARY KEY (`IoTaskId`)
) ENGINE=InnoDB AUTO_INCREMENT=9 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iotasks`
--

LOCK TABLES `iotasks` WRITE;
/*!40000 ALTER TABLE `iotasks` DISABLE KEYS */;
INSERT INTO `iotasks` VALUES 
('1','DB Backup','1','1','1',NULL,'NextMonth;Day=1;Time=01:30',NULL,'100',NULL),
('2','DB Restore','2','2','3',NULL,'',NULL,'100',NULL),
('3','Export Excel Transactions','3','2','7','c:/Programs/Archive/TransactionsExport{DateAndTime}.xlsx','',NULL,'100',NULL),
('4','DB Backup on Demand','1','2','4',NULL,'AddMinutes=5',NULL,'100',NULL),
('5','Export Excel Transactions Cycled','3','1','8','c:/Programs/Archive/TransactionsExport{DateAndTime}.xlsx','AddDays=1;Time=02:30',NULL,'100',NULL),
('6','DB Export','4','2','2','c:/Programs/Archive/DbDumpAll{DateAndTime}.sql;zip',NULL,NULL,'100',NULL),
('7','DB Export Single All','4','99','2','c:/Programs/Archive/DbDumpAll{DateAndTime}.sql',NULL,NULL,'100',NULL),
('8','DB Export Single Number','4','99','2','c:/Programs/Archive/DbDumpAll{DateAndTime}.sql',NULL,NULL,'100',NULL)
;
/*!40000 ALTER TABLE `iotasks` ENABLE KEYS */;
UNLOCK TABLES;
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-03-13 10:51:28
