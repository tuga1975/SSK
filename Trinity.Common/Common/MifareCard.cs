using System;
using System.Diagnostics;
using PCSC;
using PCSC.Iso7816;

namespace Trinity.Common.Common
{
    public class MifareCard
    {
        private const byte CUSTOM_CLA = 0xFF;
        private readonly IIsoReader _isoReader;

        public MifareCard(IIsoReader isoReader)
        {
            if (isoReader == null)
            {
                throw new ArgumentNullException(nameof(isoReader));
            }
            _isoReader = isoReader;
        }

        public bool LoadKey(KeyStructure keyStructure, byte keyNumber, byte[] key)
        {
            var loadKeyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.ExternalAuthenticate,
                P1 = (byte)keyStructure,
                P2 = keyNumber,
                Data = key
            };

            Debug.WriteLine("Load Authentication Keys: {0}", BitConverter.ToString(loadKeyCmd.ToArray()));
            var response = _isoReader.Transmit(loadKeyCmd);
            Debug.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

            return IsSuccess(response);
        }


        public bool Authenticate(byte msb, byte lsb, KeyType keyType, byte keyNumber)
        {
            var authBlock = new GeneralAuthenticate
            {
                MSB = msb,
                LSB = lsb,
                KeyNumber = keyNumber,
                KeyType = keyType
            };

            var authKeyCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.InternalAuthenticate,
                P1 = 0x00,
                P2 = 0x00,
                Data = authBlock.ToArray()
            };

            Debug.WriteLine("General Authenticate: {0}", BitConverter.ToString(authKeyCmd.ToArray()));
            var response = _isoReader.Transmit(authKeyCmd);
            Debug.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

            return (response.SW1 == 0x90) && (response.SW2 == 0x00);
        }

        public byte[] ReadBinary(byte msb, byte lsb, int size)
        {
            unchecked
            {
                var readBinaryCmd = new CommandApdu(IsoCase.Case2Short, SCardProtocol.Any)
                {
                    CLA = CUSTOM_CLA,
                    Instruction = InstructionCode.ReadBinary,
                    P1 = msb,
                    P2 = lsb,
                    Le = size
                };

                Debug.WriteLine("Read Binary (before update): {0}", BitConverter.ToString(readBinaryCmd.ToArray()));
                var response = _isoReader.Transmit(readBinaryCmd);
                //Debug.WriteLine("SW1 SW2 = {0:X2} {1:X2} Data: {2}",
                    //response.SW1,
                    //response.SW2,
                    //BitConverter.ToString(response.GetData()));

                return IsSuccess(response)
                    ? response.GetData() ?? new byte[0]
                    : null;
            }
        }

        public bool UpdateBinary(byte msb, byte lsb, byte[] data)
        {
            var updateBinaryCmd = new CommandApdu(IsoCase.Case3Short, SCardProtocol.Any)
            {
                CLA = CUSTOM_CLA,
                Instruction = InstructionCode.UpdateBinary,
                P1 = msb,
                P2 = lsb,
                Data = data
            };

            Debug.WriteLine("Update Binary: {0}", BitConverter.ToString(updateBinaryCmd.ToArray()));
            var response = _isoReader.Transmit(updateBinaryCmd);
            Debug.WriteLine("SW1 SW2 = {0:X2} {1:X2}", response.SW1, response.SW2);

            return IsSuccess(response);
        }

        private static bool IsSuccess(Response response) =>
            (response.SW1 == (byte)SW1Code.Normal) && (response.SW2 == 0x00);
    }

    public enum KeyStructure : byte
    {
        VolatileMemory = 0x00,
        NonVolatileMemory = 0x20
    }

    public enum KeyType : byte
    {
        KeyA = 0x60,
        KeyB = 0x61
    }

    public class GeneralAuthenticate
    {
        public byte Version { get; } = 0x01;

        public byte MSB { get; set; }
        public byte LSB { get; set; }
        public KeyType KeyType { get; set; }
        public byte KeyNumber { get; set; }

        public byte[] ToArray()
        {
            return new[] { Version, MSB, LSB, (byte)KeyType, KeyNumber };
        }
    }

    public class MifareCard_Block
    {
        public int Sector_Index { get; set; }
        /// <summary>
        /// index of block within sector in Decimal (using for display)
        /// </summary>
        public int Block_Index_Dec { get; set; }
        /// <summary>
        /// index of block within card in Hex (using for card command)
        /// </summary>
        public byte Block_Index_Hex { get; set; }
        public EnumBlockTypes BlockType { get; set; }
    }

    public enum EnumBlockTypes
    {
        ManufacturerBlock = 0,
        Data = 1,
        SectorTrailer = 2
    }

    public enum EnumSmartCardTypes
    {
        MifareClassic1K = 1,
        MifareClassic4K = 2
    }
}
