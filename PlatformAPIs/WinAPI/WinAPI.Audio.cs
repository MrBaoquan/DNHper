using System;
using System.Runtime.InteropServices;

namespace DNHper
{
    public static partial class WinAPI
    {
        #region Audio Operations
        private static bool InitializeAudio()
        {
            if (_audioInitialized && _audioEndpointVolume != null)
                return true;
            try
            {
                Ole32.CoInitialize(IntPtr.Zero);
                var clsid = AudioConstants.CLSID_MMDeviceEnumerator;
                var iid = AudioConstants.IID_IMMDeviceEnumerator;
                int hr = Ole32.CoCreateInstance(ref clsid, IntPtr.Zero, AudioConstants.CLSCTX_ALL, ref iid, out IntPtr deviceEnumeratorPtr);
                if (hr != 0 || deviceEnumeratorPtr == IntPtr.Zero)
                    return false;
                _deviceEnumerator = Marshal.GetObjectForIUnknown(deviceEnumeratorPtr) as IMMDeviceEnumerator;
                Marshal.Release(deviceEnumeratorPtr);
                if (_deviceEnumerator == null)
                    return false;
                hr = _deviceEnumerator.GetDefaultAudioEndpoint(DataFlow.Render, Role.Multimedia, out IMMDevice device);
                if (hr != 0 || device == null)
                    return false;
                var audioEndpointVolumeIid = AudioConstants.IID_IAudioEndpointVolume;
                hr = device.Activate(ref audioEndpointVolumeIid, AudioConstants.CLSCTX_ALL, IntPtr.Zero, out IntPtr audioEndpointVolumePtr);
                if (hr != 0 || audioEndpointVolumePtr == IntPtr.Zero)
                    return false;
                _audioEndpointVolume = Marshal.GetObjectForIUnknown(audioEndpointVolumePtr) as IAudioEndpointVolume;
                Marshal.Release(audioEndpointVolumePtr);
                _audioInitialized = _audioEndpointVolume != null;
                return _audioInitialized;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"音频初始化失败: {ex.Message}");
                return false;
            }
        }

        public static void CleanupAudio()
        {
            try
            {
                if (_audioEndpointVolume != null)
                {
                    Marshal.ReleaseComObject(_audioEndpointVolume);
                    _audioEndpointVolume = null;
                }
                if (_deviceEnumerator != null)
                {
                    Marshal.ReleaseComObject(_deviceEnumerator);
                    _deviceEnumerator = null;
                }
                _audioInitialized = false;
                Ole32.CoUninitialize();
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"音频清理失败: {ex.Message}");
            }
        }

        public static bool SetMasterVolume(float volume)
        {
            if (!InitializeAudio())
                return false;
            try
            {
                volume = Math.Max(0.0f, Math.Min(1.0f, volume));
                var guid = AudioConstants.GUID_NULL;
                return _audioEndpointVolume.SetMasterVolumeLevelScalar(volume, ref guid) == 0;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"设置音量失败: {ex.Message}");
                return false;
            }
        }

        public static float GetMasterVolume()
        {
            if (!InitializeAudio())
                return -1;
            try
            {
                return _audioEndpointVolume.GetMasterVolumeLevelScalar(out float volume) == 0 ? volume : -1;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"获取音量失败: {ex.Message}");
                return -1;
            }
        }

        public static bool SetMasterVolumePercent(int volumePercent) => SetMasterVolume(Math.Max(0, Math.Min(100, volumePercent)) / 100.0f);

        public static int GetMasterVolumePercent()
        {
            float volume = GetMasterVolume();
            return volume >= 0 ? (int)Math.Round(volume * 100) : -1;
        }

        public static bool SetMute(bool mute)
        {
            if (!InitializeAudio())
            {
                UnityEngine.Debug.LogWarning("音频系统未初始化，无法设置静音状态");
                return false;
            }

            try
            {
                var guid = AudioConstants.GUID_NULL;
                int hr = _audioEndpointVolume.SetMute(mute, ref guid);
                if (hr != 0)
                {
                    UnityEngine.Debug.LogError($"SetMute 调用失败, HRESULT: 0x{hr:X8}");
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"设置静音失败: {ex.Message}");
                return false;
            }
        }

        public static bool? GetMute()
        {
            if (!InitializeAudio())
                return null;
            try
            {
                int hr = _audioEndpointVolume.GetMute(out bool mute);
                if (hr != 0)
                {
                    UnityEngine.Debug.LogError($"GetMute 调用失败, HRESULT: 0x{hr:X8}");
                    return null;
                }
                return mute;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"获取静音状态失败: {ex.Message}");
                return null;
            }
        }

        public static bool? ToggleMute()
        {
            bool? currentMute = GetMute();
            if (currentMute == null)
                return null;
            bool newMute = !currentMute.Value;
            return SetMute(newMute) ? newMute : null;
        }

        public static bool VolumeStepUp()
        {
            if (!InitializeAudio())
                return false;
            try
            {
                var guid = AudioConstants.GUID_NULL;
                return _audioEndpointVolume.VolumeStepUp(ref guid) == 0;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"音量步进增加失败: {ex.Message}");
                return false;
            }
        }

        public static bool VolumeStepDown()
        {
            if (!InitializeAudio())
                return false;
            try
            {
                var guid = AudioConstants.GUID_NULL;
                return _audioEndpointVolume.VolumeStepDown(ref guid) == 0;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"音量步进减少失败: {ex.Message}");
                return false;
            }
        }

        public static (uint currentStep, uint totalSteps)? GetVolumeStepInfo()
        {
            if (!InitializeAudio())
                return null;
            try
            {
                return _audioEndpointVolume.GetVolumeStepInfo(out uint currentStep, out uint totalSteps) == 0
                    ? (currentStep, totalSteps)
                    : null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"获取音量步进信息失败: {ex.Message}");
                return null;
            }
        }

        public static (float minDB, float maxDB, float incrementDB)? GetVolumeRange()
        {
            if (!InitializeAudio())
                return null;
            try
            {
                return _audioEndpointVolume.GetVolumeRange(out float minDB, out float maxDB, out float incrementDB) == 0
                    ? (minDB, maxDB, incrementDB)
                    : null;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"获取音量范围失败: {ex.Message}");
                return null;
            }
        }

        public static AudioSystemInfo GetAudioSystemInfo() =>
            new AudioSystemInfo
            {
                IsInitialized = _audioInitialized,
                Volume = GetMasterVolume(),
                VolumePercent = GetMasterVolumePercent(),
                IsMuted = GetMute(),
                VolumeStepInfo = GetVolumeStepInfo(),
                VolumeRange = GetVolumeRange()
            };

        public static bool SmartVolumeControl(VolumeAction action, float value = 0) =>
            action switch
            {
                VolumeAction.SetVolume => SetMasterVolume(value),
                VolumeAction.SetVolumePercent => SetMasterVolumePercent((int)value),
                VolumeAction.Mute => SetMute(true),
                VolumeAction.Unmute => SetMute(false),
                VolumeAction.ToggleMute => ToggleMute() != null,
                VolumeAction.VolumeUp => VolumeStepUp(),
                VolumeAction.VolumeDown => VolumeStepDown(),
                _ => false
            };
        #endregion
    }
}
