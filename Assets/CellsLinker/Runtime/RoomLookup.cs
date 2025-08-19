using System;
using System.Collections.Generic;
using System.Linq;

namespace CellsLinker.Runtime
{
    /// <summary>
    /// Pre-process room for faster lookups when picking candidate rooms during level construction
    /// </summary>
    public class RoomLookup
    {
        /// <summary>
        /// Room candidate list indexed using RoomEdge
        /// </summary>
        List<RoomCandidate>[] _candidates = new List<RoomCandidate>[4];
        public RoomLookup(RoomTemplateCollection collection)
        {
            // Initialize candidate lists
            for (int i = 0; i < 4; i++) _candidates[i] = new List<RoomCandidate>();
            // Process rooms
            foreach (RoomTemplateBase room in collection.Templates)
            {
                AddRoom(room, false);
                // if (room.AllowMirror) AddRoom(room, true);
            }
            // TODO: When the lists are finalized, sort
        }

        private void AddRoom(RoomTemplateBase room, bool mirror)
        {
            if (mirror) throw new NotImplementedException("Adding mirrored rooms not supported yet.");

            // A room can spawn up to 4 candidates without mirroring
            RoomCandidate[] directionCandidates = new RoomCandidate[4];
            for (int i = 0; i < 4; i++) directionCandidates[i] = new RoomCandidate(room);
            // TODO: Add support for mirror
            int entranceCount = 0;
            int exitCount = 0;
            for (int i = 0; i < room.Doors.Count; i++)
            {
                // There are two things we need to do while iterating over doors
                // First, count the "effective exits":
                // ExitCount = count(EntranceOnly, max 1) + count(Exit) - 1
                RoomDoor door = room.Doors[i];
                if (door.Directionality == DoorDirectionality.EntranceOnly) entranceCount = 1;
                else exitCount++;

                // Second, we add entrances to their corresponding RoomCandidates
                if (door.Directionality != DoorDirectionality.ExitOnly)
                {
                    directionCandidates[(int)door.Edge].Entrances.Add(i);
                }
            }
            int effectiveExitCount = entranceCount + exitCount - 1;
            // Finally, we save the local candidate if it has at least one entrance
            for (int i = 0; i < 4; i++)
            {
                if (directionCandidates[i].Entrances.Count == 0) continue;

                directionCandidates[i].ExitCount = effectiveExitCount;
                _candidates[i].Add(directionCandidates[i]);
            }
        }

        /// <summary>
        /// Given a room edge, find all rooms that have at least one entrance on that edge.
        /// </summary>
        /// <param name="edge">The edge where we expect to find entrances</param>
        /// <param name="minExitCount">Filter candidates by the effective exit count</param>
        /// <returns>IEnumerable of RoomCandidate</returns>
        public IEnumerable<RoomCandidate> GetCandidatesByEdge(DoorEdge edge, int minExitCount = 0)
        {
            // TODO: Optimize with sorted lists
            return _candidates[(int)edge].Where(c => c.ExitCount >= minExitCount);
        }
    }

    /// <summary>
    /// Represents a candidate room for connection
    /// </summary>
    public class RoomCandidate
    {
        public RoomTemplateBase Prefab;
        public bool Mirrored;
        /// <summary>
        /// Indices of doors that can be used as an entrance on one side
        /// </summary>
        public List<int> Entrances;
        /// <summary>
        /// This is an *over-estimation* of exit count after an entrance is connected.
        /// Should only be used for quick filtering and requires double-check.
        /// </summary>
        public int ExitCount;

        public RoomCandidate(RoomTemplateBase room, bool mirrored = false)
        {
            Prefab = room;
            Mirrored = mirrored;
            Entrances = new List<int>();
        }
    }
}
