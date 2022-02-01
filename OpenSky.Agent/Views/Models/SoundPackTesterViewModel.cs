// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SoundPackTesterViewModel.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Views.Models
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Speech.Synthesis;

    using OpenSky.Agent.Controls;
    using OpenSky.Agent.Controls.Models;
    using OpenSky.Agent.MVVM;
    using OpenSky.Agent.Simulator;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Sound pack tester view model.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/12/2021.
    /// </remarks>
    /// <seealso cref="T:OpenSky.Agent.MVVM.ViewModel"/>
    /// -------------------------------------------------------------------------------------------------
    public class SoundPackTesterViewModel : ViewModel
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string flightNumber;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected sound pack.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string selectedSoundPack;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The selected text to speech voice.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private string selectedTextToSpeechVoice;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundPackTesterViewModel"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1304:Specify CultureInfo", Justification = "This is ok here as we want the voices for all cultures")]
        public SoundPackTesterViewModel()
        {
            this.PlaySpeechEventCommand = new Command(this.PlaySpeechEvent);
            this.TestFlightNumberCommand = new Command(this.TestFlightNumber);
            this.ClearSoundPackCommand = new Command(this.ClearSoundPack);

            // Fetch available text to speech voices
            var speech = new SpeechSynthesizer();
            this.TextToSpeechVoices = new List<string>();
            foreach (var voice in speech.GetInstalledVoices())
            {
                this.TextToSpeechVoices.Add(voice.VoiceInfo.Name);
            }

            // Set the default voice in case the user hasn't selected one (yet)
            this.SelectedTextToSpeechVoice = speech.Voice.Name;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.SoundPack))
            {
                this.SelectedSoundPack = Properties.Settings.Default.SoundPack;
            }

            if (!string.IsNullOrEmpty(Properties.Settings.Default.TextToSpeechVoice))
            {
                this.SelectedTextToSpeechVoice = Properties.Settings.Default.TextToSpeechVoice;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the clear sound pack command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command ClearSoundPackCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the flight number.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string FlightNumber
        {
            get => this.flightNumber;

            set
            {
                if (Equals(this.flightNumber, value))
                {
                    return;
                }

                this.flightNumber = value;
                this.NotifyPropertyChanged();
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the play speech event command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command PlaySpeechEventCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected sound pack.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SelectedSoundPack
        {
            get => this.selectedSoundPack;

            set
            {
                if (Equals(this.selectedSoundPack, value))
                {
                    return;
                }

                this.selectedSoundPack = value;
                this.NotifyPropertyChanged();
                SpeechSoundPacks.Instance.SelectedSoundPack = value;
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected text to speech voice.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SelectedTextToSpeechVoice
        {
            get => this.selectedTextToSpeechVoice;

            set
            {
                if (Equals(this.selectedTextToSpeechVoice, value))
                {
                    return;
                }

                this.selectedTextToSpeechVoice = value;
                this.NotifyPropertyChanged();
                try
                {
                    SpeechSoundPacks.Instance.SetSpeechVoice(value);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine("Error setting text-to-speech voice: " + ex);
                    this.ViewReference.ShowMessageBox(new OpenSkyMessageBox(ex, "Error setting text-to-speech voice", ex.Message, ExtendedMessageBoxImage.Error, 30));
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available sound packs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> SoundPacks => SpeechSoundPacks.Instance.SoundPacks.Keys.ToList();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the speech events.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<SpeechEvent> SpeechEvents => Enum.GetValues(typeof(SpeechEvent)).OfType<SpeechEvent>().ToList();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the test flight number command.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Command TestFlightNumberCommand { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available text to speech voices.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public List<string> TextToSpeechVoices { get; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Clears the selected sound pack.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void ClearSoundPack()
        {
            this.SelectedSoundPack = null;
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Plays the specified speech event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// <param name="parameter">
        /// The parameter.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private void PlaySpeechEvent(object parameter)
        {
            if (parameter is SpeechEvent spEvent)
            {
                SpeechSoundPacks.Instance.PlaySpeechEvent(spEvent);
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Test the ReadyForBoarding with flight number META event.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// -------------------------------------------------------------------------------------------------
        private void TestFlightNumber()
        {
            SpeechSoundPacks.Instance.PlaySpeechEvent(SpeechEvent.ReadyForBoarding, true, this.FlightNumber);
        }
    }
}