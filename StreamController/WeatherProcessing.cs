using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using uk.JohnCook.dotnet.OBSWebSocketLibrary;
using uk.JohnCook.dotnet.OBSWebSocketLibrary.ObsRequests;
using uk.JohnCook.dotnet.StreamController.Models;

namespace uk.JohnCook.dotnet.StreamController
{
    public class WeatherProcessing : INotifyPropertyChanged, IDisposable
    {
        #region Instantiation

        #region Properties and Variables

        private readonly ChronoTimer chronoTimer = ChronoTimer.Instance;
        private string TimezoneString { get; set; } = String.Empty;
        private string PreviousLocalClockWeatherAttrib1 { get; set; } = String.Empty;
        private string PreviousLocalClockWeatherAttrib2 { get; set; } = String.Empty;
        private int CurrentLocalClockWeatherRecord { get; set; }
        private bool FirstWeatherCycle { get; set; } = true;
        private readonly HttpClient httpClient = new HttpClient();
        private Models.WeatherData WeatherDataCollection { get; set; }
        public bool ClockUpdatesEnabled { get; set; }
        public bool WeatherUpdatesEnabled { get; set; }
        private bool disposedValue;

        public event PropertyChangedEventHandler PropertyChanged;

        public void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion

        public WeatherProcessing()
        {
            UpdateTimezoneString();
            chronoTimer.MinuteChanged += FetchWeatherData;
            chronoTimer.MinuteChanged += UpdateLocalClock;
            chronoTimer.MinuteChanged += UpdateLocalWeather;
            chronoTimer.SecondChanged += UpdateLocalWeather;
            FetchWeatherData(this, DateTime.UtcNow);
        }

        #endregion

        #region Reset the weather location cycler

        public void ResetWeatherCycle()
        {
            if (ObsWebsocketConnection.Instance.Client.CanSend)
            {
                FirstWeatherCycle = true;
                NotifyPropertyChanged(nameof(FirstWeatherCycle));
                CurrentLocalClockWeatherRecord = 0;
                PreviousLocalClockWeatherAttrib1 = String.Empty;
                NotifyPropertyChanged(nameof(PreviousLocalClockWeatherAttrib1));
                PreviousLocalClockWeatherAttrib2 = String.Empty;
                NotifyPropertyChanged(nameof(PreviousLocalClockWeatherAttrib2));
                return;
            }
        }

        #endregion

        #region Update abbreviation for current timezone

        /// <summary>
        /// Update the current timezone abbreviation
        /// </summary>
        public void UpdateTimezoneString()
        {
            if (Preferences.Default.local_timezone_use_utc_offset)
            {
                TimeSpan utcOffset = DateTimeOffset.Now.Offset;
                int utcHourOffset = utcOffset.Hours;
                string utcHourOffsetString = utcHourOffset > -1 ? $"+{utcHourOffset}" : utcHourOffset.ToString(CultureInfo.InvariantCulture);
                int utcMinuteOffset = utcOffset.Minutes;
                string utcMinuteOffsetString = utcMinuteOffset > 0 ? $":{utcMinuteOffset.ToString(CultureInfo.InvariantCulture)}" : "";
                TimezoneString = $"UTC{utcHourOffsetString}{utcMinuteOffsetString}";
            }
            else
            {
                // NB: WPF has no timezone abbreviation lookup table.
                if (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now))
                {
                    TimezoneString = !string.IsNullOrEmpty(Preferences.Default.local_timezone_dst_abbreviation) ? Preferences.Default.local_timezone_dst_abbreviation : "?????";
                }
                else
                {
                    TimezoneString = !string.IsNullOrEmpty(Preferences.Default.local_timezone_abbreviation) ? Preferences.Default.local_timezone_abbreviation : "?????";
                }
            }
        }

        #endregion

        #region Update time display

        public async void UpdateLocalClock(object sender, DateTime e)
        {
            if (!ClockUpdatesEnabled || ObsWebsocketConnection.Instance.Client == null || !ObsWebsocketConnection.Instance.Client.CanSend)
            {
                return;
            }
            string localDisplayTime = String.Format(CultureInfo.CurrentCulture, Properties.Resources.obs_time_display_format, e.ToLocalTime().ToString(Properties.Resources.obs_time_string_format, CultureInfo.InvariantCulture), TimezoneString);
            SetTextGDIPlusPropertiesRequestTextPropertyOnly request = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
            {
                Source = Preferences.Default.obs_local_clock_source_name,
                Text = localDisplayTime
            };
            await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(true);
        }

