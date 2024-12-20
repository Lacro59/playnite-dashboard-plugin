﻿using Playnite.SDK;
using Playnite.SDK.Data;
using CommonPluginsShared;
using System;
using System.Collections.Generic;
using GameActivity.Services;

namespace GameActivity.Models
{
    public class Activity : ObservableObject
    {
        private ActivityDatabase PluginDatabase => GameActivity.PluginDatabase;


        public Guid SourceID { get; set; }
        public List<Guid> PlatformIDs { get; set; }

        [SerializationPropertyName("GameActionName")]
        public string _gameActionName;
        [DontSerialize]
        public string GameActionName
        {
            get => _gameActionName.IsNullOrEmpty() ? ResourceProvider.GetString("LOCGameActivityDefaultAction") : _gameActionName;
            set => _gameActionName = value;
        }

        [DontSerialize]
        public string SourceName => PlayniteTools.GetSourceBySourceIdOrPlatformId(SourceID, PlatformIDs);

        public int IdConfiguration { get; set; } = -1;

        [DontSerialize]
        public SystemConfiguration Configuration
        {
            get
            {
                if (IdConfiguration == -1)
                {
                    return new SystemConfiguration();
                }

                if (IdConfiguration >= (PluginDatabase.LocalSystem.GetConfigurations()?.Count ?? 0))
                {
                    return new SystemConfiguration();
                }

                return PluginDatabase.LocalSystem.GetConfigurations()[IdConfiguration];
            }
        }

        public DateTime? DateSession { get; set; }

        public ulong ElapsedSeconds { get; set; } = 0;
    }
}
