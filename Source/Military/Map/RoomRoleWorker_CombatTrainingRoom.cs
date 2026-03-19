using System.Collections.Generic;
using Verse;

namespace Military
{
    public class RoomRoleWorker_CombatTrainingRoom : RoomRoleWorker
    {
        public override float GetScore(Room room)
        {
            int dummyCount = 0;
            IReadOnlyList<Thing> contained = room.ContainedAndAdjacentThings;
            for (int i = 0; i < contained.Count; i++)
            {
                if (contained[i] is Building_CombatDummy)
                    dummyCount++;
            }
            return dummyCount > 0 ? dummyCount * 50f : 0f;
        }
    }
}
