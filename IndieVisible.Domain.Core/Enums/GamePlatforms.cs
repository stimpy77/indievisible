﻿using IndieVisible.Domain.Core.Attributes;

namespace IndieVisible.Domain.Core.Enums
{
    public enum GamePlatforms
    {
        [UiInfo(Class = "windows")]
        Windows,
        [UiInfo(Class = "linux")]
        Linux,
        [UiInfo(Class = "xbox")]
        Xbox,
        [UiInfo(Class = "playstation")]
        Playstation,
        [UiInfo(Class = "nintendo-switch")]
        NintendoSwitch,
        [UiInfo(Class = "android")]
        Android,
        [UiInfo(Class = "ios")]
        Ios,
        [UiInfo(Class = "steam")]
        Steam
    }
}
