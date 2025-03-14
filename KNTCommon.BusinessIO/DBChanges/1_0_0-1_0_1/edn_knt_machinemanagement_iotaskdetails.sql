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
-- Table structure for table `iotaskdetails`
--

DROP TABLE IF EXISTS `iotaskdetails`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `iotaskdetails` (
  `IoTaskDetailId` int(11) unsigned NOT NULL AUTO_INCREMENT,
  `IoTaskId` int(11) unsigned NOT NULL,
  `Par1` varchar(50) COLLATE utf8_bin DEFAULT NULL,
  `Par2` varchar(100) COLLATE utf8_bin DEFAULT NULL,
  `par3` varchar(100) COLLATE utf8_bin DEFAULT NULL,
  `Par4` varchar(150) COLLATE utf8_bin DEFAULT NULL,
  `par5` varchar(50) COLLATE utf8_bin DEFAULT NULL,
  `TaskDetailOrder` int(11) DEFAULT '0',
  `Info` varchar(100) COLLATE utf8_bin DEFAULT NULL,
  PRIMARY KEY (`IoTaskDetailId`)
) ENGINE=InnoDB AUTO_INCREMENT=78 DEFAULT CHARSET=utf8 COLLATE=utf8_bin;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `iotaskdetails`
--

LOCK TABLES `iotaskdetails` WRITE;
/*!40000 ALTER TABLE `iotaskdetails` DISABLE KEYS */;
INSERT INTO `iotaskdetails` VALUES (1,1,'transactions','TransactionId','DateAndTime<\'{dayBeforeStr}\'','Archived','copyAlways',1,NULL),
(2,1,'transactiondetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',12,NULL),
(3,1,'transactiondetailslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',11,NULL),
(4,1,'transactionslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',13,NULL),
(5,1,'leaktestermeasurementsdetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',3,NULL),
(6,1,'leaktestermeasurementsdetailslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',2,NULL),
(7,1,'leaktestermeasurements','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',5,NULL),
(8,1,'leaktestermeasurementslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',4,NULL),
(9,1,'heliummeasurement','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',7,NULL),
(10,1,'heliummeasuredpoints','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',6,NULL),
(11,1,'forcepath','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',10,NULL),
(12,1,'forcepathdetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',8,NULL),
(13,1,'forcepathwindows','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',9,NULL),
(14,2,'transactions','TransactionId','','Archived','delete',1,NULL),
(15,2,'transactiondetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',12,NULL),
(16,2,'transactiondetailslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',11,NULL),
(17,2,'transactionslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',13,NULL),
(18,2,'leaktestermeasurementsdetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',3,NULL),
(19,2,'leaktestermeasurementsdetailslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',2,NULL),
(20,2,'LeaktesterMeasurements','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',5,NULL),
(21,2,'LeaktesterMeasurementslog','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',4,NULL),
(22,2,'heliummeasurement','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',7,NULL),
(23,2,'heliummeasuredpoints','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',6,NULL),
(24,2,'forcepath','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',10,NULL),
(25,2,'forcepathdetails','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',8,NULL),
(26,2,'forcepathwindows','TransactionId IN ({transactionIdArrayStr})','TransactionId=transactions.TransactionId',NULL,'move',9,NULL);
/*!40000 ALTER TABLE `iotaskdetails` ENABLE KEYS */;
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
