﻿using System;
using Nimator.Settings;
using Nimator.Util;

namespace Nimator.Notifiers.Opsgenie
{
    /// <inheritdoc/>
    public class OpsGenieAlertConverter : IOpsGenieAlertConverter
    {
        private const int MaxOpsgenieTagLength = 50;
        private const int MaxOpsgenieMessageLength = 130;
        private const int MaxOpsgenieDescriptionLength = 15000;
        
        private readonly OpsGenieSettings settings;

        /// <summary>
        /// Constructs a settings based <see cref="IOpsGenieAlertConverter"/>.
        /// </summary>
        /// <param name="settings">Settings for the alert</param>
        public OpsGenieAlertConverter(OpsGenieSettings settings)
        {
            if (settings == null) throw new ArgumentNullException(nameof(settings));
            if (string.IsNullOrWhiteSpace(settings.TeamName)) throw new ArgumentException("settings.TeamName was not set", nameof(settings.TeamName));

            this.settings = settings;
        }

        /// <inheritdoc/>
        public OpsGenieAlertRequest Convert(INimatorResult result)
        {
            if (result == null) throw new ArgumentNullException(nameof(result));

            var message = string.IsNullOrEmpty(result.Message) ?
                "Unknown message" :
                result.Message.Truncate(MaxOpsgenieMessageLength);

            var failingLayerName = result.GetFirstFailedLayerName() ?? "UnknownLayer";
            
            return new OpsGenieAlertRequest(message)
            {                
                Alias = "nimator-failure",
                Description = result.RenderPlainText(settings.Threshold).Truncate(MaxOpsgenieDescriptionLength),                
                Responders = new[]
                {
                    new OpsGenieResponder
                    {
                        Type = OpsGenieResponderType.Team,
                        Name = settings.TeamName
                    }
                },
                Tags = new[] { "Nimator", failingLayerName.Truncate(MaxOpsgenieTagLength) }
            };
        }
    }
}
