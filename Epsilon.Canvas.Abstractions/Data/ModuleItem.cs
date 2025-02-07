﻿using System.Text.Json.Serialization;

namespace Epsilon.Canvas.Abstractions.Data;

public record ModuleItem(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
    ModuleItemType? Type,
    [property: JsonPropertyName("content_id")] int? ContentId
);