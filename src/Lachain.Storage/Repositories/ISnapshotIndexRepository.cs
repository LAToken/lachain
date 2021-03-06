using Lachain.Storage.State;

namespace Lachain.Storage.Repositories
{
    public interface ISnapshotIndexRepository
    {
        IBlockchainSnapshot GetSnapshotForBlock(ulong block);
        void SaveSnapshotForBlock(ulong block, IBlockchainSnapshot snapshot);
    }
}