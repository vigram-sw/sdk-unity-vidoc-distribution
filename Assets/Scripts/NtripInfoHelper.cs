using System;
using System.Collections.Generic;
using UnityEngine;
using ViGram;

public class NtripInfo
{
    public NtripConnectionInformation NtripConfig { get; private set; }
    public string Mount { get; private set; }

    public NtripInfo(NtripConnectionInformation ntripConfig, string mount)
    {
        NtripConfig = ntripConfig;
        Mount = mount;
    }

    public override bool Equals(object obj)
    {
        if (obj == null || GetType() != obj.GetType())
        {
            return false;
        }

        NtripInfo other = (NtripInfo)obj;
        return other.NtripConfig.Equals(NtripConfig) && Mount == other.Mount;
    }

    public override int GetHashCode()
    {
        return NtripConfig.GetHashCode() ^ Mount.GetHashCode();
    }
}

public class NtripInfoHelper
{
    [Serializable]
    private sealed class SavedNtripInfoList
    {
        public List<SavedNtripInfo> items = new();
    }

    [Serializable]
    private sealed class SavedNtripInfo
    {
        public SavedNtripConnectionInformation ntripConfig;
        public string mount;
    }

    [Serializable]
    private sealed class SavedNtripConnectionInformation
    {
        public string hostname;
        public int port;
        public string username;
        public string password;
        public bool forceHTTPSconnection;
        public bool forceHTTPSMountpointsConnection;
    }

    private Action<List<NtripInfo>> _action;
    private List<NtripInfo> map = new List<NtripInfo>();

    public void Add(NtripConnectionInformation item, string mount)
    {
        var newNtripInfo = new NtripInfo(item, mount);

        if (!map.Contains(newNtripInfo))
        {
            map.Add(newNtripInfo);
            SaveToPlayerPrefs();
        }
    }

    public void Remove(NtripInfo ntripInfo)
    {
        if (!map.Contains(ntripInfo)) return;

        map.Remove(ntripInfo);
        SaveToPlayerPrefs();
    }

    public NtripInfo Get(int id)
    {
        if (id >= 0 && id < map.Count)
        {
            return map[id];
        }

        throw new Exception("NtripInfo by ID not found.");
    }

    public void GetAll(Action<List<NtripInfo>> action)
    {
        string json = PlayerPrefs.GetString(SHARED_PREFERENCES_KEY, "");

        if (!string.IsNullOrEmpty(json))
        {
            map.Clear();

            var trimmedJson = json.TrimStart();
            var serializedList = trimmedJson.StartsWith("[", StringComparison.Ordinal)
                ? $"{{\"items\":{json}}}"
                : json;
            var savedList = JsonUtility.FromJson<SavedNtripInfoList>(serializedList);

            if (savedList?.items != null)
            {
                foreach (var savedInfo in savedList.items)
                {
                    if (savedInfo?.ntripConfig == null)
                    {
                        continue;
                    }

                    var config = savedInfo.ntripConfig;
                    map.Add(new NtripInfo(
                        new NtripConnectionInformation(
                            config.hostname,
                            config.port,
                            config.username,
                            config.password,
                            config.forceHTTPSconnection,
                            config.forceHTTPSMountpointsConnection),
                        savedInfo.mount));
                }
            }
        }

        _action = action;
        _action?.Invoke(map);
    }

    private void SaveToPlayerPrefs()
    {
        var savedList = new SavedNtripInfoList();
        foreach (var ntripInfo in map)
        {
            var config = ntripInfo.NtripConfig;
            savedList.items.Add(new SavedNtripInfo
            {
                ntripConfig = new SavedNtripConnectionInformation
                {
                    hostname = config.Hostname,
                    port = config.Port,
                    username = config.Username,
                    password = config.Password,
                    forceHTTPSconnection = config.ForceHttpsConnection,
                    forceHTTPSMountpointsConnection = config.ForceHttpsMountpointsConnection
                },
                mount = ntripInfo.Mount
            });
        }

        string json = JsonUtility.ToJson(savedList);
        PlayerPrefs.SetString(SHARED_PREFERENCES_KEY, json);
        PlayerPrefs.Save();

        _action?.Invoke(new List<NtripInfo>(map));
    }

    private const string SHARED_PREFERENCES_KEY = "my_ntrip_information";
}
