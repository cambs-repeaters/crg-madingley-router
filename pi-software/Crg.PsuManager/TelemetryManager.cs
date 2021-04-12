﻿using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Crg.PsuManager
{
    public class TelemetryManager
    {
        private readonly IMqttClientOptions options;
        private readonly IMqttClient mqtt;
        private readonly string topicPrefix = "crg/psumanager/madingley/";

        public PowerManager PowerManager { get; set; }
        public AdcManager AdcManager { get; set; }
        public TemperatureManager TemperatureManager { get; set; }

        public TelemetryManager()
        {
            options = new MqttClientOptionsBuilder()
                .WithTcpServer("localhost")
                .Build();

            mqtt = new MqttFactory().CreateMqttClient();
            mqtt.ConnectAsync(options, CancellationToken.None).Wait();
            PublishMessage("online", new { status = "online" });
        }

        private void PublishMessage(string topic, object payload)
        {
            if (!mqtt.IsConnected)
                mqtt.ConnectAsync(options, CancellationToken.None).Wait();
            string payloadJson = System.Text.Json.JsonSerializer.Serialize(payload);
            mqtt.PublishAsync(new MqttApplicationMessage { Topic = topicPrefix + topic, Payload = Encoding.UTF8.GetBytes(payloadJson) }).Wait();
        }

        public void PublishTelemetry()
        {
            dynamic t = new
            {
                powerGood = PowerManager.PowerGood,
                batteryVoltage = AdcManager.BatteryVoltage,
                supplyVoltage = AdcManager.MainSupplyVoltage,
                poeVoltage = AdcManager.PoeVoltage,
                piVoltage = AdcManager.PiVoltage,
                onBoardTemperature = TemperatureManager.OnBoardTemperature,
            };
            PublishMessage("telemetry", t);
        }
    }
}
