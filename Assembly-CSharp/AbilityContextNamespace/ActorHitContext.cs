// ROGUES
// SERVER
using UnityEngine;

namespace AbilityContextNamespace
{
    public class ActorHitContext
    {
        public Vector3 m_hitOrigin;
        public bool m_ignoreMinCoverDist;
        public bool m_inRangeForTargeter;
        // public int m_ordinal; // rogues
        public ContextVars m_contextVars = new ContextVars();
        // public List<int> m_targeterIndices = new List<int>(); // rogues
        // public List<int> m_segmentIndices = new List<int>(); // rogues

        public void Reset()
        {
            m_hitOrigin = Vector3.zero;
            m_contextVars.ClearData();
            m_inRangeForTargeter = false;
            m_ignoreMinCoverDist = false;
            // m_ordinal = 0; // rogues
            // m_targeterIndices.Clear(); // rogues
            // m_segmentIndices.Clear(); // rogues
        }

        // rogues?
        // public ActorHitContext Clone()
        // {
        //     ActorHitContext actorHitContext = (ActorHitContext)MemberwiseClone();
        //     actorHitContext.m_contextVars = m_contextVars.Clone();
        //     return actorHitContext;
        // }

        // rogues
        // public void AddTargeterIndex(int targeterIndex)
        // {
        //     if (!m_targeterIndices.Contains(targeterIndex))
        //     {
        //         m_targeterIndices.Add(targeterIndex);
        //     }
        // }

        // rogues
        // public void AddSegmentIndex(int segmentIndex)
        // {
        //     if (!m_segmentIndices.Contains(segmentIndex))
        //     {
        //         m_segmentIndices.Add(segmentIndex);
        //     }
        // }
    }
}