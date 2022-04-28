using Arbor.AppModel.Messaging;

namespace Arbor.Hypermedia;

public interface ICommandResult<out T> : ICommandResult where T : IMetadata
{
    T Result { get; }
}