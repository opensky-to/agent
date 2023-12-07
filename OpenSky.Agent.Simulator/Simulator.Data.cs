// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Simulator.Data.cs" company="OpenSky">
// OpenSky project 2021-2023
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using OpenSky.Agent.Simulator.Models;

    /// -------------------------------------------------------------------------------------------------
    /// <content>
    /// Simulator interface - data structures.
    /// </content>
    /// -------------------------------------------------------------------------------------------------
    public partial class Simulator
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last aircraft identity data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private AircraftIdentity aircraftIdentity;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last fuel tanks data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private FuelTanks fuelTanks;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last landing analysis data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private LandingAnalysis landingAnalysis;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last payload stations data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PayloadStations payloadStations;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last primary tracking data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private PrimaryTracking primaryTracking;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last secondary tracking data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SecondaryTracking secondaryTracking;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last slew aircraft into position data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private SlewAircraftIntoPosition slewAircraftIntoPosition;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The last weight and balance data received from the simulator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private WeightAndBalance weightAndBalance;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the aircraft identity data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public AircraftIdentity AircraftIdentity
        {
            get => this.aircraftIdentity;

            set
            {
                if (Equals(this.aircraftIdentity, value))
                {
                    return;
                }

                this.aircraftIdentity = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the fuel tanks data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public FuelTanks FuelTanks
        {
            get => this.fuelTanks;

            set
            {
                if (Equals(this.fuelTanks, value))
                {
                    return;
                }

                this.fuelTanks = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the landing analysis data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public LandingAnalysis LandingAnalysis
        {
            get => this.landingAnalysis;

            set
            {
                if (Equals(this.landingAnalysis, value))
                {
                    return;
                }

                this.landingAnalysis = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the payload stations data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PayloadStations PayloadStations
        {
            get => this.payloadStations;

            set
            {
                if (Equals(this.payloadStations, value))
                {
                    return;
                }

                this.payloadStations = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the primary tracking data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public PrimaryTracking PrimaryTracking
        {
            get => this.primaryTracking;

            set
            {
                if (Equals(this.primaryTracking, value))
                {
                    return;
                }

                this.primaryTracking = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the secondary tracking data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SecondaryTracking SecondaryTracking
        {
            get => this.secondaryTracking;

            set
            {
                if (Equals(this.secondaryTracking, value))
                {
                    return;
                }

                this.secondaryTracking = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the slew aircraft into position data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public SlewAircraftIntoPosition SlewAircraftIntoPosition
        {
            get => this.slewAircraftIntoPosition;

            protected set
            {
                if (Equals(this.slewAircraftIntoPosition, value))
                {
                    return;
                }

                this.slewAircraftIntoPosition = value;
                this.OnPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the weight and balance data.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public WeightAndBalance WeightAndBalance
        {
            get => this.weightAndBalance;

            set
            {
                if (Equals(this.weightAndBalance, value))
                {
                    return;
                }

                this.weightAndBalance = value;
                this.OnPropertyChanged();
            }
        }
    }
}