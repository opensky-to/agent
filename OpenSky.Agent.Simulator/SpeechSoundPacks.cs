// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SpeechSoundPacks.cs" company="OpenSky">
// OpenSky project 2021-2022
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace OpenSky.Agent.Simulator
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Media;
    using System.Speech.Synthesis;
    using System.Text.RegularExpressions;
    using System.Threading;

    using OpenSky.Agent.Simulator.Tools;

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Speech sound packs manager.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public class SpeechSoundPacks
    {
        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The single static instance.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public static SpeechSoundPacks Instance { get; private set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The random generator.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly Random random = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// The speech synthesizer.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        private readonly SpeechSynthesizer speech;

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes the speech sound packs.
        /// </summary>
        /// <remarks>
        /// sushi.at, 31/01/2022.
        /// </remarks>
        /// <param name="selectedSoundPack">
        /// The selected sound pack.
        /// </param>
        /// <param name="textToSpeechVoice">
        /// The text to speech voice.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public static void InitializeSpeechSoundPacks(string selectedSoundPack, string textToSpeechVoice)
        {
            Instance = new SpeechSoundPacks(selectedSoundPack, textToSpeechVoice);
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Initializes a new instance of the <see cref="SpeechSoundPacks"/> class.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// <param name="selectedSoundPack">
        /// The selected sound pack.
        /// </param>
        /// <param name="textToSpeechVoice">
        /// The text to speech voice.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        private SpeechSoundPacks(string selectedSoundPack, string textToSpeechVoice)
        {
            try
            {
                // Load the initial sound pack setting
                this.SelectedSoundPack = selectedSoundPack;

                // Initialize the speech synthesizer
                this.speech = new SpeechSynthesizer();
                if (!string.IsNullOrEmpty(textToSpeechVoice))
                {
                    try
                    {
                        this.speech.SelectVoice(textToSpeechVoice);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("Error setting text-to-speech voice from settings: " + ex);
                    }
                }

                // Scan sound pack folder for recognized wav files
                foreach (var soundPackDirectory in Directory.EnumerateDirectories(".\\SoundPacks"))
                {
                    var packName = soundPackDirectory.Split('\\').Last();
                    var packFiles = Directory.GetFiles(soundPackDirectory);
                    var packDictionary = new Dictionary<SpeechEvent, List<string>>();
                    foreach (SpeechEvent spEvent in Enum.GetValues(typeof(SpeechEvent)))
                    {
                        if (spEvent != SpeechEvent.ReadyForBoarding)
                        {
                            var eventConfig = spEvent.GetStringValue();
                            if (eventConfig?.Contains("|") == true)
                            {
                                var fileMask = eventConfig.Split('|')[0];
                                foreach (var packFile in packFiles)
                                {
                                    if (Regex.IsMatch(packFile, fileMask))
                                    {
                                        if (!packDictionary.ContainsKey(spEvent))
                                        {
                                            packDictionary.Add(spEvent, new List<string>());
                                        }

                                        packDictionary[spEvent].Add(packFile);
                                    }
                                }
                            }
                        }
                    }

                    // Check for special ReadyForBoarding meta event conditions
                    if (packDictionary.ContainsKey(SpeechEvent.ReadyForBoardingBeginning) && packDictionary.ContainsKey(SpeechEvent.ReadyForBoardingEnd) && packDictionary.ContainsKey(SpeechEvent.Number1) &&
                        packDictionary.ContainsKey(SpeechEvent.Number2) && packDictionary.ContainsKey(SpeechEvent.Number3) && packDictionary.ContainsKey(SpeechEvent.Number4) && packDictionary.ContainsKey(SpeechEvent.Number5) &&
                        packDictionary.ContainsKey(SpeechEvent.Number6) && packDictionary.ContainsKey(SpeechEvent.Number7) && packDictionary.ContainsKey(SpeechEvent.Number8) && packDictionary.ContainsKey(SpeechEvent.Number9) &&
                        packDictionary.ContainsKey(SpeechEvent.Number0))
                    {
                        packDictionary.Add(SpeechEvent.ReadyForBoarding, null);
                    }

                    // Does the pack contain any recognized files?
                    if (packDictionary.Count > 0)
                    {
                        this.SoundPacks.Add(packName, packDictionary);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error initializing sound packs: {ex}");
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets or sets the selected sound pack.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public string SelectedSoundPack { get; set; }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Gets the available sound packs.
        /// </summary>
        /// -------------------------------------------------------------------------------------------------
        public Dictionary<string, Dictionary<SpeechEvent, List<string>>> SoundPacks { get; } = new();

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Play the specified speech event from the selected or randomized sound pack, or use TTS as a
        /// fallback.
        /// </summary>
        /// <remarks>
        /// sushi.at, 24/12/2021.
        /// </remarks>
        /// <param name="spEvent">
        /// The speech event to play.
        /// </param>
        /// <param name="async">
        /// (Optional) True to asynchronously play the audio, false to play in synchronized.
        /// </param>
        /// <param name="flightNumber">
        /// (Optional) The flight number (only used for ReadyForBoarding META event).
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void PlaySpeechEvent(SpeechEvent spEvent, bool async = true, string flightNumber = null)
        {
            // Special META event for ready for boarding
            if (spEvent == SpeechEvent.ReadyForBoarding)
            {
                if (string.IsNullOrEmpty(flightNumber))
                {
                    return;
                }

                var flightNumberDigits = flightNumber.ToCharArray().Where(char.IsDigit).ToArray();
                if (flightNumberDigits.Length == 0)
                {
                    return;
                }

                if (string.IsNullOrEmpty(this.SelectedSoundPack) || !this.SoundPacks.ContainsKey(this.SelectedSoundPack))
                {
                    // Random mode
                    var validPacks = this.SoundPacks.Where(p => p.Value.ContainsKey(spEvent)).ToList();
                    if (validPacks.Count > 0)
                    {
                        var randomPack = validPacks[this.random.Next(validPacks.Count)].Key;
                        try
                        {
                            var beginningSoundFile = this.SoundPacks[randomPack][SpeechEvent.ReadyForBoardingBeginning][this.random.Next(this.SoundPacks[randomPack][SpeechEvent.ReadyForBoardingBeginning].Count)];
                            var soundPlayer = new SoundPlayer(beginningSoundFile);
                            soundPlayer.PlaySync();

                            foreach (var digit in flightNumberDigits)
                            {
                                var numberEvent = (SpeechEvent)Enum.Parse(typeof(SpeechEvent), $"Number{digit}");
                                var numberSoundFile = this.SoundPacks[randomPack][numberEvent][this.random.Next(this.SoundPacks[randomPack][numberEvent].Count)];
                                soundPlayer.SoundLocation = numberSoundFile;
                                soundPlayer.PlaySync();
                                Thread.Sleep(200);
                            }

                            Thread.Sleep(200);

                            var endSoundFile = this.SoundPacks[randomPack][SpeechEvent.ReadyForBoardingEnd][this.random.Next(this.SoundPacks[randomPack][SpeechEvent.ReadyForBoardingEnd].Count)];
                            soundPlayer.SoundLocation = endSoundFile;
                            soundPlayer.PlaySync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error playing sound pack file: {ex}");

                            // Don't fall back to TTS here, since we don't know where it died and we don't want to repeat a partial message
                        }
                    }
                    else
                    {
                        // No sound pack has that file, use TTS as a fallback
                        var lineToSpeek = flightNumberDigits.Aggregate("OpenSky flight number ", (current, digit) => current + $"{digit} ");
                        lineToSpeek += "is now ready for boarding.";
                        this.speech.SpeakAsync(lineToSpeek);
                    }
                }
                else
                {
                    // Specific sound pack mode
                    if (this.SoundPacks[this.SelectedSoundPack].ContainsKey(spEvent))
                    {
                        try
                        {
                            var beginningSoundFile = this.SoundPacks[this.SelectedSoundPack][SpeechEvent.ReadyForBoardingBeginning][this.random.Next(this.SoundPacks[this.SelectedSoundPack][SpeechEvent.ReadyForBoardingBeginning].Count)];
                            var soundPlayer = new SoundPlayer(beginningSoundFile);
                            soundPlayer.PlaySync();

                            foreach (var digit in flightNumberDigits)
                            {
                                var numberEvent = (SpeechEvent)Enum.Parse(typeof(SpeechEvent), $"Number{digit}");
                                var numberSoundFile = this.SoundPacks[this.SelectedSoundPack][numberEvent][this.random.Next(this.SoundPacks[this.SelectedSoundPack][numberEvent].Count)];
                                soundPlayer.SoundLocation = numberSoundFile;
                                soundPlayer.PlaySync();
                                Thread.Sleep(200);
                            }

                            Thread.Sleep(200);

                            var endSoundFile = this.SoundPacks[this.SelectedSoundPack][SpeechEvent.ReadyForBoardingEnd][this.random.Next(this.SoundPacks[this.SelectedSoundPack][SpeechEvent.ReadyForBoardingEnd].Count)];
                            soundPlayer.SoundLocation = endSoundFile;
                            soundPlayer.PlaySync();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"Error playing sound pack file: {ex}");

                            // Don't fall back to TTS here, since we don't know where it died and we don't want to repeat a partial message
                        }
                    }
                    else
                    {
                        // Sound pack doesn't have that file, use TTS as a fallback
                        var lineToSpeek = flightNumberDigits.Aggregate("OpenSky flight number ", (current, digit) => current + $"{digit} ");
                        lineToSpeek += "is now ready for boarding.";
                        this.speech.SpeakAsync(lineToSpeek);
                    }
                }

                return;
            }

            if (string.IsNullOrEmpty(this.SelectedSoundPack) || !this.SoundPacks.ContainsKey(this.SelectedSoundPack))
            {
                // Random mode
                var validPacks = this.SoundPacks.Where(p => p.Value.ContainsKey(spEvent)).ToList();
                if (validPacks.Count > 0)
                {
                    var randomPack = validPacks[this.random.Next(validPacks.Count)].Key;
                    try
                    {
                        var soundFile = this.SoundPacks[randomPack][spEvent][this.random.Next(this.SoundPacks[randomPack][spEvent].Count)];
                        var soundPlayer = new SoundPlayer(soundFile);
                        if (async)
                        {
                            soundPlayer.Play();
                        }
                        else
                        {
                            soundPlayer.PlaySync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error playing sound pack file: {ex}");

                        // Audio file failed, use TTS as a fallback
                        var lineToSpeek = spEvent.GetStringValue()?.Contains("|") == true ? spEvent.GetStringValue()?.Split('|')[1] : string.Empty;
                        if (!string.IsNullOrEmpty(lineToSpeek))
                        {
                            if (async)
                            {
                                this.speech.SpeakAsync(lineToSpeek);
                            }
                            else
                            {
                                this.speech.Speak(lineToSpeek);
                            }
                        }
                    }
                }
                else
                {
                    // No sound pack has that file, use TTS as a fallback
                    var lineToSpeek = spEvent.GetStringValue()?.Contains("|") == true ? spEvent.GetStringValue()?.Split('|')[1] : string.Empty;
                    if (!string.IsNullOrEmpty(lineToSpeek))
                    {
                        if (async)
                        {
                            this.speech.SpeakAsync(lineToSpeek);
                        }
                        else
                        {
                            this.speech.Speak(lineToSpeek);
                        }
                    }
                }
            }
            else
            {
                // Specific sound pack mode
                if (this.SoundPacks[this.SelectedSoundPack].ContainsKey(spEvent))
                {
                    try
                    {
                        var soundFile = this.SoundPacks[this.SelectedSoundPack][spEvent][this.random.Next(this.SoundPacks[this.SelectedSoundPack][spEvent].Count)];
                        var soundPlayer = new SoundPlayer(soundFile);
                        if (async)
                        {
                            soundPlayer.Play();
                        }
                        else
                        {
                            soundPlayer.PlaySync();
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"Error playing sound pack file: {ex}");

                        // Audio file failed, use TTS as a fallback
                        var lineToSpeek = spEvent.GetStringValue()?.Contains("|") == true ? spEvent.GetStringValue()?.Split('|')[1] : string.Empty;
                        if (!string.IsNullOrEmpty(lineToSpeek))
                        {
                            if (async)
                            {
                                this.speech.SpeakAsync(lineToSpeek);
                            }
                            else
                            {
                                this.speech.Speak(lineToSpeek);
                            }
                        }
                    }
                }
                else
                {
                    // Sound pack doesn't have that file, use TTS as a fallback
                    var lineToSpeek = spEvent.GetStringValue()?.Contains("|") == true ? spEvent.GetStringValue()?.Split('|')[1] : string.Empty;
                    if (!string.IsNullOrEmpty(lineToSpeek))
                    {
                        if (async)
                        {
                            this.speech.SpeakAsync(lineToSpeek);
                        }
                        else
                        {
                            this.speech.Speak(lineToSpeek);
                        }
                    }
                }
            }
        }

        /// -------------------------------------------------------------------------------------------------
        /// <summary>
        /// Sets the text-to-speech voice.
        /// </summary>
        /// <remarks>
        /// sushi.at, 02/04/2021.
        /// </remarks>
        /// <param name="voiceName">
        /// Name of the voice.
        /// </param>
        /// -------------------------------------------------------------------------------------------------
        public void SetSpeechVoice(string voiceName)
        {
            this.speech.SelectVoice(voiceName);
        }
    }

    /// -------------------------------------------------------------------------------------------------
    /// <summary>
    /// Speech events.
    /// </summary>
    /// <remarks>
    /// sushi.at, 24/12/2021.
    /// </remarks>
    /// -------------------------------------------------------------------------------------------------
    public enum SpeechEvent
    {
#pragma warning disable CS1591

        // ReSharper disable StringLiteralTypo
        [StringValue("|META EVENT WITH FLIGHT NUMBER PARAMETER")]
        ReadyForBoarding,

        [StringValue(".*\\\\ready_for_boarding_beginning.*\\.wav|OpenSky flight number")]
        ReadyForBoardingBeginning,

        [StringValue(".*\\\\ready_for_boarding_end.*\\.wav|is now ready for boarding.")]
        ReadyForBoardingEnd,

        [StringValue(".*\\\\tracking_started.*\\.wav|Flight tracking started.")]
        TrackingStarted,

        [StringValue(".*\\\\tracking_gh_started.*\\.wav|Tracking started, however ground handling is in progress. You may want to use this time to setup your aircraft. But do not start the engines or push back.")]
        TrackingStartedGroundHandling,

        [StringValue(".*\\\\fuel_loading.*\\.wav|Ground to cockpit: Fuel loading is complete.")]
        FuelLoadingComplete,

        [StringValue(".*\\\\payload_loadsheet.*\\.wav|Ground to cockpit: Payload is set. Load sheet will be there in a second.")]
        PayloadSetLoadsheet,

        [StringValue(".*\\\\saved_and_paused.*\\.wav|Flight saved and paused.")]
        FlightSavedPaused,

        [StringValue(".*\\\\tracking_resumed.*\\.wav|Flight tracking resumed.")]
        TrackingResumed,

        [StringValue(".*\\\\push_start.*\\.wav|Ground to cockpit: We are ready for push and start.")]
        ReadyPushStart,

        [StringValue(".*\\\\complete_submitting.*\\.wav|Flight is complete. Submitting final report to OpenSky server.")]
        FlightCompleteSubmitting,

        [StringValue(".*\\\\welcome.*\\.wav|Welcome to OpenSky, now buckle up and stow those tray tables.")]
        WelcomeOpenSky,

        [StringValue(".*\\\\engine_off_not_airborne.*\\.wav|Engine was turned off, but the plane was never airborne, aborting tracking session.")]
        EngineOffNeverAirborne,

        [StringValue(".*\\\\lostsim_saving_stop.*\\.wav|Lost connection to the simulator, saving flight and stopping tracking session.")]
        LostSimSavingStopTracking,

        [StringValue(".*\\\\abort_aircraft.*\\.wav|Tracking aborted, aircraft type was changed.")]
        AbortedAircraftType,

        [StringValue(".*\\\\abort_payload.*\\.wav|Tracking aborted due to change in payload.")]
        AbortedPayloadChange,

        [StringValue(".*\\\\abort_slew.*\\.wav|Tracking aborted due to slew detection.")]
        AbortedSlew,

        [StringValue(".*\\\\abort_teleport.*\\.wav|Tracking aborted due to teleport detection.")]
        AbortedTeleport,

        [StringValue(".*\\\\abort_sim_main.*\\.wav|Tracking aborted, simulator returned to main menu.")]
        AbortedSimMainMenu,

        [StringValue(".*\\\\abort_fuel.*\\.wav|Tracking aborted, fuel increased.")]
        AbortedFuelIncreased,

        [StringValue(".*\\\\abort_engines_gh.*\\.wav|Tracking aborted, you must not start your engines while ground handling isn't complete.")]
        AbortedEnginesGroundHandling,

        [StringValue(".*\\\\abort_pushback_gh.*\\.wav|Tracking aborted, you must not start pushback while ground handling isn't complete.")]
        AbortedPushbackGroundHandling,

        [StringValue(".*\\\\abort_crash_detect.*\\.wav|Tracking aborted, crash detection turned off.")]
        AbortedCrashDetectOff,

        [StringValue(".*\\\\abort_unlimited_fuel.*\\.wav|Tracking aborted, unlimited fuel turned on.")]
        AbortedUnlimitedFuel,

        [StringValue(".*\\\\abort_time_back.*\\.wav|Tracking aborted, time moved backwards.")]
        AbortedTimeBackwards,

        [StringValue(".*\\\\abort_time_changed.*\\.wav|Tracking aborted, time changed in simulator.")]
        AbortedTimeChanged,

        [StringValue(".*\\\\number1.*\\.wav|1")]
        Number1,

        [StringValue(".*\\\\number2.*\\.wav|2")]
        Number2,

        [StringValue(".*\\\\number3.*\\.wav|3")]
        Number3,

        [StringValue(".*\\\\number4.*\\.wav|4")]
        Number4,

        [StringValue(".*\\\\number5.*\\.wav|5")]
        Number5,

        [StringValue(".*\\\\number6.*\\.wav|6")]
        Number6,

        [StringValue(".*\\\\number7.*\\.wav|7")]
        Number7,

        [StringValue(".*\\\\number8.*\\.wav|8")]
        Number8,

        [StringValue(".*\\\\number9.*\\.wav|9")]
        Number9,

        [StringValue(".*\\\\number0.*\\.wav|0")]
        Number0

        // ReSharper restore StringLiteralTypo
#pragma warning restore CS1591
    }
}