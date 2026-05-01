using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;

namespace GameLauncherApp.Core
{
    /// <summary>
    /// Handles the persistent storage of game profiles using JSON serialization.
    /// </summary>
    public static class ProfileManager
    {
        private static readonly string ProfilesPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "profiles.json");
        private static readonly JsonSerializerOptions Options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        /// <summary>
        /// Saves the list of game profiles to a JSON file.
        /// </summary>
        /// <param name="profiles">The list of profiles to serialize.</param>
        public static void SaveProfiles(List<GameProfile> profiles)
        {
            try
            {
                string jsonString = JsonSerializer.Serialize(profiles, Options);
                File.WriteAllText(ProfilesPath, jsonString);
                Logger.LogInfo($"Successfully saved {profiles.Count} profiles to {ProfilesPath}");
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to save profiles to JSON", ex);
            }
        }

        /// <summary>
        /// Loads the list of game profiles from the JSON file.
        /// </summary>
        /// <returns>A list of GameProfile objects, or an empty list if the file is missing or corrupt.</returns>
        public static List<GameProfile> LoadProfiles()
        {
            try
            {
                if (!File.Exists(ProfilesPath))
                {
                    Logger.LogInfo("Profiles file not found. Returning empty list.");
                    return new List<GameProfile>();
                }

                string jsonString = File.ReadAllText(ProfilesPath);
                List<GameProfile> profiles = JsonSerializer.Deserialize<List<GameProfile>>(jsonString);

                if (profiles == null)
                {
                    Logger.LogWarning("Deserialization returned null. Initializing empty profile list.");
                    return new List<GameProfile>();
                }

                Logger.LogInfo($"Successfully loaded {profiles.Count} profiles from {ProfilesPath}");
                return profiles;
            }
            catch (JsonException jex)
            {
                Logger.LogError("JSON parsing error in profiles file", jex);
                return new List<GameProfile>();
            }
            catch (Exception ex)
            {
                Logger.LogError("Failed to load profiles from disk", ex);
                return new List<GameProfile>();
            }
        }
    }
}
