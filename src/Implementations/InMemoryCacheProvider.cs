using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManagedLib.ManagedSignalR.Abstractions;
using ManagedLib.ManagedSignalR.Helper;

namespace ManagedLib.ManagedSignalR.Implementations;
public class InMemoryCacheProvider : ICacheProvider
{
    private static readonly ConcurrentDictionary<string, dynamic> ConnectionCache = new();

}
