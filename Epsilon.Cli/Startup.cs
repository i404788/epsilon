﻿using Epsilon.Abstractions.Export;
using Epsilon.Canvas;
using Epsilon.Canvas.Abstractions;
using Epsilon.Export;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Epsilon.Cli;

public class Startup : IHostedService
{
    private readonly ILogger<Startup> _logger;
    private readonly IHostApplicationLifetime _lifetime;
    private readonly ExportOptions _exportOptions;
    private readonly CanvasSettings _canvasSettings;
    private readonly ICanvasModuleCollectionFetcher _collectionFetcher;
    private readonly IModuleExporterCollection _exporterCollection;

    public Startup(
        ILogger<Startup> logger,
        IHostApplicationLifetime lifetime,
        IOptions<CanvasSettings> canvasSettings,
        IOptions<ExportOptions> exportSettings,
        ICanvasModuleCollectionFetcher collectionFetcher,
        IModuleExporterCollection exporterCollection)
    {
        _logger = logger;
        _canvasSettings = canvasSettings.Value;
        _exportOptions = exportSettings.Value;
        _lifetime = lifetime;
        _collectionFetcher = collectionFetcher;
        _exporterCollection = exporterCollection;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        // TODO: Replace exceptions with validation errors
        ValidateOptions();

        _lifetime.ApplicationStarted.Register(() => Task.Run(ExecuteAsync, cancellationToken));

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task ExecuteAsync()
    {
        _logger.LogInformation("Targeting Canvas course: {CourseId}, at {Url}", _canvasSettings.CourseId, _canvasSettings.ApiUrl);
        var modules = await _collectionFetcher.Fetch(_canvasSettings.CourseId);

        _logger.LogInformation("Attempting to use following formats: {Formats}", string.Join(", ", _exportOptions.Formats));
        var exporters = _exporterCollection.DetermineExporters(_exportOptions.Formats).ToArray();

        foreach (var (format, exporter) in exporters)
        {
            _logger.LogInformation("Exporting to {Format} using {Exporter}...", format, exporter.GetType().Name);
            exporter.Export(modules, format);
        }

        _lifetime.StopApplication();
    }

    private void ValidateOptions()
    {
        if (_canvasSettings.CourseId <= 0)
        {
            throw new ArgumentNullException(nameof(_canvasSettings.CourseId));
        }

        if (string.IsNullOrEmpty(_canvasSettings.AccessToken))
        {
            throw new ArgumentNullException(nameof(_canvasSettings.AccessToken));
        }
    }
}