﻿using Mediator;
using Users.Core.Dto;

namespace Users.Core.Commands;

public record SignUp(string UserName, string Email, string Password, bool IsNotificationsNeeded, Guid? RoleGid) : ICommand<JwtDto>;