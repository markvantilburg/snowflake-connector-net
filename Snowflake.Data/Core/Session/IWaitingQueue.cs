using System.Threading;

namespace Snowflake.Data.Core.Session
{
    internal interface IWaitingQueue
    {
        bool Wait(int millisecondsTimeout, CancellationToken cancellationToken);

        void OnResourceIncrease();

        bool IsAnyoneWaiting();

        bool IsWaitingEnabled();
    }
}