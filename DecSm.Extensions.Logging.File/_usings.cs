global using System.Collections.Concurrent;
global using System.Diagnostics;
global using System.IO.Abstractions;
global using System.Runtime.CompilerServices;
global using System.Text;
global using System.Threading.Channels;
global using DecSm.Extensions.Logging.File.Configuration;
global using DecSm.Extensions.Logging.File.Provider;
global using DecSm.Extensions.Logging.File.Writer;
global using JetBrains.Annotations;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.DependencyInjection.Extensions;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Logging.Configuration;
global using Microsoft.Extensions.Options;

[assembly: InternalsVisibleTo("DecSm.Extensions.Logging.File.Tests")]
