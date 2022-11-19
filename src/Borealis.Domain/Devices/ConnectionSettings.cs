using System;
using System.Device.Gpio;
using System.Device.Spi;
using System.Linq;



namespace Borealis.Domain.Devices;


public class ConnectionSettings
{
    public SpiBusSettings? Spi { get; set; } = new SpiBusSettings();



    public class SpiBusSettings
    {
        /// <summary>
        /// The bus ID the device is connected to.
        /// </summary>
        public int BusId { get; set; }

        /// <summary>
        /// The chip select line used on the bus.
        /// </summary>
        public int ChipSelectLine { get; set; }

        /// <summary>
        /// The SPI mode being used.
        /// </summary>
        public SpiMode Mode { get; set; } = SpiMode.Mode0;

        /// <summary>
        /// The length of the data to be transfered.
        /// </summary>
        public int DataBitLength { get; set; } = 8; // 1 byte

        /// <summary>
        /// The frequency in which the data will be transferred.
        /// </summary>
        public int ClockFrequency { get; set; } = 2_400_000; //  2.4 MHz

        /// <summary>
        /// Specifies order in which bits are transferred first on the SPI bus.
        /// </summary>
        public DataFlow DataFlow { get; set; } = DataFlow.MsbFirst;

        /// <summary>
        /// Specifies which value on chip select pin means "active".
        /// </summary>
        public PinValue ChipSelectLineActiveState { get; set; } = PinValue.Low;
    }
}