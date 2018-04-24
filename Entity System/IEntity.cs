
using System;
using System.Collections.Generic;

namespace XenoEngine
{
    public interface IEntity
    {
        /// <summary>
        /// Add an entity as a child of this entity.
        /// </summary>
        /// <param name="entity">entity to make child.</param>
        void AddChild(Entity entity);
        /// <summary>
        /// remove a child entity from this entity.
        /// </summary>
        /// <param name="entity">entity to remove</param>
        void RemoveChild(Entity entity);
        /// <summary>
        /// send a message between entities.
        /// </summary>
        /// <param name="szMessageName">the message to be called.</param>
        /// <param name="entity">the entity that will receive the message</param>
        /// <param name="msgData">data for the message.</param>
        /// <param name="IsNetworkMessage">should this be push over the network</param>
        /// <returns>returns a message result.</returns>
        EntityMsgRslt SendMessage(String szMessageName, Entity entity, object msgData = null, bool IsNetworkMessage = false);
        /// <summary>
        /// gets the entity identity struct.
        /// </summary>
        /// <returns></returns>
        EntityIdentification GetIdentStruct();

        /// <summary>
        /// gets this entity parents id
        /// </summary>
        /// <returns>parent id</returns>
        int GetParentID();
        /// <summary>
        /// gets a list of all the children of this entity.
        /// </summary>
        /// <returns>a list of entities</returns>
        List<int> GetChildList();
        /// <summary>
        /// check if this entity is authoritative across the network.
        /// </summary>
        /// <returns>true or false.</returns>
        bool IsAuthoritative();
    }
}
