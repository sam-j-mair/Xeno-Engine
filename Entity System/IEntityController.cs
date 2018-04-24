using System;
using XenoEngine.Network;

namespace XenoEngine
{
    public interface IEntityController
    {
        Entity  CreateEntity(Type type, Entity parentEntity, Object[] createParams, int nSubIndex = 0);
        Entity  CreateEntity(string szEntityName, Entity parentEntity, Object[] createParams, int nSubIndex = 0);
        TEntity CreateEntity<TEntity>(Entity parentEntity, Object[] createParams, int nSubIndex = 0) where TEntity : Entity;
        TEntity CreateEntity<TEntity>(string szEntityName, Entity parentEntity, Object[] createParams, int nSubIndex = 0) where TEntity : Entity;
        void    RequestDeletion(Entity entity);
        void    DestroyEntity(Entity entity);
        Entity  GetEntity(int nReference, int nSubIndex = 0);
        bool    IsAuthoritative(Entity entity);
        void    FlushSharedData();
        void    MessageReceived(NetworkDataPacket packet);
        void    ProcessClientConnection(int nMaskIndex);
        void    CopyEntitiesToPeers();
    }
}
