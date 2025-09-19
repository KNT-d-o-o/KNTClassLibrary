using Sharp7;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace KNTPlc.S7
{
    public static class S7ErrorHelper
    {
        public static string ErrorText(int error)
        {
            switch (error)
            {
                case 0: return "OK";
                case S7Consts.errTCPSocketCreation: return "SYS : Error creating the Socket";
                case S7Consts.errTCPConnectionTimeout: return "TCP : Connection Timeout";
                case S7Consts.errTCPConnectionFailed: return "TCP : Connection Error";
                case S7Consts.errTCPReceiveTimeout: return "TCP : Data receive Timeout";
                case S7Consts.errTCPDataReceive: return "TCP : Error receiving Data";
                case S7Consts.errTCPSendTimeout: return "TCP : Data send Timeout";
                case S7Consts.errTCPDataSend: return "TCP : Error sending Data";
                case S7Consts.errTCPConnectionReset: return "TCP : Connection reset by the Peer";
                case S7Consts.errTCPNotConnected: return "CLI : Client not connected";
                case S7Consts.errTCPUnreachableHost: return "TCP : Unreachable host";
                case S7Consts.errIsoConnect: return "ISO : Connection Error";
                case S7Consts.errIsoInvalidPDU: return "ISO : Invalid PDU received";
                case S7Consts.errIsoInvalidDataSize: return "ISO : Invalid Buffer passed to Send/Receive";
                case S7Consts.errCliNegotiatingPDU: return "CLI : Error in PDU negotiation";
                case S7Consts.errCliInvalidParams: return "CLI : Invalid param(s) supplied";
                case S7Consts.errCliJobPending: return "CLI : Job pending";
                case S7Consts.errCliTooManyItems: return "CLI : Too many items (>20) in multi read/write";
                case S7Consts.errCliInvalidWordLen: return "CLI : Invalid WordLength";
                case S7Consts.errCliPartialDataWritten: return "CLI : Partial data written";
                case S7Consts.errCliSizeOverPDU: return "CPU : Total data exceeds the PDU size";
                case S7Consts.errCliInvalidPlcAnswer: return "CLI : Invalid CPU answer";
                case S7Consts.errCliAddressOutOfRange: return "CPU : Address out of range";
                case S7Consts.errCliInvalidTransportSize: return "CPU : Invalid Transport size";
                case S7Consts.errCliWriteDataSizeMismatch: return "CPU : Data size mismatch";
                case S7Consts.errCliItemNotAvailable: return "CPU : Item not available";
                case S7Consts.errCliInvalidValue: return "CPU : Invalid value supplied";
                case S7Consts.errCliCannotStartPLC: return "CPU : Cannot start PLC";
                case S7Consts.errCliAlreadyRun: return "CPU : PLC already RUN";
                case S7Consts.errCliCannotStopPLC: return "CPU : Cannot stop PLC";
                case S7Consts.errCliCannotCopyRamToRom: return "CPU : Cannot copy RAM to ROM";
                case S7Consts.errCliCannotCompress: return "CPU : Cannot compress";
                case S7Consts.errCliAlreadyStop: return "CPU : PLC already STOP";
                case S7Consts.errCliFunNotAvailable: return "CPU : Function not available";
                case S7Consts.errCliUploadSequenceFailed: return "CPU : Upload sequence failed";
                case S7Consts.errCliInvalidDataSizeRecvd: return "CLI : Invalid data size received";
                case S7Consts.errCliInvalidBlockType: return "CLI : Invalid block type";
                case S7Consts.errCliInvalidBlockNumber: return "CLI : Invalid block number";
                case S7Consts.errCliInvalidBlockSize: return "CLI : Invalid block size";
                case S7Consts.errCliNeedPassword: return "CPU : Function not authorized for current protection level";
                case S7Consts.errCliInvalidPassword: return "CPU : Invalid password";
                case S7Consts.errCliNoPasswordToSetOrClear: return "CPU : No password to set or clear";
                case S7Consts.errCliJobTimeout: return "CLI : Job Timeout";
                case S7Consts.errCliFunctionRefused: return "CLI : Function refused by CPU (Unknown error)";
                case S7Consts.errCliPartialDataRead: return "CLI : Partial data read";
                case S7Consts.errCliBufferTooSmall: return "CLI : The buffer supplied is too small";
                case S7Consts.errCliDestroying: return "CLI : Cannot perform (destroying)";
                case S7Consts.errCliInvalidParamNumber: return "CLI : Invalid Param Number";
                case S7Consts.errCliCannotChangeParam: return "CLI : Cannot change this param now";
                case S7Consts.errCliFunctionNotImplemented: return "CLI : Function not implemented";
                default: return "CLI : Unknown error (0x" + Convert.ToString(error, 16) + ")";
            }
        }

    }
}
