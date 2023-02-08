﻿using Shared.Abstractions.AdditionalAbstractions;
using Shared.Contexts.Contexts;

namespace Shared.Messaging;

public record MessageEnvelope<TEvent>(TEvent Message, MessageContext Context) where TEvent : IEvent;