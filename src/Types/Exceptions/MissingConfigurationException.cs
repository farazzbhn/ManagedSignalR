﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManagedLib.ManagedSignalR.Types.Exceptions;

public class MissingConfigurationException(string message) : Exception(message);