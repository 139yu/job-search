using Commons.Enums;
using NLog;

namespace Commons.Logging;

public class Log
{
    public static NLog.Logger For<T>(LogModule module) => For(module, typeof(T));
    public static NLog.Logger For(LogModule module, Type type) =>
        LogManager.GetLogger(type.FullName ?? type.Name).WithProperty("Module", module.ToString());
}