        #endregion

        #region Fetch latest weather data

        public async void FetchWeatherData(object sender, DateTime e)
        {
            // Early return if weather URI isn't set
            if (string.IsNullOrEmpty(Preferences.Default.obs_local_clock_weather_json_url))
            {
                return;
            }
            Uri weatherUri = new Uri(Preferences.Default.obs_local_clock_weather_json_url);
            // Early return if we already have data and aren't fetching this minute
            if (WeatherDataCollection?.Items.Count > 0 && (e.Minute == 0 || e.Minute % Preferences.Default.obs_local_clock_weather_json_url_fetch_delay > 0))
            {
                return;
            }
            // Fetch JSON from URI
            using HttpResponseMessage weatherdataResponse = await httpClient.GetAsync(weatherUri, HttpCompletionOption.ResponseHeadersRead).ConfigureAwait(false);
            try
            {
                weatherdataResponse.EnsureSuccessStatusCode();
            }
            catch (HttpRequestException)
            {
                // How long should old data be used for?
                return;
            }
            // Parse JSON
            if (!(weatherdataResponse.Content.Headers.ContentType.MediaType == "application/json"))
            {
                return;
            }
            var responseStream = await weatherdataResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            try
            {
                WeatherDataCollection = await JsonSerializer.DeserializeAsync<WeatherData>(responseStream).ConfigureAwait(false);
                if (CurrentLocalClockWeatherRecord == 0 && string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1))
                {
                    UpdateLocalWeather(this, DateTime.UtcNow);
                }
            }
            catch (JsonException)
            {
                Trace.WriteLine("Error parsing JSON.");
            }
        }

        #endregion

        #region Update weather display

        public async void UpdateLocalWeather(object sender, DateTime e)
        {
            // Return early if not connected, no weather data, or nothing to change this second
            if (!WeatherUpdatesEnabled ||
                ObsWebsocketConnection.Instance.Client == null ||
                !ObsWebsocketConnection.Instance.Client.CanSend ||
                WeatherDataCollection == null ||
                WeatherDataCollection.Items.Count == 0 ||
                (!string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1) && e.Second % Preferences.Default.obs_local_clock_cycle_delay > 0)
                )
            {
                return;
            }
            // Set hold time (display duration) for local weather to the configured default
            int localWeatherHoldCount = Preferences.Default.obs_local_clock_cycle_delay;
            // Get the total duration of one cycle of weather data locations
            int localWeatherCycleTotalTime = WeatherDataCollection.Items.Count * Preferences.Default.obs_local_clock_cycle_delay;
            // If total duration of a cycle is less than a minute, increase display duration of our location and weather
            //  - i.e. if there are 10 weather data items, and we cycle every 5 seconds, we have a spare 10 seconds
            if (localWeatherCycleTotalTime < 60)
            {
                localWeatherHoldCount += 60 - localWeatherCycleTotalTime;
            }

            // FirstWeatherCycle ends at 0 seconds past the minute
            if (FirstWeatherCycle && e.Second == 0)
            {
                FirstWeatherCycle = false;
                ObsWebsocketConnection.Instance.NotifyPropertyChanged(nameof(FirstWeatherCycle));
            }
            // Return to first record at 0 seconds past the minute
            else if (!FirstWeatherCycle && e.Second == 0)
            {
                CurrentLocalClockWeatherRecord = 0;
            }
            // During FirstWeatherCycle there is nothing to do after initial run of this method
            // After FirstWeatherCycle there is nothing to do if we're holding at first record
            else if ((FirstWeatherCycle && !string.IsNullOrEmpty(PreviousLocalClockWeatherAttrib1)) ||
                (!FirstWeatherCycle && e.Second < localWeatherHoldCount))
            {
                return;
            }
            // After FirstWeatherCycle, switch to next record (we already returned early if e.Second % obs_local_clock_cycle_delay > 0)
            else if (!FirstWeatherCycle && e.Second >= localWeatherHoldCount)
            {
                CurrentLocalClockWeatherRecord++;
            }
            if (CurrentLocalClockWeatherRecord >= WeatherDataCollection.Items.Count)
            {
                return;
            }

            // JSON API text may contain HTML entities - &deg; is easier to type, &#xf002; won't usually look like a cloud, also avoids icon font hard-coding
            string weatherSymbolText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Symbol);
            string weatherTempText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Temperature);
            string weatherLocationText = WebUtility.HtmlDecode(WeatherDataCollection.Items[CurrentLocalClockWeatherRecord].Location);
            // Weather data attribution is currently stored as UTF-16 strings in user preferences
            string weatherAttrib1Text = Preferences.Default.obs_local_clock_weather_attrib1_text;
            string weatherAttrib2Text = Preferences.Default.obs_local_clock_weather_attrib2_text;

            List<SetTextGDIPlusPropertiesRequestTextPropertyOnly> requestList = new List<SetTextGDIPlusPropertiesRequestTextPropertyOnly>();
            
            // Add weather symbol update to list
            if (ObsWebsocketConnection.Instance.ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_symbol_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherSymbol = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_weather_symbol_source_name,
                    Text = weatherSymbolText
                };
                requestList.Add(weatherSymbol);
            }
            // Add weather temperature update to list
            if (ObsWebsocketConnection.Instance.ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_temp_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherTemp = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_weather_temp_source_name,
                    Text = weatherTempText
                };
                requestList.Add(weatherTemp);
            }
            // Add weather location update to list
            if (ObsWebsocketConnection.Instance.ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_location_source_name))
            {
                SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherLocation = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                {
                    Source = Preferences.Default.obs_local_clock_location_source_name,
                    Text = weatherLocationText
                };
                requestList.Add(weatherLocation);
            }
            // Add weather data attribution line 1 update to list (if necessary)
            if (ObsWebsocketConnection.Instance.ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_attrib1_source_name))
            {
                if (weatherAttrib1Text != PreviousLocalClockWeatherAttrib1)
                {
                    SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherAttrib1 = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                    {
                        Source = Preferences.Default.obs_local_clock_weather_attrib1_source_name,
                        Text = weatherAttrib1Text
                    };
                    requestList.Add(weatherAttrib1);
                    PreviousLocalClockWeatherAttrib1 = weatherAttrib1Text;
                    ObsWebsocketConnection.Instance.NotifyPropertyChanged(PreviousLocalClockWeatherAttrib1);
                }
            }
            // Add weather data attribution line 2 update to list (if necessary)
            if (ObsWebsocketConnection.Instance.ObsSourceDictionary.ContainsKey(Preferences.Default.obs_local_clock_weather_attrib2_source_name))
            {
                if (weatherAttrib2Text != PreviousLocalClockWeatherAttrib2)
                {
                    SetTextGDIPlusPropertiesRequestTextPropertyOnly weatherAttrib2 = new SetTextGDIPlusPropertiesRequestTextPropertyOnly()
                    {
                        Source = Preferences.Default.obs_local_clock_weather_attrib2_source_name,
                        Text = weatherAttrib2Text
                    };
                    requestList.Add(weatherAttrib2);
                    PreviousLocalClockWeatherAttrib2 = weatherAttrib2Text;
                    ObsWebsocketConnection.Instance.NotifyPropertyChanged(PreviousLocalClockWeatherAttrib2);
                }
            }

            // Send all update requests in list
            foreach (SetTextGDIPlusPropertiesRequestTextPropertyOnly request in requestList)
            {
                _ = await ObsWebsocketConnection.Instance.Client.ObsSend(request).ConfigureAwait(true);
            }
            requestList.Clear();
        }

        #endregion

        #region dispose

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    httpClient.Dispose();
                    chronoTimer.MinuteChanged -= FetchWeatherData;
                    chronoTimer.MinuteChanged -= UpdateLocalClock;
                    chronoTimer.MinuteChanged -= UpdateLocalWeather;
                    chronoTimer.SecondChanged -= UpdateLocalWeather;
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        // ~WeatherProcessing()
        // {
        //     // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
        //     Dispose(disposing: false);
        // }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
