using System;
using System.Collections.Concurrent;
using AEngine;

namespace TokyoServer
{
    public class Server : ServerBase<Session>
    {
        public ConcurrentDictionary<string, Session> UserMap = new ConcurrentDictionary<string, Session>();

        public override void OnAccept(Session session)
        {
            session.Guid = Guid.NewGuid().ToString("N");
            UserMap.TryAdd(session.Guid, session);
        }

        public override void OnDisconnect(Session session)
        {
            Session tmp;
            UserMap.TryRemove(session.Guid, out tmp);
        }
    }
}